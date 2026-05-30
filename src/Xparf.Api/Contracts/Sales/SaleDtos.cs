using Xparf.Core.Enums;

namespace Xparf.Api.Contracts.Sales;

public sealed record SaleResponse(long Id, long BranchId, long? CustomerId, string SaleNumber, DateTime SaleDate, SaleType SaleType, decimal Subtotal, decimal Discount, decimal Tax, decimal Total, decimal PaidAmount, decimal ChangeAmount, decimal OutstandingAmount, decimal CoinDeducted, SaleStatus Status, PaymentStatus PaymentStatus, IReadOnlyList<SaleLineResponse> Lines, IReadOnlyList<SalePaymentResponse> Payments);
public sealed record SaleLineResponse(long Id, long ItemId, string? Description, decimal Quantity, string Unit, decimal UnitPrice, decimal Discount, decimal LineTotal);
public sealed record SalePaymentResponse(long Id, DateTime PaymentDate, decimal Amount, PaymentMethod PaymentMethod, string? Note);
public sealed record CreateSaleRequest(long BranchId, long? CustomerId, string SaleNumber, DateTime SaleDate, SaleType SaleType, decimal Discount, decimal Tax, string? IdempotencyKey);
public sealed record UpdateSaleRequest(long? CustomerId, DateTime SaleDate, SaleType SaleType, decimal Discount, decimal Tax);
public sealed record CreateSaleLineRequest(long ItemId, string? Description, decimal Quantity, string Unit, decimal UnitPrice, decimal Discount);
public sealed record UpdateSaleLineRequest(string? Description, decimal Quantity, string Unit, decimal UnitPrice, decimal Discount);
public sealed record CreateSalePaymentRequest(DateTime PaymentDate, decimal Amount, PaymentMethod PaymentMethod, string? Note);
