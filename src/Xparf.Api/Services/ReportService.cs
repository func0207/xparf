using Microsoft.EntityFrameworkCore;
using Xparf.Api.Contracts.Reports;
using Xparf.Core.Abstractions;
using Xparf.Core.Enums;
using Xparf.Infrastructure.Persistence;

namespace Xparf.Api.Services;

public interface IReportService
{
    Task<DashboardSummaryResponse> GetDashboardSummaryAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken);
    Task<IReadOnlyList<StockReportResponse>> GetStockReportAsync(long? branchId, CancellationToken cancellationToken);
    Task<SalesReportResponse> GetSalesReportAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken);
    Task<PurchaseReportResponse> GetPurchaseReportAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken);
}

public sealed class ReportService(XparfDbContext dbContext, ICurrentUserContext currentUserContext) : IReportService
{
    public async Task<DashboardSummaryResponse> GetDashboardSummaryAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        var start = from ?? DateTime.UtcNow.Date;
        var end = to ?? DateTime.UtcNow.Date.AddDays(1).AddTicks(-1);
        var sales = dbContext.Sales.Where(x => x.CompanyId == companyId && x.Status == SaleStatus.Posted && x.SaleDate >= start && x.SaleDate <= end);
        var purchases = dbContext.Purchases.Where(x => x.CompanyId == companyId && x.Status == PurchaseStatus.Posted && x.PurchaseDate >= start && x.PurchaseDate <= end);
        var lowStock = await dbContext.BranchItems.CountAsync(x => x.CompanyId == companyId && x.QuantityOnHand <= x.MinimumStock, cancellationToken);
        var coinBalance = await dbContext.Companies.Where(x => x.Id == companyId).Select(x => x.CoinBalance).FirstAsync(cancellationToken);
        return new DashboardSummaryResponse(await sales.SumAsync(x => x.Total, cancellationToken), await purchases.SumAsync(x => x.Total, cancellationToken), await sales.CountAsync(cancellationToken), await purchases.CountAsync(cancellationToken), lowStock, coinBalance);
    }

    public async Task<IReadOnlyList<StockReportResponse>> GetStockReportAsync(long? branchId, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        return await dbContext.BranchItems
            .Include(x => x.Branch)
            .Include(x => x.Item)
            .Where(x => x.CompanyId == companyId && (branchId == null || x.BranchId == branchId))
            .OrderBy(x => x.Branch.Name).ThenBy(x => x.Item.Name)
            .Select(x => new StockReportResponse(x.BranchId, x.Branch.Name, x.ItemId, x.Item.Sku, x.Item.Name, x.QuantityOnHand, x.MinimumStock, x.AverageCost, x.SellingPrice, x.QuantityOnHand <= x.MinimumStock))
            .ToListAsync(cancellationToken);
    }

    public async Task<SalesReportResponse> GetSalesReportAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        var start = from ?? DateTime.UtcNow.Date;
        var end = to ?? DateTime.UtcNow.Date.AddDays(1).AddTicks(-1);
        var rows = await dbContext.Sales
            .Where(x => x.CompanyId == companyId && x.Status == SaleStatus.Posted && x.SaleDate >= start && x.SaleDate <= end)
            .OrderByDescending(x => x.SaleDate)
            .Select(x => new SalesReportRowResponse(x.Id, x.SaleNumber, x.SaleDate, x.BranchId, x.Total, x.PaidAmount, x.OutstandingAmount, x.PaymentStatus))
            .ToListAsync(cancellationToken);
        return new SalesReportResponse(start, end, rows.Count, rows.Sum(x => x.Total), rows.Sum(x => x.PaidAmount), rows.Sum(x => x.OutstandingAmount), rows);
    }

    public async Task<PurchaseReportResponse> GetPurchaseReportAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        var start = from ?? DateTime.UtcNow.Date;
        var end = to ?? DateTime.UtcNow.Date.AddDays(1).AddTicks(-1);
        var rows = await dbContext.Purchases
            .Where(x => x.CompanyId == companyId && x.Status == PurchaseStatus.Posted && x.PurchaseDate >= start && x.PurchaseDate <= end)
            .OrderByDescending(x => x.PurchaseDate)
            .Select(x => new PurchaseReportRowResponse(x.Id, x.PurchaseNumber, x.PurchaseDate, x.BranchId, x.Total, x.PaidAmount, x.OutstandingAmount, x.PaymentStatus))
            .ToListAsync(cancellationToken);
        return new PurchaseReportResponse(start, end, rows.Count, rows.Sum(x => x.Total), rows.Sum(x => x.PaidAmount), rows.Sum(x => x.OutstandingAmount), rows);
    }

    private long GetCompanyId() => currentUserContext.CompanyId ?? throw new UnauthorizedAccessException("Company context tidak ditemukan.");
}
