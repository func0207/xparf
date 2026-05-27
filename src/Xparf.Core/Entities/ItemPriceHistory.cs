namespace Xparf.Core.Entities;

public sealed class ItemPriceHistory : AuditableEntity
{
    public long CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public long BranchId { get; set; }
    public Branch Branch { get; set; } = null!;
    public long ItemId { get; set; }
    public Item Item { get; set; } = null!;
    public decimal OldSellingPrice { get; set; }
    public decimal NewSellingPrice { get; set; }
    public decimal OldWholesalePrice { get; set; }
    public decimal NewWholesalePrice { get; set; }
    public string? Reason { get; set; }
}
