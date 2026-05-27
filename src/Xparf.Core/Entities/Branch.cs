namespace Xparf.Core.Entities;

public sealed class Branch : AuditableEntity
{
    public long CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<BranchItem> BranchItems { get; set; } = new List<BranchItem>();
    public ICollection<UserBranch> UserBranches { get; set; } = new List<UserBranch>();
}
