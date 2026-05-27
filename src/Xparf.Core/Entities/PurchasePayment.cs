using Xparf.Core.Enums;

namespace Xparf.Core.Entities;

public sealed class PurchasePayment : AuditableEntity
{
    public long PurchaseId { get; set; }
    public Purchase Purchase { get; set; } = null!;
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? Note { get; set; }
}
