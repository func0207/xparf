using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xparf.Api.Contracts.Branches;
using Xparf.Api.Services;

namespace Xparf.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/branches")]
public sealed class BranchesController(IBranchService branchService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BranchResponse>>> GetBranches(CancellationToken cancellationToken)
    {
        var response = await branchService.GetBranchesAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<BranchResponse>> GetBranch(long id, CancellationToken cancellationToken)
    {
        var response = await branchService.GetBranchAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<BranchResponse>> Create(CreateBranchRequest request, CancellationToken cancellationToken)
    {
        var response = await branchService.CreateBranchAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<BranchResponse>> Update(long id, UpdateBranchRequest request, CancellationToken cancellationToken)
    {
        var response = await branchService.UpdateBranchAsync(id, request, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        await branchService.DeleteBranchAsync(id, cancellationToken);
        return NoContent();
    }
}
