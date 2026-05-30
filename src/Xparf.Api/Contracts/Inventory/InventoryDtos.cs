using Xparf.Core.Enums;

namespace Xparf.Api.Contracts.Inventory;

public sealed record BranchItemResponse(long Id, long BranchId, long ItemId, string ItemSku, string ItemName, decimal QuantityOnHand, decimal MinimumStock, decimal AverageCost, decimal LastCost, decimal SellingPrice, decimal WholesalePrice, bool IsAvailable);
public sealed record CreateBranchItemRequest(long BranchId, long ItemId, decimal MinimumStock, decimal AverageCost, decimal LastCost, decimal SellingPrice, decimal WholesalePrice, bool IsAvailable);
public sealed record UpdateBranchItemRequest(decimal MinimumStock, decimal AverageCost, decimal LastCost, decimal SellingPrice, decimal WholesalePrice, bool IsAvailable);
public sealed record StockLedgerResponse(long Id, long BranchId, long ItemId, StockMovementType MovementType, string ReferenceType, long? ReferenceId, decimal QuantityIn, decimal QuantityOut, decimal BalanceAfter, decimal UnitCost, string? Note, DateTime CreatedAt);
public sealed record StockAdjustmentRequest(long BranchId, long ItemId, decimal QuantityChange, decimal UnitCost, string? Note);
