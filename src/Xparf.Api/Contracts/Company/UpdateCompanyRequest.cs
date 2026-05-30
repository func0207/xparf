namespace Xparf.Api.Contracts.Company;

public sealed record UpdateCompanyRequest(
    string Name,
    string? Phone,
    string? Address);
