namespace Xparf.Api.Contracts.Auth;

public sealed record RegisterCompanyRequest(
    string CompanyName,
    string Email,
    string UserName,
    string Password,
    string? Phone,
    string? Address);
