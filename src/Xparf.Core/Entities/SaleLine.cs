namespace Xparf.Core.Entities;

public sealed class SaleLine : AuditableEntity
{
    public long SaleId { get; set; }
    public Sale Sale { get; set; } = null!;
    public long ItemId { get; set; }
    public Item Item { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal LineTotal { get; set; }
}
