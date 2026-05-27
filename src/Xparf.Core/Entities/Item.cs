using Xparf.Core.Enums;

namespace Xparf.Core.Entities;

public sealed class Item : AuditableEntity
{
    public long CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public string Sku { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ItemType ItemType { get; set; }
    public string BaseUnit { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsDiscontinued { get; set; }

    public ICollection<BranchItem> BranchItems { get; set; } = new List<BranchItem>();
}
