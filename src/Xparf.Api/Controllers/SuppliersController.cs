using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xparf.Api.Contracts.Common;
using Xparf.Api.Contracts.Suppliers;
using Xparf.Api.Services;

namespace Xparf.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/suppliers")]
public sealed class SuppliersController(ISupplierService supplierService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PageResponse<SupplierResponse>>> GetSuppliers([FromQuery] PageRequest request, CancellationToken cancellationToken)
    {
        var response = await supplierService.GetSuppliersAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<SupplierResponse>> GetSupplier(long id, CancellationToken cancellationToken)
    {
        var response = await supplierService.GetSupplierAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<SupplierResponse>> Create(CreateSupplierRequest request, CancellationToken cancellationToken)
    {
        var response = await supplierService.CreateSupplierAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<SupplierResponse>> Update(long id, UpdateSupplierRequest request, CancellationToken cancellationToken)
    {
        var response = await supplierService.UpdateSupplierAsync(id, request, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        await supplierService.DeleteSupplierAsync(id, cancellationToken);
        return NoContent();
    }
}
