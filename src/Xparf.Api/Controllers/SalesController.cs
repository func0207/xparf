using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xparf.Api.Contracts.Sales;
using Xparf.Api.Services;

namespace Xparf.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/sales")]
public sealed class SalesController(ISaleService saleService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SaleResponse>>> GetSales(CancellationToken cancellationToken) => Ok(await saleService.GetSalesAsync(cancellationToken));
    [HttpGet("{id:long}")]
    public async Task<ActionResult<SaleResponse>> GetSale(long id, CancellationToken cancellationToken) => Ok(await saleService.GetSaleAsync(id, cancellationToken));
    [HttpPost]
    public async Task<ActionResult<SaleResponse>> Create(CreateSaleRequest request, CancellationToken cancellationToken) => Ok(await saleService.CreateSaleAsync(request, cancellationToken));
    [HttpPut("{id:long}")]
    public async Task<ActionResult<SaleResponse>> Update(long id, UpdateSaleRequest request, CancellationToken cancellationToken) => Ok(await saleService.UpdateSaleAsync(id, request, cancellationToken));
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken) { await saleService.DeleteSaleAsync(id, cancellationToken); return NoContent(); }
    [HttpPost("{id:long}/lines")]
    public async Task<ActionResult<SaleResponse>> AddLine(long id, CreateSaleLineRequest request, CancellationToken cancellationToken) => Ok(await saleService.AddLineAsync(id, request, cancellationToken));
    [HttpPut("{id:long}/lines/{lineId:long}")]
    public async Task<ActionResult<SaleResponse>> UpdateLine(long id, long lineId, UpdateSaleLineRequest request, CancellationToken cancellationToken) => Ok(await saleService.UpdateLineAsync(id, lineId, request, cancellationToken));
    [HttpDelete("{id:long}/lines/{lineId:long}")]
    public async Task<ActionResult<SaleResponse>> DeleteLine(long id, long lineId, CancellationToken cancellationToken) => Ok(await saleService.DeleteLineAsync(id, lineId, cancellationToken));
    [HttpPost("{id:long}/post")]
    public async Task<ActionResult<SaleResponse>> Post(long id, CancellationToken cancellationToken) => Ok(await saleService.PostSaleAsync(id, cancellationToken));
    [HttpPost("{id:long}/payments")]
    public async Task<ActionResult<SaleResponse>> AddPayment(long id, CreateSalePaymentRequest request, CancellationToken cancellationToken) => Ok(await saleService.AddPaymentAsync(id, request, cancellationToken));
    [HttpPost("{id:long}/void")]
    public async Task<ActionResult<SaleResponse>> Void(long id, CancellationToken cancellationToken) => Ok(await saleService.VoidSaleAsync(id, cancellationToken));
    [HttpGet("{id:long}/receipt")]
    public async Task<ActionResult<SaleResponse>> Receipt(long id, CancellationToken cancellationToken) => Ok(await saleService.GetSaleAsync(id, cancellationToken));
}
