using Microsoft.EntityFrameworkCore;
using Xparf.Api.Contracts.Purchases;
using Xparf.Core.Abstractions;
using Xparf.Core.Entities;
using Xparf.Core.Enums;
using Xparf.Infrastructure.Persistence;

namespace Xparf.Api.Services;

public interface IPurchaseService
{
    Task<IReadOnlyList<PurchaseResponse>> GetPurchasesAsync(CancellationToken cancellationToken);
    Task<PurchaseResponse> GetPurchaseAsync(long id, CancellationToken cancellationToken);
    Task<PurchaseResponse> CreatePurchaseAsync(CreatePurchaseRequest request, CancellationToken cancellationToken);
    Task<PurchaseResponse> UpdatePurchaseAsync(long id, UpdatePurchaseRequest request, CancellationToken cancellationToken);
    Task DeletePurchaseAsync(long id, CancellationToken cancellationToken);
    Task<PurchaseResponse> AddLineAsync(long id, CreatePurchaseLineRequest request, CancellationToken cancellationToken);
    Task<PurchaseResponse> UpdateLineAsync(long id, long lineId, UpdatePurchaseLineRequest request, CancellationToken cancellationToken);
    Task<PurchaseResponse> DeleteLineAsync(long id, long lineId, CancellationToken cancellationToken);
    Task<PurchaseResponse> PostPurchaseAsync(long id, CancellationToken cancellationToken);
    Task<PurchaseResponse> AddPaymentAsync(long id, CreatePurchasePaymentRequest request, CancellationToken cancellationToken);
    Task<PurchaseResponse> CancelPurchaseAsync(long id, CancellationToken cancellationToken);
}

public sealed class PurchaseService(XparfDbContext dbContext, ICurrentUserContext currentUserContext) : IPurchaseService
{
    public async Task<IReadOnlyList<PurchaseResponse>> GetPurchasesAsync(CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        return await dbContext.Purchases.Include(x => x.Lines).Include(x => x.Payments).Where(x => x.CompanyId == companyId).OrderByDescending(x => x.PurchaseDate).ThenByDescending(x => x.Id).Select(x => ToResponse(x)).ToListAsync(cancellationToken);
    }

    public async Task<PurchaseResponse> GetPurchaseAsync(long id, CancellationToken cancellationToken) => ToResponse(await GetPurchaseEntityAsync(GetCompanyId(), id, cancellationToken));

    public async Task<PurchaseResponse> CreatePurchaseAsync(CreatePurchaseRequest request, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        await ValidateBranchAndSupplierAsync(companyId, request.BranchId, request.SupplierId, cancellationToken);
        var number = request.PurchaseNumber.Trim().ToUpperInvariant();
        if (await dbContext.Purchases.AnyAsync(x => x.CompanyId == companyId && x.PurchaseNumber == number, cancellationToken)) throw new InvalidOperationException("Nomor purchase sudah ada.");
        var purchase = new Purchase { CompanyId = companyId, BranchId = request.BranchId, SupplierId = request.SupplierId, PurchaseNumber = number, PurchaseDate = request.PurchaseDate, Discount = request.Discount, Tax = request.Tax };
        RecalculatePurchase(purchase);
        dbContext.Purchases.Add(purchase);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(purchase);
    }

    public async Task<PurchaseResponse> UpdatePurchaseAsync(long id, UpdatePurchaseRequest request, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        var purchase = await GetPurchaseEntityAsync(companyId, id, cancellationToken);
        EnsureDraft(purchase);
        await ValidateBranchAndSupplierAsync(companyId, purchase.BranchId, request.SupplierId, cancellationToken);
        purchase.SupplierId = request.SupplierId;
        purchase.PurchaseDate = request.PurchaseDate;
        purchase.Discount = request.Discount;
        purchase.Tax = request.Tax;
        RecalculatePurchase(purchase);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(purchase);
    }

    public async Task DeletePurchaseAsync(long id, CancellationToken cancellationToken)
    {
        var purchase = await GetPurchaseEntityAsync(GetCompanyId(), id, cancellationToken);
        EnsureDraft(purchase);
        dbContext.Purchases.Remove(purchase);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PurchaseResponse> AddLineAsync(long id, CreatePurchaseLineRequest request, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        var purchase = await GetPurchaseEntityAsync(companyId, id, cancellationToken);
        EnsureDraft(purchase);
        await ValidateItemAsync(companyId, request.ItemId, cancellationToken);
        purchase.Lines.Add(new PurchaseLine { ItemId = request.ItemId, Description = Normalize(request.Description), Quantity = request.Quantity, Unit = request.Unit.Trim(), UnitCost = request.UnitCost, Discount = request.Discount, LineTotal = CalcLineTotal(request.Quantity, request.UnitCost, request.Discount) });
        RecalculatePurchase(purchase);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(purchase);
    }

    public async Task<PurchaseResponse> UpdateLineAsync(long id, long lineId, UpdatePurchaseLineRequest request, CancellationToken cancellationToken)
    {
        var purchase = await GetPurchaseEntityAsync(GetCompanyId(), id, cancellationToken);
        EnsureDraft(purchase);
        var line = purchase.Lines.FirstOrDefault(x => x.Id == lineId) ?? throw new InvalidOperationException("Line purchase tidak ditemukan.");
        line.Description = Normalize(request.Description);
        line.Quantity = request.Quantity;
        line.Unit = request.Unit.Trim();
        line.UnitCost = request.UnitCost;
        line.Discount = request.Discount;
        line.LineTotal = CalcLineTotal(request.Quantity, request.UnitCost, request.Discount);
        RecalculatePurchase(purchase);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(purchase);
    }

    public async Task<PurchaseResponse> DeleteLineAsync(long id, long lineId, CancellationToken cancellationToken)
    {
        var purchase = await GetPurchaseEntityAsync(GetCompanyId(), id, cancellationToken);
        EnsureDraft(purchase);
        var line = purchase.Lines.FirstOrDefault(x => x.Id == lineId) ?? throw new InvalidOperationException("Line purchase tidak ditemukan.");
        dbContext.PurchaseLines.Remove(line);
        RecalculatePurchase(purchase);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(purchase);
    }

    public async Task<PurchaseResponse> PostPurchaseAsync(long id, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        var purchase = await GetPurchaseEntityAsync(companyId, id, cancellationToken);
        EnsureDraft(purchase);
        if (purchase.Lines.Count == 0) throw new InvalidOperationException("Purchase belum punya line.");
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        foreach (var line in purchase.Lines)
        {
            var branchItem = await dbContext.BranchItems.FirstOrDefaultAsync(x => x.CompanyId == companyId && x.BranchId == purchase.BranchId && x.ItemId == line.ItemId, cancellationToken);
            if (branchItem is null)
            {
                branchItem = new BranchItem { CompanyId = companyId, BranchId = purchase.BranchId, ItemId = line.ItemId, IsAvailable = true };
                dbContext.BranchItems.Add(branchItem);
            }
            branchItem.QuantityOnHand += line.Quantity;
            branchItem.LastCost = line.UnitCost;
            branchItem.AverageCost = line.UnitCost;
            dbContext.StockLedgers.Add(new StockLedger { CompanyId = companyId, BranchId = purchase.BranchId, ItemId = line.ItemId, MovementType = StockMovementType.Purchase, ReferenceType = "Purchase", ReferenceId = purchase.Id, QuantityIn = line.Quantity, BalanceAfter = branchItem.QuantityOnHand, UnitCost = line.UnitCost, Note = purchase.PurchaseNumber });
        }
        purchase.Status = PurchaseStatus.Posted;
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ToResponse(purchase);
    }

    public async Task<PurchaseResponse> AddPaymentAsync(long id, CreatePurchasePaymentRequest request, CancellationToken cancellationToken)
    {
        var purchase = await GetPurchaseEntityAsync(GetCompanyId(), id, cancellationToken);
        if (purchase.Status == PurchaseStatus.Cancelled) throw new InvalidOperationException("Purchase cancelled tidak bisa dibayar.");
        purchase.Payments.Add(new PurchasePayment { PaymentDate = request.PaymentDate, Amount = request.Amount, PaymentMethod = request.PaymentMethod, Note = Normalize(request.Note) });
        purchase.PaidAmount += request.Amount;
        purchase.OutstandingAmount = Math.Max(0, purchase.Total - purchase.PaidAmount);
        purchase.PaymentStatus = purchase.PaidAmount <= 0 ? PaymentStatus.Unpaid : purchase.PaidAmount >= purchase.Total ? PaymentStatus.Paid : PaymentStatus.Partial;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(purchase);
    }

    public async Task<PurchaseResponse> CancelPurchaseAsync(long id, CancellationToken cancellationToken)
    {
        var purchase = await GetPurchaseEntityAsync(GetCompanyId(), id, cancellationToken);
        if (purchase.Status == PurchaseStatus.Posted) throw new InvalidOperationException("Purchase posted belum bisa dicancel otomatis karena perlu reversal stok.");
        purchase.Status = PurchaseStatus.Cancelled;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(purchase);
    }

    private long GetCompanyId() => currentUserContext.CompanyId ?? throw new UnauthorizedAccessException("Company context tidak ditemukan.");
    private async Task<Purchase> GetPurchaseEntityAsync(long companyId, long id, CancellationToken cancellationToken) => await dbContext.Purchases.Include(x => x.Lines).Include(x => x.Payments).FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Id == id, cancellationToken) ?? throw new InvalidOperationException("Purchase tidak ditemukan.");
    private async Task ValidateBranchAndSupplierAsync(long companyId, long branchId, long? supplierId, CancellationToken cancellationToken)
    {
        if (!await dbContext.Branches.AnyAsync(x => x.CompanyId == companyId && x.Id == branchId, cancellationToken)) throw new InvalidOperationException("Branch tidak ditemukan.");
        if (supplierId is not null && !await dbContext.Suppliers.AnyAsync(x => x.CompanyId == companyId && x.Id == supplierId, cancellationToken)) throw new InvalidOperationException("Supplier tidak ditemukan.");
    }
    private async Task ValidateItemAsync(long companyId, long itemId, CancellationToken cancellationToken)
    {
        if (!await dbContext.Items.AnyAsync(x => x.CompanyId == companyId && x.Id == itemId, cancellationToken)) throw new InvalidOperationException("Item tidak ditemukan.");
    }
    private static void EnsureDraft(Purchase purchase) { if (purchase.Status != PurchaseStatus.Draft) throw new InvalidOperationException("Purchase bukan draft."); }
    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static decimal CalcLineTotal(decimal quantity, decimal unitCost, decimal discount) => (quantity * unitCost) - discount;
    private static void RecalculatePurchase(Purchase purchase)
    {
        purchase.Subtotal = purchase.Lines.Sum(x => x.LineTotal);
        purchase.Total = purchase.Subtotal - purchase.Discount + purchase.Tax;
        purchase.OutstandingAmount = Math.Max(0, purchase.Total - purchase.PaidAmount);
        purchase.PaymentStatus = purchase.PaidAmount <= 0 ? PaymentStatus.Unpaid : purchase.PaidAmount >= purchase.Total ? PaymentStatus.Paid : PaymentStatus.Partial;
    }
    private static PurchaseResponse ToResponse(Purchase x) => new(x.Id, x.BranchId, x.SupplierId, x.PurchaseNumber, x.PurchaseDate, x.Subtotal, x.Discount, x.Tax, x.Total, x.PaidAmount, x.OutstandingAmount, x.Status, x.PaymentStatus, x.Lines.OrderBy(l => l.Id).Select(l => new PurchaseLineResponse(l.Id, l.ItemId, l.Description, l.Quantity, l.Unit, l.UnitCost, l.Discount, l.LineTotal)).ToList(), x.Payments.OrderBy(p => p.PaymentDate).Select(p => new PurchasePaymentResponse(p.Id, p.PaymentDate, p.Amount, p.PaymentMethod, p.Note)).ToList());
}
