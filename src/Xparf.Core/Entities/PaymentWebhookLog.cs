namespace Xparf.Core.Entities;

public sealed class PaymentWebhookLog : AuditableEntity
{
    public string Provider { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
    public string? Signature { get; set; }
    public bool IsValid { get; set; }
    public bool IsProcessed { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ErrorMessage { get; set; }
}
