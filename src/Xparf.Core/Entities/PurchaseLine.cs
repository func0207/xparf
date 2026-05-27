namespace Xparf.Core.Entities;

public sealed class PurchaseLine : AuditableEntity
{
    public long PurchaseId { get; set; }
    public Purchase Purchase { get; set; } = null!;
    public long ItemId { get; set; }
    public Item Item { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal UnitCost { get; set; }
    public decimal Discount { get; set; }
    public decimal LineTotal { get; set; }
}
