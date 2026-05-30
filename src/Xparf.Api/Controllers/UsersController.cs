using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xparf.Api.Contracts.Users;
using Xparf.Api.Services;

namespace Xparf.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/users")]
public sealed class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> GetUsers(CancellationToken cancellationToken)
    {
        var response = await userService.GetUsersAsync(cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var response = await userService.CreateUserAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("invite")]
    public async Task<ActionResult<UserResponse>> Invite(InviteUserRequest request, CancellationToken cancellationToken)
    {
        var response = await userService.InviteUserAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<UserResponse>> Update(long id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var response = await userService.UpdateUserAsync(id, request, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        await userService.DeleteUserAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:long}/roles")]
    public async Task<ActionResult<UserResponse>> UpdateRole(long id, UpdateUserRoleRequest request, CancellationToken cancellationToken)
    {
        var response = await userService.UpdateRoleAsync(id, request, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{id:long}/branches")]
    public async Task<ActionResult<UserResponse>> UpdateBranches(long id, UpdateUserBranchesRequest request, CancellationToken cancellationToken)
    {
        var response = await userService.UpdateBranchesAsync(id, request, cancellationToken);
        return Ok(response);
    }
}
