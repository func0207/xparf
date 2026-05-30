using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xparf.Api.Contracts.Complaints;
using Xparf.Api.Services;

namespace Xparf.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/complaints")]
public sealed class ComplaintsController(ISaleComplaintService complaintService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SaleComplaintResponse>>> GetComplaints(CancellationToken cancellationToken) => Ok(await complaintService.GetComplaintsAsync(cancellationToken));

    [HttpPost]
    public async Task<ActionResult<SaleComplaintResponse>> Create(CreateSaleComplaintRequest request, CancellationToken cancellationToken) => Ok(await complaintService.CreateComplaintAsync(request, cancellationToken));

    [HttpPost("{id:long}/resolve")]
    public async Task<ActionResult<SaleComplaintResponse>> Resolve(long id, ResolveSaleComplaintRequest request, CancellationToken cancellationToken) => Ok(await complaintService.ResolveComplaintAsync(id, request, cancellationToken));
}
