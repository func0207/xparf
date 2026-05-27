using Xparf.Core.Enums;

namespace Xparf.Core.Entities;

public sealed class CoinTopup : AuditableEntity
{
    public long CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public string TopupNumber { get; set; } = string.Empty;
    public decimal MoneyAmount { get; set; }
    public decimal CoinAmount { get; set; }
    public TopupStatus Status { get; set; } = TopupStatus.Pending;
    public string PaymentProvider { get; set; } = string.Empty;
    public string ProviderReference { get; set; } = string.Empty;
    public string? QrCodeText { get; set; }
    public string? QrCodeImageUrl { get; set; }
    public DateTime ExpiredAt { get; set; }
    public DateTime? PaidAt { get; set; }
}
