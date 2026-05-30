using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xparf.Api.Contracts.Company;
using Xparf.Api.Services;

namespace Xparf.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/company")]
public sealed class CompanyController(ICompanyService companyService) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<CompanyResponse>> GetMe(CancellationToken cancellationToken)
    {
        var response = await companyService.GetCurrentCompanyAsync(cancellationToken);
        return Ok(response);
    }

    [HttpPut("me")]
    public async Task<ActionResult<CompanyResponse>> UpdateMe(UpdateCompanyRequest request, CancellationToken cancellationToken)
    {
        var response = await companyService.UpdateCurrentCompanyAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("me/coin-balance")]
    public async Task<ActionResult<CoinBalanceResponse>> GetCoinBalance(CancellationToken cancellationToken)
    {
        var response = await companyService.GetCoinBalanceAsync(cancellationToken);
        return Ok(response);
    }
}
