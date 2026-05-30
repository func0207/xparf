using Xparf.Core.Enums;

namespace Xparf.Api.Contracts.Reports;

public sealed record DashboardSummaryResponse(
    decimal SalesTotal,
    decimal PurchaseTotal,
    int SalesCount,
    int PurchaseCount,
    int LowStockCount,
    decimal CoinBalance);

public sealed record StockReportResponse(
    long BranchId,
    string BranchName,
    long ItemId,
    string Sku,
    string ItemName,
    decimal QuantityOnHand,
    decimal MinimumStock,
    decimal AverageCost,
    decimal SellingPrice,
    bool IsLowStock);

public sealed record SalesReportRowResponse(long Id, string SaleNumber, DateTime SaleDate, long BranchId, decimal Total, decimal PaidAmount, decimal OutstandingAmount, PaymentStatus PaymentStatus);
public sealed record SalesReportResponse(DateTime From, DateTime To, int Count, decimal Total, decimal PaidAmount, decimal OutstandingAmount, IReadOnlyList<SalesReportRowResponse> Rows);

public sealed record PurchaseReportRowResponse(long Id, string PurchaseNumber, DateTime PurchaseDate, long BranchId, decimal Total, decimal PaidAmount, decimal OutstandingAmount, PaymentStatus PaymentStatus);
public sealed record PurchaseReportResponse(DateTime From, DateTime To, int Count, decimal Total, decimal PaidAmount, decimal OutstandingAmount, IReadOnlyList<PurchaseReportRowResponse> Rows);
