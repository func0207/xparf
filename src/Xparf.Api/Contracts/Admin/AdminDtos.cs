using Xparf.Core.Enums;

namespace Xparf.Api.Contracts.Admin;

public sealed record AdminCompanyResponse(
    long Id,
    string Name,
    string Email,
    string? Phone,
    SubscriptionPlan SubscriptionPlan,
    decimal CoinBalance,
    bool IsFrozen,
    int UserCount,
    int BranchCount,
    DateTime CreatedAt);

public sealed record AdminTopupPackageResponse(long Id, string Name, decimal MoneyAmount, decimal CoinAmount, bool IsActive, int SortOrder);
public sealed record CreateAdminTopupPackageRequest(string Name, decimal MoneyAmount, decimal CoinAmount, bool IsActive, int SortOrder);
public sealed record UpdateAdminTopupPackageRequest(string Name, decimal MoneyAmount, decimal CoinAmount, bool IsActive, int SortOrder);

public sealed record AdminPlatformSettingResponse(long Id, string Key, string Value, string DataType, string? Description);
public sealed record CreateAdminPlatformSettingRequest(string Key, string Value, string DataType, string? Description);
public sealed record UpdateAdminPlatformSettingRequest(string Value, string DataType, string? Description);

public sealed record AdminCoinLedgerResponse(
    long Id,
    long CompanyId,
    string CompanyName,
    CoinTransactionType TransactionType,
    string ReferenceType,
    long? ReferenceId,
    decimal CoinIn,
    decimal CoinOut,
    decimal BalanceBefore,
    decimal BalanceAfter,
    string? Note,
    DateTime CreatedAt);

public sealed record AdminCoinAdjustmentRequest(long CompanyId, decimal Amount, string Note);

public sealed record AdminPaymentWebhookLogResponse(
    long Id,
    string Provider,
    string Reference,
    string? Signature,
    bool IsValid,
    bool IsProcessed,
    DateTime? ProcessedAt,
    string? ErrorMessage,
    DateTime CreatedAt);
