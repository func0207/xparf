using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xparf.Api.Contracts.Reports;
using Xparf.Api.Services;

namespace Xparf.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/reports")]
public sealed class ReportsController(IReportService reportService) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardSummaryResponse>> Dashboard([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken) => Ok(await reportService.GetDashboardSummaryAsync(from, to, cancellationToken));

    [HttpGet("stock")]
    public async Task<ActionResult<IReadOnlyList<StockReportResponse>>> Stock([FromQuery] long? branchId, CancellationToken cancellationToken) => Ok(await reportService.GetStockReportAsync(branchId, cancellationToken));

    [HttpGet("sales")]
    public async Task<ActionResult<SalesReportResponse>> Sales([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken) => Ok(await reportService.GetSalesReportAsync(from, to, cancellationToken));

    [HttpGet("purchases")]
    public async Task<ActionResult<PurchaseReportResponse>> Purchases([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken) => Ok(await reportService.GetPurchaseReportAsync(from, to, cancellationToken));
}
