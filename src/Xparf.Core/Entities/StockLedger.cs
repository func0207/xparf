using Xparf.Core.Enums;

namespace Xparf.Core.Entities;

public sealed class StockLedger : AuditableEntity
{
    public long CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public long BranchId { get; set; }
    public Branch Branch { get; set; } = null!;
    public long ItemId { get; set; }
    public Item Item { get; set; } = null!;

    public StockMovementType MovementType { get; set; }
    public string ReferenceType { get; set; } = string.Empty;
    public long? ReferenceId { get; set; }
    public decimal QuantityIn { get; set; }
    public decimal QuantityOut { get; set; }
    public decimal BalanceAfter { get; set; }
    public decimal UnitCost { get; set; }
    public string? Note { get; set; }
}
