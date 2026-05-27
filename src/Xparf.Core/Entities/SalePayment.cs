using Xparf.Core.Enums;

namespace Xparf.Core.Entities;

public sealed class SalePayment : AuditableEntity
{
    public long SaleId { get; set; }
    public Sale Sale { get; set; } = null!;
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? Note { get; set; }
}
