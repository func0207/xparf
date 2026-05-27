namespace Xparf.Core.Entities;

public sealed class UserBranch
{
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    public long BranchId { get; set; }
    public Branch Branch { get; set; } = null!;
    public bool IsDefault { get; set; }
}
