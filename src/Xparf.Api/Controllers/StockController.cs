using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xparf.Api.Contracts.Inventory;
using Xparf.Api.Services;

namespace Xparf.Api.Controllers;

[Authorize]
[ApiController]
[Route("api")]
public sealed class StockController(IInventoryService inventoryService) : ControllerBase
{
    [HttpGet("stock-ledgers")]
    public async Task<ActionResult<IReadOnlyList<StockLedgerResponse>>> GetStockLedgers([FromQuery] long? branchId, [FromQuery] long? itemId, CancellationToken cancellationToken)
    {
        var response = await inventoryService.GetStockLedgersAsync(branchId, itemId, cancellationToken);
        return Ok(response);
    }

    [HttpPost("stock-adjustments")]
    public async Task<ActionResult<StockLedgerResponse>> CreateStockAdjustment(StockAdjustmentRequest request, CancellationToken cancellationToken)
    {
        var response = await inventoryService.CreateStockAdjustmentAsync(request, cancellationToken);
        return Ok(response);
    }
}
