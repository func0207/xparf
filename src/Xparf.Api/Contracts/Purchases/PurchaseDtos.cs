using Xparf.Core.Enums;

namespace Xparf.Api.Contracts.Purchases;

public sealed record PurchaseResponse(long Id, long BranchId, long? SupplierId, string PurchaseNumber, DateTime PurchaseDate, decimal Subtotal, decimal Discount, decimal Tax, decimal Total, decimal PaidAmount, decimal OutstandingAmount, PurchaseStatus Status, PaymentStatus PaymentStatus, IReadOnlyList<PurchaseLineResponse> Lines, IReadOnlyList<PurchasePaymentResponse> Payments);
public sealed record PurchaseLineResponse(long Id, long ItemId, string? Description, decimal Quantity, string Unit, decimal UnitCost, decimal Discount, decimal LineTotal);
public sealed record PurchasePaymentResponse(long Id, DateTime PaymentDate, decimal Amount, PaymentMethod PaymentMethod, string? Note);
public sealed record CreatePurchaseRequest(long BranchId, long? SupplierId, string PurchaseNumber, DateTime PurchaseDate, decimal Discount, decimal Tax);
public sealed record UpdatePurchaseRequest(long? SupplierId, DateTime PurchaseDate, decimal Discount, decimal Tax);
public sealed record CreatePurchaseLineRequest(long ItemId, string? Description, decimal Quantity, string Unit, decimal UnitCost, decimal Discount);
public sealed record UpdatePurchaseLineRequest(string? Description, decimal Quantity, string Unit, decimal UnitCost, decimal Discount);
public sealed record CreatePurchasePaymentRequest(DateTime PaymentDate, decimal Amount, PaymentMethod PaymentMethod, string? Note);
