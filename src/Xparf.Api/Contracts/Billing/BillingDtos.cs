using Xparf.Core.Enums;

namespace Xparf.Api.Contracts.Billing;

public sealed record BillingCoinBalanceResponse(decimal CoinBalance, bool IsFrozen);

public sealed record CoinLedgerResponse(
    long Id,
    CoinTransactionType TransactionType,
    string ReferenceType,
    long? ReferenceId,
    decimal CoinIn,
    decimal CoinOut,
    decimal BalanceBefore,
    decimal BalanceAfter,
    string? Note,
    DateTime CreatedAt);

public sealed record TopupPackageResponse(
    long Id,
    string Name,
    decimal MoneyAmount,
    decimal CoinAmount,
    int SortOrder);

public sealed record CreateTopupRequest(long TopupPackageId);

public sealed record CoinTopupResponse(
    long Id,
    string TopupNumber,
    decimal MoneyAmount,
    decimal CoinAmount,
    TopupStatus Status,
    string PaymentProvider,
    string ProviderReference,
    string? QrCodeText,
    string? QrCodeImageUrl,
    DateTime ExpiredAt,
    DateTime? PaidAt);

public sealed record QrisWebhookRequest(
    string Reference,
    decimal Amount,
    string Status,
    string? Signature,
    string? PayloadJson);

public sealed record QrisWebhookResponse(bool Processed, string Message);
