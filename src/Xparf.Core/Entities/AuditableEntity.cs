namespace Xparf.Core.Entities;

public abstract class AuditableEntity
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? UpdatedByUserId { get; set; }
    public DateTime? DeletedAt { get; set; }
    public long? DeletedByUserId { get; set; }
    public bool IsDeleted { get; set; }
    public uint RowVersion { get; set; }
}
