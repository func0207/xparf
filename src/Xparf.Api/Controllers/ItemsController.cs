using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xparf.Api.Contracts.Common;
using Xparf.Api.Contracts.Items;
using Xparf.Api.Services;

namespace Xparf.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/items")]
public sealed class ItemsController(IItemService itemService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PageResponse<ItemResponse>>> GetItems([FromQuery] PageRequest request, CancellationToken cancellationToken)
    {
        var response = await itemService.GetItemsAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ItemResponse>> GetItem(long id, CancellationToken cancellationToken)
    {
        var response = await itemService.GetItemAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<ItemResponse>> Create(CreateItemRequest request, CancellationToken cancellationToken)
    {
        var response = await itemService.CreateItemAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<ItemResponse>> Update(long id, UpdateItemRequest request, CancellationToken cancellationToken)
    {
        var response = await itemService.UpdateItemAsync(id, request, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        await itemService.DeleteItemAsync(id, cancellationToken);
        return NoContent();
    }
}
