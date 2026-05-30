using Microsoft.EntityFrameworkCore;
using Xparf.Api.Contracts.Sales;
using Xparf.Core.Abstractions;
using Xparf.Core.Entities;
using Xparf.Core.Enums;
using Xparf.Infrastructure.Persistence;

namespace Xparf.Api.Services;

public interface ISaleService
{
    Task<IReadOnlyList<SaleResponse>> GetSalesAsync(CancellationToken cancellationToken);
    Task<SaleResponse> GetSaleAsync(long id, CancellationToken cancellationToken);
    Task<SaleResponse> CreateSaleAsync(CreateSaleRequest request, CancellationToken cancellationToken);
    Task<SaleResponse> UpdateSaleAsync(long id, UpdateSaleRequest request, CancellationToken cancellationToken);
    Task DeleteSaleAsync(long id, CancellationToken cancellationToken);
    Task<SaleResponse> AddLineAsync(long id, CreateSaleLineRequest request, CancellationToken cancellationToken);
    Task<SaleResponse> UpdateLineAsync(long id, long lineId, UpdateSaleLineRequest request, CancellationToken cancellationToken);
    Task<SaleResponse> DeleteLineAsync(long id, long lineId, CancellationToken cancellationToken);
    Task<SaleResponse> PostSaleAsync(long id, CancellationToken cancellationToken);
    Task<SaleResponse> AddPaymentAsync(long id, CreateSalePaymentRequest request, CancellationToken cancellationToken);
    Task<SaleResponse> VoidSaleAsync(long id, CancellationToken cancellationToken);
}

public sealed class SaleService(XparfDbContext dbContext, ICurrentUserContext currentUserContext) : ISaleService
{
    public async Task<IReadOnlyList<SaleResponse>> GetSalesAsync(CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        return await dbContext.Sales.Include(x => x.Lines).Include(x => x.Payments).Where(x => x.CompanyId == companyId).OrderByDescending(x => x.SaleDate).ThenByDescending(x => x.Id).Select(x => ToResponse(x)).ToListAsync(cancellationToken);
    }
    public async Task<SaleResponse> GetSaleAsync(long id, CancellationToken cancellationToken) => ToResponse(await GetSaleEntityAsync(GetCompanyId(), id, cancellationToken));
    public async Task<SaleResponse> CreateSaleAsync(CreateSaleRequest request, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            var existing = await dbContext.Sales.Include(x => x.Lines).Include(x => x.Payments).FirstOrDefaultAsync(x => x.CompanyId == companyId && x.IdempotencyKey == request.IdempotencyKey, cancellationToken);
            if (existing is not null) return ToResponse(existing);
        }
        await ValidateBranchAndCustomerAsync(companyId, request.BranchId, request.CustomerId, cancellationToken);
        var number = request.SaleNumber.Trim().ToUpperInvariant();
        if (await dbContext.Sales.AnyAsync(x => x.CompanyId == companyId && x.SaleNumber == number, cancellationToken)) throw new InvalidOperationException("Nomor sale sudah ada.");
        var sale = new Sale { CompanyId = companyId, BranchId = request.BranchId, CustomerId = request.CustomerId, SaleNumber = number, SaleDate = request.SaleDate, SaleType = request.SaleType, Discount = request.Discount, Tax = request.Tax, IdempotencyKey = string.IsNullOrWhiteSpace(request.IdempotencyKey) ? null : request.IdempotencyKey.Trim() };
        RecalculateSale(sale);
        dbContext.Sales.Add(sale);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(sale);
    }
    public async Task<SaleResponse> UpdateSaleAsync(long id, UpdateSaleRequest request, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        var sale = await GetSaleEntityAsync(companyId, id, cancellationToken);
        EnsureDraft(sale);
        await ValidateBranchAndCustomerAsync(companyId, sale.BranchId, request.CustomerId, cancellationToken);
        sale.CustomerId = request.CustomerId; sale.SaleDate = request.SaleDate; sale.SaleType = request.SaleType; sale.Discount = request.Discount; sale.Tax = request.Tax;
        RecalculateSale(sale);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(sale);
    }
    public async Task DeleteSaleAsync(long id, CancellationToken cancellationToken)
    {
        var sale = await GetSaleEntityAsync(GetCompanyId(), id, cancellationToken);
        EnsureDraft(sale);
        dbContext.Sales.Remove(sale);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
    public async Task<SaleResponse> AddLineAsync(long id, CreateSaleLineRequest request, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId(); var sale = await GetSaleEntityAsync(companyId, id, cancellationToken); EnsureDraft(sale); await ValidateItemAsync(companyId, request.ItemId, cancellationToken);
        sale.Lines.Add(new SaleLine { ItemId = request.ItemId, Description = Normalize(request.Description), Quantity = request.Quantity, Unit = request.Unit.Trim(), UnitPrice = request.UnitPrice, Discount = request.Discount, LineTotal = CalcLineTotal(request.Quantity, request.UnitPrice, request.Discount) });
        RecalculateSale(sale); await dbContext.SaveChangesAsync(cancellationToken); return ToResponse(sale);
    }
    public async Task<SaleResponse> UpdateLineAsync(long id, long lineId, UpdateSaleLineRequest request, CancellationToken cancellationToken)
    {
        var sale = await GetSaleEntityAsync(GetCompanyId(), id, cancellationToken); EnsureDraft(sale);
        var line = sale.Lines.FirstOrDefault(x => x.Id == lineId) ?? throw new InvalidOperationException("Line sale tidak ditemukan.");
        line.Description = Normalize(request.Description); line.Quantity = request.Quantity; line.Unit = request.Unit.Trim(); line.UnitPrice = request.UnitPrice; line.Discount = request.Discount; line.LineTotal = CalcLineTotal(request.Quantity, request.UnitPrice, request.Discount);
        RecalculateSale(sale); await dbContext.SaveChangesAsync(cancellationToken); return ToResponse(sale);
    }
    public async Task<SaleResponse> DeleteLineAsync(long id, long lineId, CancellationToken cancellationToken)
    {
        var sale = await GetSaleEntityAsync(GetCompanyId(), id, cancellationToken); EnsureDraft(sale);
        var line = sale.Lines.FirstOrDefault(x => x.Id == lineId) ?? throw new InvalidOperationException("Line sale tidak ditemukan.");
        dbContext.SaleLines.Remove(line); RecalculateSale(sale); await dbContext.SaveChangesAsync(cancellationToken); return ToResponse(sale);
    }
    public async Task<SaleResponse> PostSaleAsync(long id, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId(); var sale = await GetSaleEntityAsync(companyId, id, cancellationToken); EnsureDraft(sale);
        if (sale.Lines.Count == 0) throw new InvalidOperationException("Sale belum punya line.");
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var company = await dbContext.Companies.FirstAsync(x => x.Id == companyId, cancellationToken);
        var coinDeduction = await GetSaleCoinDeductionAsync(cancellationToken);
        if (company.CoinBalance < coinDeduction) throw new InvalidOperationException("Coin balance tidak cukup untuk posting sale.");
        foreach (var line in sale.Lines)
        {
            var branchItem = await dbContext.BranchItems.FirstOrDefaultAsync(x => x.CompanyId == companyId && x.BranchId == sale.BranchId && x.ItemId == line.ItemId, cancellationToken) ?? throw new InvalidOperationException($"Stok item {line.ItemId} tidak ditemukan di branch.");
            if (branchItem.QuantityOnHand < line.Quantity) throw new InvalidOperationException($"Stok item {line.ItemId} tidak cukup.");
            branchItem.QuantityOnHand -= line.Quantity;
            dbContext.StockLedgers.Add(new StockLedger { CompanyId = companyId, BranchId = sale.BranchId, ItemId = line.ItemId, MovementType = StockMovementType.Sale, ReferenceType = "Sale", ReferenceId = sale.Id, QuantityOut = line.Quantity, BalanceAfter = branchItem.QuantityOnHand, UnitCost = branchItem.AverageCost, Note = sale.SaleNumber });
        }
        if (coinDeduction > 0)
        {
            var before = company.CoinBalance; company.CoinBalance -= coinDeduction; sale.CoinDeducted = coinDeduction;
            dbContext.CoinLedgers.Add(new CoinLedger { CompanyId = companyId, TransactionType = CoinTransactionType.SaleDeduction, ReferenceType = "Sale", ReferenceId = sale.Id, CoinOut = coinDeduction, BalanceBefore = before, BalanceAfter = company.CoinBalance, Note = sale.SaleNumber });
        }
        sale.Status = SaleStatus.Posted;
        await dbContext.SaveChangesAsync(cancellationToken); await transaction.CommitAsync(cancellationToken); return ToResponse(sale);
    }
    public async Task<SaleResponse> AddPaymentAsync(long id, CreateSalePaymentRequest request, CancellationToken cancellationToken)
    {
        var sale = await GetSaleEntityAsync(GetCompanyId(), id, cancellationToken); if (sale.Status == SaleStatus.Voided) throw new InvalidOperationException("Sale void tidak bisa dibayar.");
        sale.Payments.Add(new SalePayment { PaymentDate = request.PaymentDate, Amount = request.Amount, PaymentMethod = request.PaymentMethod, Note = Normalize(request.Note) });
        sale.PaidAmount += request.Amount; RecalculateSale(sale); await dbContext.SaveChangesAsync(cancellationToken); return ToResponse(sale);
    }
    public async Task<SaleResponse> VoidSaleAsync(long id, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        var sale = await GetSaleEntityAsync(companyId, id, cancellationToken);
        if (sale.Status == SaleStatus.Voided) return ToResponse(sale);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        if (sale.Status == SaleStatus.Posted)
        {
            foreach (var line in sale.Lines)
            {
                var branchItem = await dbContext.BranchItems.FirstOrDefaultAsync(x => x.CompanyId == companyId && x.BranchId == sale.BranchId && x.ItemId == line.ItemId, cancellationToken)
                    ?? throw new InvalidOperationException($"Stok item {line.ItemId} tidak ditemukan di branch.");
                branchItem.QuantityOnHand += line.Quantity;
                dbContext.StockLedgers.Add(new StockLedger { CompanyId = companyId, BranchId = sale.BranchId, ItemId = line.ItemId, MovementType = StockMovementType.SaleVoid, ReferenceType = "SaleVoid", ReferenceId = sale.Id, QuantityIn = line.Quantity, BalanceAfter = branchItem.QuantityOnHand, UnitCost = branchItem.AverageCost, Note = sale.SaleNumber });
            }
            if (sale.CoinDeducted > 0)
            {
                var company = await dbContext.Companies.FirstAsync(x => x.Id == companyId, cancellationToken);
                var before = company.CoinBalance;
                company.CoinBalance += sale.CoinDeducted;
                dbContext.CoinLedgers.Add(new CoinLedger { CompanyId = companyId, TransactionType = CoinTransactionType.Refund, ReferenceType = "SaleVoid", ReferenceId = sale.Id, CoinIn = sale.CoinDeducted, BalanceBefore = before, BalanceAfter = company.CoinBalance, Note = sale.SaleNumber });
            }
        }
        sale.Status = SaleStatus.Voided;
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ToResponse(sale);
    }
    private long GetCompanyId() => currentUserContext.CompanyId ?? throw new UnauthorizedAccessException("Company context tidak ditemukan.");
    private async Task<Sale> GetSaleEntityAsync(long companyId, long id, CancellationToken cancellationToken) => await dbContext.Sales.Include(x => x.Lines).Include(x => x.Payments).FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Id == id, cancellationToken) ?? throw new InvalidOperationException("Sale tidak ditemukan.");
    private async Task ValidateBranchAndCustomerAsync(long companyId, long branchId, long? customerId, CancellationToken cancellationToken)
    { if (!await dbContext.Branches.AnyAsync(x => x.CompanyId == companyId && x.Id == branchId, cancellationToken)) throw new InvalidOperationException("Branch tidak ditemukan."); if (customerId is not null && !await dbContext.Customers.AnyAsync(x => x.CompanyId == companyId && x.Id == customerId, cancellationToken)) throw new InvalidOperationException("Customer tidak ditemukan."); }
    private async Task ValidateItemAsync(long companyId, long itemId, CancellationToken cancellationToken) { if (!await dbContext.Items.AnyAsync(x => x.CompanyId == companyId && x.Id == itemId, cancellationToken)) throw new InvalidOperationException("Item tidak ditemukan."); }
    private async Task<decimal> GetSaleCoinDeductionAsync(CancellationToken cancellationToken)
    { var enabled = await dbContext.PlatformSettings.FirstOrDefaultAsync(x => x.Key == "coin.enable_deduction", cancellationToken); if (enabled?.Value.Equals("false", StringComparison.OrdinalIgnoreCase) == true) return 0; var setting = await dbContext.PlatformSettings.FirstOrDefaultAsync(x => x.Key == "coin.sale_posted_deduction", cancellationToken); return decimal.TryParse(setting?.Value, out var result) ? result : 0; }
    private static void EnsureDraft(Sale sale) { if (sale.Status != SaleStatus.Draft) throw new InvalidOperationException("Sale bukan draft."); }
    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static decimal CalcLineTotal(decimal quantity, decimal unitPrice, decimal discount) => (quantity * unitPrice) - discount;
    private static void RecalculateSale(Sale sale) { sale.Subtotal = sale.Lines.Sum(x => x.LineTotal); sale.Total = sale.Subtotal - sale.Discount + sale.Tax; sale.ChangeAmount = Math.Max(0, sale.PaidAmount - sale.Total); sale.OutstandingAmount = Math.Max(0, sale.Total - sale.PaidAmount); sale.PaymentStatus = sale.PaidAmount <= 0 ? PaymentStatus.Unpaid : sale.PaidAmount >= sale.Total ? PaymentStatus.Paid : PaymentStatus.Partial; }
    private static SaleResponse ToResponse(Sale x) => new(x.Id, x.BranchId, x.CustomerId, x.SaleNumber, x.SaleDate, x.SaleType, x.Subtotal, x.Discount, x.Tax, x.Total, x.PaidAmount, x.ChangeAmount, x.OutstandingAmount, x.CoinDeducted, x.Status, x.PaymentStatus, x.Lines.OrderBy(l => l.Id).Select(l => new SaleLineResponse(l.Id, l.ItemId, l.Description, l.Quantity, l.Unit, l.UnitPrice, l.Discount, l.LineTotal)).ToList(), x.Payments.OrderBy(p => p.PaymentDate).Select(p => new SalePaymentResponse(p.Id, p.PaymentDate, p.Amount, p.PaymentMethod, p.Note)).ToList());
}
