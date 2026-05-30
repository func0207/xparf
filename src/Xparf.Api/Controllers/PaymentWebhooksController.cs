using Microsoft.AspNetCore.Mvc;
using Xparf.Api.Contracts.Billing;
using Xparf.Api.Services;

namespace Xparf.Api.Controllers;

[ApiController]
[Route("api/payment-webhooks")]
public sealed class PaymentWebhooksController(IBillingService billingService) : ControllerBase
{
    [HttpPost("qris")]
    public async Task<ActionResult<QrisWebhookResponse>> Qris(QrisWebhookRequest request, CancellationToken cancellationToken) => Ok(await billingService.ProcessQrisWebhookAsync(request, cancellationToken));
}
