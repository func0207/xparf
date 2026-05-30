using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xparf.Api.Contracts.Purchases;
using Xparf.Api.Services;

namespace Xparf.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/purchases")]
public sealed class PurchasesController(IPurchaseService purchaseService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PurchaseResponse>>> GetPurchases(CancellationToken cancellationToken) => Ok(await purchaseService.GetPurchasesAsync(cancellationToken));
    [HttpGet("{id:long}")]
    public async Task<ActionResult<PurchaseResponse>> GetPurchase(long id, CancellationToken cancellationToken) => Ok(await purchaseService.GetPurchaseAsync(id, cancellationToken));
    [HttpPost]
    public async Task<ActionResult<PurchaseResponse>> Create(CreatePurchaseRequest request, CancellationToken cancellationToken) => Ok(await purchaseService.CreatePurchaseAsync(request, cancellationToken));
    [HttpPut("{id:long}")]
    public async Task<ActionResult<PurchaseResponse>> Update(long id, UpdatePurchaseRequest request, CancellationToken cancellationToken) => Ok(await purchaseService.UpdatePurchaseAsync(id, request, cancellationToken));
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken) { await purchaseService.DeletePurchaseAsync(id, cancellationToken); return NoContent(); }
    [HttpPost("{id:long}/lines")]
    public async Task<ActionResult<PurchaseResponse>> AddLine(long id, CreatePurchaseLineRequest request, CancellationToken cancellationToken) => Ok(await purchaseService.AddLineAsync(id, request, cancellationToken));
    [HttpPut("{id:long}/lines/{lineId:long}")]
    public async Task<ActionResult<PurchaseResponse>> UpdateLine(long id, long lineId, UpdatePurchaseLineRequest request, CancellationToken cancellationToken) => Ok(await purchaseService.UpdateLineAsync(id, lineId, request, cancellationToken));
    [HttpDelete("{id:long}/lines/{lineId:long}")]
    public async Task<ActionResult<PurchaseResponse>> DeleteLine(long id, long lineId, CancellationToken cancellationToken) => Ok(await purchaseService.DeleteLineAsync(id, lineId, cancellationToken));
    [HttpPost("{id:long}/post")]
    public async Task<ActionResult<PurchaseResponse>> Post(long id, CancellationToken cancellationToken) => Ok(await purchaseService.PostPurchaseAsync(id, cancellationToken));
    [HttpPost("{id:long}/payments")]
    public async Task<ActionResult<PurchaseResponse>> AddPayment(long id, CreatePurchasePaymentRequest request, CancellationToken cancellationToken) => Ok(await purchaseService.AddPaymentAsync(id, request, cancellationToken));
    [HttpPost("{id:long}/cancel")]
    public async Task<ActionResult<PurchaseResponse>> Cancel(long id, CancellationToken cancellationToken) => Ok(await purchaseService.CancelPurchaseAsync(id, cancellationToken));
}
