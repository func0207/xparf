namespace Xparf.Core.Entities;

public sealed class TopupPackage : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal MoneyAmount { get; set; }
    public decimal CoinAmount { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
