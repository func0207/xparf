using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xparf.Api.Contracts.Admin;
using Xparf.Api.Services;

namespace Xparf.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/admin")]
public sealed class AdminController(IAdminService adminService) : ControllerBase
{
    [HttpGet("companies")]
    public async Task<ActionResult<IReadOnlyList<AdminCompanyResponse>>> GetCompanies(CancellationToken cancellationToken) => Ok(await adminService.GetCompaniesAsync(cancellationToken));

    [HttpPut("companies/{id:long}/freeze")]
    public async Task<ActionResult<AdminCompanyResponse>> FreezeCompany(long id, CancellationToken cancellationToken) => Ok(await adminService.FreezeCompanyAsync(id, cancellationToken));

    [HttpPut("companies/{id:long}/unfreeze")]
    public async Task<ActionResult<AdminCompanyResponse>> UnfreezeCompany(long id, CancellationToken cancellationToken) => Ok(await adminService.UnfreezeCompanyAsync(id, cancellationToken));

    [HttpGet("topup-packages")]
    public async Task<ActionResult<IReadOnlyList<AdminTopupPackageResponse>>> GetTopupPackages(CancellationToken cancellationToken) => Ok(await adminService.GetTopupPackagesAsync(cancellationToken));

    [HttpPost("topup-packages")]
    public async Task<ActionResult<AdminTopupPackageResponse>> CreateTopupPackage(CreateAdminTopupPackageRequest request, CancellationToken cancellationToken) => Ok(await adminService.CreateTopupPackageAsync(request, cancellationToken));

    [HttpPut("topup-packages/{id:long}")]
    public async Task<ActionResult<AdminTopupPackageResponse>> UpdateTopupPackage(long id, UpdateAdminTopupPackageRequest request, CancellationToken cancellationToken) => Ok(await adminService.UpdateTopupPackageAsync(id, request, cancellationToken));

    [HttpDelete("topup-packages/{id:long}")]
    public async Task<IActionResult> DeleteTopupPackage(long id, CancellationToken cancellationToken) { await adminService.DeleteTopupPackageAsync(id, cancellationToken); return NoContent(); }

    [HttpGet("platform-settings")]
    public async Task<ActionResult<IReadOnlyList<AdminPlatformSettingResponse>>> GetPlatformSettings(CancellationToken cancellationToken) => Ok(await adminService.GetPlatformSettingsAsync(cancellationToken));

    [HttpPost("platform-settings")]
    public async Task<ActionResult<AdminPlatformSettingResponse>> CreatePlatformSetting(CreateAdminPlatformSettingRequest request, CancellationToken cancellationToken) => Ok(await adminService.CreatePlatformSettingAsync(request, cancellationToken));

    [HttpPut("platform-settings/{id:long}")]
    public async Task<ActionResult<AdminPlatformSettingResponse>> UpdatePlatformSetting(long id, UpdateAdminPlatformSettingRequest request, CancellationToken cancellationToken) => Ok(await adminService.UpdatePlatformSettingAsync(id, request, cancellationToken));

    [HttpDelete("platform-settings/{id:long}")]
    public async Task<IActionResult> DeletePlatformSetting(long id, CancellationToken cancellationToken) { await adminService.DeletePlatformSettingAsync(id, cancellationToken); return NoContent(); }

    [HttpGet("coin-ledgers")]
    public async Task<ActionResult<IReadOnlyList<AdminCoinLedgerResponse>>> GetCoinLedgers(CancellationToken cancellationToken) => Ok(await adminService.GetCoinLedgersAsync(cancellationToken));

    [HttpPost("coin-adjustments")]
    public async Task<ActionResult<AdminCoinLedgerResponse>> CreateCoinAdjustment(AdminCoinAdjustmentRequest request, CancellationToken cancellationToken) => Ok(await adminService.CreateCoinAdjustmentAsync(request, cancellationToken));

    [HttpGet("payment-webhook-logs")]
    public async Task<ActionResult<IReadOnlyList<AdminPaymentWebhookLogResponse>>> GetPaymentWebhookLogs(CancellationToken cancellationToken) => Ok(await adminService.GetPaymentWebhookLogsAsync(cancellationToken));
}
