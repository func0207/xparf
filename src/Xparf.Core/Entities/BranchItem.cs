namespace Xparf.Core.Entities;

public sealed class BranchItem : AuditableEntity
{
    public long CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public long BranchId { get; set; }
    public Branch Branch { get; set; } = null!;
    public long ItemId { get; set; }
    public Item Item { get; set; } = null!;

    public decimal QuantityOnHand { get; set; }
    public decimal MinimumStock { get; set; }
    public decimal AverageCost { get; set; }
    public decimal LastCost { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal WholesalePrice { get; set; }
    public bool IsAvailable { get; set; } = true;
}
