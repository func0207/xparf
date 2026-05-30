using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xparf.Api.Contracts.Roles;
using Xparf.Api.Services;

namespace Xparf.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/roles")]
public sealed class RolesController(IRoleService roleService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RoleResponse>>> GetRoles(CancellationToken cancellationToken)
    {
        var response = await roleService.GetRolesAsync(cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<RoleResponse>> Create(CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var response = await roleService.CreateRoleAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<RoleResponse>> Update(long id, UpdateRoleRequest request, CancellationToken cancellationToken)
    {
        var response = await roleService.UpdateRoleAsync(id, request, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        await roleService.DeleteRoleAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:long}/permissions")]
    public async Task<ActionResult<RoleResponse>> UpdatePermissions(long id, UpdateRolePermissionsRequest request, CancellationToken cancellationToken)
    {
        var response = await roleService.UpdatePermissionsAsync(id, request, cancellationToken);
        return Ok(response);
    }
}
