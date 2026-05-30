using Xparf.Core.Enums;

namespace Xparf.Api.Contracts.Company;

public sealed record CompanyResponse(
    long Id,
    string Name,
    string Email,
    string? Phone,
    string? Address,
    SubscriptionPlan SubscriptionPlan,
    decimal CoinBalance,
    bool IsFrozen);
