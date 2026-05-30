using Microsoft.EntityFrameworkCore;
using Xparf.Api.Contracts.Inventory;
using Xparf.Core.Abstractions;
using Xparf.Core.Entities;
using Xparf.Core.Enums;
using Xparf.Infrastructure.Persistence;

namespace Xparf.Api.Services;

public interface IInventoryService
{
    Task<IReadOnlyList<BranchItemResponse>> GetBranchItemsAsync(CancellationToken cancellationToken);
    Task<BranchItemResponse> GetBranchItemAsync(long id, CancellationToken cancellationToken);
    Task<BranchItemResponse> CreateBranchItemAsync(CreateBranchItemRequest request, CancellationToken cancellationToken);
    Task<BranchItemResponse> UpdateBranchItemAsync(long id, UpdateBranchItemRequest request, CancellationToken cancellationToken);
    Task DeleteBranchItemAsync(long id, CancellationToken cancellationToken);
    Task<IReadOnlyList<StockLedgerResponse>> GetStockLedgersAsync(long? branchId, long? itemId, CancellationToken cancellationToken);
    Task<StockLedgerResponse> CreateStockAdjustmentAsync(StockAdjustmentRequest request, CancellationToken cancellationToken);
}

public sealed class InventoryService(XparfDbContext dbContext, ICurrentUserContext currentUserContext) : IInventoryService
{
    public async Task<IReadOnlyList<BranchItemResponse>> GetBranchItemsAsync(CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        return await dbContext.BranchItems.Include(x => x.Item).Where(x => x.CompanyId == companyId).OrderBy(x => x.BranchId).ThenBy(x => x.Item.Sku).Select(x => ToBranchItemResponse(x)).ToListAsync(cancellationToken);
    }

    public async Task<BranchItemResponse> GetBranchItemAsync(long id, CancellationToken cancellationToken) => ToBranchItemResponse(await GetBranchItemEntityAsync(GetCompanyId(), id, cancellationToken));

    public async Task<BranchItemResponse> CreateBranchItemAsync(CreateBranchItemRequest request, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        await ValidateBranchAndItemAsync(companyId, request.BranchId, request.ItemId, cancellationToken);
        if (await dbContext.BranchItems.AnyAsync(x => x.CompanyId == companyId && x.BranchId == request.BranchId && x.ItemId == request.ItemId, cancellationToken)) throw new InvalidOperationException("Item branch sudah ada.");
        var branchItem = new BranchItem { CompanyId = companyId, BranchId = request.BranchId, ItemId = request.ItemId, MinimumStock = request.MinimumStock, AverageCost = request.AverageCost, LastCost = request.LastCost, SellingPrice = request.SellingPrice, WholesalePrice = request.WholesalePrice, IsAvailable = request.IsAvailable };
        dbContext.BranchItems.Add(branchItem);
        await dbContext.SaveChangesAsync(cancellationToken);
        await dbContext.Entry(branchItem).Reference(x => x.Item).LoadAsync(cancellationToken);
        return ToBranchItemResponse(branchItem);
    }

    public async Task<BranchItemResponse> UpdateBranchItemAsync(long id, UpdateBranchItemRequest request, CancellationToken cancellationToken)
    {
        var branchItem = await GetBranchItemEntityAsync(GetCompanyId(), id, cancellationToken);
        branchItem.MinimumStock = request.MinimumStock;
        branchItem.AverageCost = request.AverageCost;
        branchItem.LastCost = request.LastCost;
        branchItem.SellingPrice = request.SellingPrice;
        branchItem.WholesalePrice = request.WholesalePrice;
        branchItem.IsAvailable = request.IsAvailable;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToBranchItemResponse(branchItem);
    }

    public async Task DeleteBranchItemAsync(long id, CancellationToken cancellationToken)
    {
        var branchItem = await GetBranchItemEntityAsync(GetCompanyId(), id, cancellationToken);
        if (branchItem.QuantityOnHand != 0) throw new InvalidOperationException("Item branch masih punya stok, tidak bisa dihapus.");
        dbContext.BranchItems.Remove(branchItem);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StockLedgerResponse>> GetStockLedgersAsync(long? branchId, long? itemId, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        var query = dbContext.StockLedgers.Where(x => x.CompanyId == companyId);
        if (branchId is not null) query = query.Where(x => x.BranchId == branchId);
        if (itemId is not null) query = query.Where(x => x.ItemId == itemId);
        return await query.OrderByDescending(x => x.CreatedAt).Take(200).Select(x => ToStockLedgerResponse(x)).ToListAsync(cancellationToken);
    }

    public async Task<StockLedgerResponse> CreateStockAdjustmentAsync(StockAdjustmentRequest request, CancellationToken cancellationToken)
    {
        if (request.QuantityChange == 0) throw new InvalidOperationException("Quantity adjustment tidak boleh 0.");
        var companyId = GetCompanyId();
        await ValidateBranchAndItemAsync(companyId, request.BranchId, request.ItemId, cancellationToken);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var branchItem = await dbContext.BranchItems.FirstOrDefaultAsync(x => x.CompanyId == companyId && x.BranchId == request.BranchId && x.ItemId == request.ItemId, cancellationToken);
        if (branchItem is null)
        {
            branchItem = new BranchItem { CompanyId = companyId, BranchId = request.BranchId, ItemId = request.ItemId, IsAvailable = true, LastCost = request.UnitCost, AverageCost = request.UnitCost };
            dbContext.BranchItems.Add(branchItem);
        }
        var nextBalance = branchItem.QuantityOnHand + request.QuantityChange;
        if (nextBalance < 0) throw new InvalidOperationException("Stok tidak boleh minus.");
        branchItem.QuantityOnHand = nextBalance;
        branchItem.LastCost = request.UnitCost;
        var ledger = new StockLedger { CompanyId = companyId, BranchId = request.BranchId, ItemId = request.ItemId, MovementType = StockMovementType.Adjustment, ReferenceType = "StockAdjustment", QuantityIn = request.QuantityChange > 0 ? request.QuantityChange : 0, QuantityOut = request.QuantityChange < 0 ? Math.Abs(request.QuantityChange) : 0, BalanceAfter = nextBalance, UnitCost = request.UnitCost, Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim() };
        dbContext.StockLedgers.Add(ledger);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ToStockLedgerResponse(ledger);
    }

    private long GetCompanyId() => currentUserContext.CompanyId ?? throw new UnauthorizedAccessException("Company context tidak ditemukan.");
    private async Task<BranchItem> GetBranchItemEntityAsync(long companyId, long id, CancellationToken cancellationToken) => await dbContext.BranchItems.Include(x => x.Item).FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Id == id, cancellationToken) ?? throw new InvalidOperationException("Item branch tidak ditemukan.");
    private async Task ValidateBranchAndItemAsync(long companyId, long branchId, long itemId, CancellationToken cancellationToken)
    {
        if (!await dbContext.Branches.AnyAsync(x => x.CompanyId == companyId && x.Id == branchId, cancellationToken)) throw new InvalidOperationException("Branch tidak ditemukan.");
        if (!await dbContext.Items.AnyAsync(x => x.CompanyId == companyId && x.Id == itemId, cancellationToken)) throw new InvalidOperationException("Item tidak ditemukan.");
    }
    private static BranchItemResponse ToBranchItemResponse(BranchItem x) => new(x.Id, x.BranchId, x.ItemId, x.Item.Sku, x.Item.Name, x.QuantityOnHand, x.MinimumStock, x.AverageCost, x.LastCost, x.SellingPrice, x.WholesalePrice, x.IsAvailable);
    private static StockLedgerResponse ToStockLedgerResponse(StockLedger x) => new(x.Id, x.BranchId, x.ItemId, x.MovementType, x.ReferenceType, x.ReferenceId, x.QuantityIn, x.QuantityOut, x.BalanceAfter, x.UnitCost, x.Note, x.CreatedAt);
}
