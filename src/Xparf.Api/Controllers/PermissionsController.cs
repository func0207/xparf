using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xparf.Api.Contracts.Roles;
using Xparf.Api.Services;

namespace Xparf.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/permissions")]
public sealed class PermissionsController(IRoleService roleService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PermissionResponse>>> GetPermissions(CancellationToken cancellationToken)
    {
        var response = await roleService.GetPermissionsAsync(cancellationToken);
        return Ok(response);
    }
}
