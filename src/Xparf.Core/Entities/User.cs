namespace Xparf.Core.Entities;

public sealed class User : AuditableEntity
{
    public long CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public long RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public bool IsOwner { get; set; }
    public bool IsSuperAdmin { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    public ICollection<UserBranch> UserBranches { get; set; } = new List<UserBranch>();
}
