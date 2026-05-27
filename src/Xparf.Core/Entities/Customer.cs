namespace Xparf.Core.Entities;

public sealed class Customer : AuditableEntity
{
    public long CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public long? BranchId { get; set; }
    public Branch? Branch { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal CurrentDebt { get; set; }
    public bool IsActive { get; set; } = true;
}
