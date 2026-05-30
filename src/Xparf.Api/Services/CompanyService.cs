using Microsoft.EntityFrameworkCore;
using Xparf.Api.Contracts.Company;
using Xparf.Core.Abstractions;
using Xparf.Infrastructure.Persistence;

namespace Xparf.Api.Services;

public interface ICompanyService
{
    Task<CompanyResponse> GetCurrentCompanyAsync(CancellationToken cancellationToken);
    Task<CompanyResponse> UpdateCurrentCompanyAsync(UpdateCompanyRequest request, CancellationToken cancellationToken);
    Task<CoinBalanceResponse> GetCoinBalanceAsync(CancellationToken cancellationToken);
}

public sealed class CompanyService(
    XparfDbContext dbContext,
    ICurrentUserContext currentUserContext) : ICompanyService
{
    public async Task<CompanyResponse> GetCurrentCompanyAsync(CancellationToken cancellationToken)
    {
        var company = await GetCompanyAsync(cancellationToken);
        return ToResponse(company);
    }

    public async Task<CompanyResponse> UpdateCurrentCompanyAsync(UpdateCompanyRequest request, CancellationToken cancellationToken)
    {
        var company = await GetCompanyAsync(cancellationToken);
        company.Name = request.Name.Trim();
        company.Phone = request.Phone;
        company.Address = request.Address;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(company);
    }

    public async Task<CoinBalanceResponse> GetCoinBalanceAsync(CancellationToken cancellationToken)
    {
        var company = await GetCompanyAsync(cancellationToken);
        return new CoinBalanceResponse(company.Id, company.CoinBalance, company.IsFrozen);
    }

    private async Task<Core.Entities.Company> GetCompanyAsync(CancellationToken cancellationToken)
    {
        var companyId = currentUserContext.CompanyId
            ?? throw new UnauthorizedAccessException("Company context tidak ditemukan.");

        return await dbContext.Companies.FirstOrDefaultAsync(x => x.Id == companyId, cancellationToken)
            ?? throw new InvalidOperationException("Company tidak ditemukan.");
    }

    private static CompanyResponse ToResponse(Core.Entities.Company company) => new(
        company.Id,
        company.Name,
        company.Email,
        company.Phone,
        company.Address,
        company.SubscriptionPlan,
        company.CoinBalance,
        company.IsFrozen);
}
