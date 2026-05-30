namespace Xparf.Api.Contracts.Company;

public sealed record CoinBalanceResponse(long CompanyId, decimal CoinBalance, bool IsFrozen);
