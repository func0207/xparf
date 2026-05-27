namespace Xparf.Core.Entities;

public sealed class SaleComplaint : AuditableEntity
{
    public long CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public long SaleId { get; set; }
    public Sale Sale { get; set; } = null!;
    public string ComplaintNumber { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? Resolution { get; set; }
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
}
