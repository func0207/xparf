namespace Xparf.Core.Entities;

public sealed class RefreshToken : AuditableEntity
{
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedReason { get; set; }
    public bool IsRevoked => RevokedAt.HasValue;
}
