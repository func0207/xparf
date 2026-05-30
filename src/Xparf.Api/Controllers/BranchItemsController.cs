using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xparf.Api.Contracts.Inventory;
using Xparf.Api.Services;

namespace Xparf.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/branch-items")]
public sealed class BranchItemsController(IInventoryService inventoryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BranchItemResponse>>> GetBranchItems(CancellationToken cancellationToken) => Ok(await inventoryService.GetBranchItemsAsync(cancellationToken));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<BranchItemResponse>> GetBranchItem(long id, CancellationToken cancellationToken) => Ok(await inventoryService.GetBranchItemAsync(id, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<BranchItemResponse>> Create(CreateBranchItemRequest request, CancellationToken cancellationToken) => Ok(await inventoryService.CreateBranchItemAsync(request, cancellationToken));

    [HttpPut("{id:long}")]
    public async Task<ActionResult<BranchItemResponse>> Update(long id, UpdateBranchItemRequest request, CancellationToken cancellationToken) => Ok(await inventoryService.UpdateBranchItemAsync(id, request, cancellationToken));

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        await inventoryService.DeleteBranchItemAsync(id, cancellationToken);
        return NoContent();
    }
}
