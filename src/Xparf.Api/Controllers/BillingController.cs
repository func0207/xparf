using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xparf.Api.Contracts.Billing;
using Xparf.Api.Services;

namespace Xparf.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/billing")]
public sealed class BillingController(IBillingService billingService) : ControllerBase
{
    [HttpGet("coin-balance")]
    public async Task<ActionResult<BillingCoinBalanceResponse>> GetCoinBalance(CancellationToken cancellationToken) => Ok(await billingService.GetCoinBalanceAsync(cancellationToken));

    [HttpGet("coin-ledgers")]
    public async Task<ActionResult<IReadOnlyList<CoinLedgerResponse>>> GetCoinLedgers(CancellationToken cancellationToken) => Ok(await billingService.GetCoinLedgersAsync(cancellationToken));

    [HttpGet("topup-packages")]
    public async Task<ActionResult<IReadOnlyList<TopupPackageResponse>>> GetTopupPackages(CancellationToken cancellationToken) => Ok(await billingService.GetTopupPackagesAsync(cancellationToken));

    [HttpPost("topups")]
    public async Task<ActionResult<CoinTopupResponse>> CreateTopup(CreateTopupRequest request, CancellationToken cancellationToken) => Ok(await billingService.CreateTopupAsync(request, cancellationToken));

    [HttpGet("topups/{id:long}")]
    public async Task<ActionResult<CoinTopupResponse>> GetTopup(long id, CancellationToken cancellationToken) => Ok(await billingService.GetTopupAsync(id, cancellationToken));
}
