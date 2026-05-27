using Xparf.Core.Enums;

namespace Xparf.Core.Entities;

public sealed class CoinLedger : AuditableEntity
{
    public long CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public CoinTransactionType TransactionType { get; set; }
    public string ReferenceType { get; set; } = string.Empty;
    public long? ReferenceId { get; set; }
    public decimal CoinIn { get; set; }
    public decimal CoinOut { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public string? Note { get; set; }
}
