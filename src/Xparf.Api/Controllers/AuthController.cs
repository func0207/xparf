using Microsoft.AspNetCore.Mvc;
using Xparf.Api.Contracts.Auth;
using Xparf.Api.Services;

namespace Xparf.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register-company")]
    public async Task<ActionResult<AuthResponse>> RegisterCompany(RegisterCompanyRequest request, CancellationToken cancellationToken)
    {
        var response = await authService.RegisterCompanyAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await authService.LoginAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var response = await authService.RefreshAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("logout")]
    public async Task<ActionResult<AuthMessageResponse>> Logout(LogoutRequest request, CancellationToken cancellationToken) => Ok(await authService.LogoutAsync(request, cancellationToken));

    [HttpPost("confirm-email")]
    public async Task<ActionResult<AuthMessageResponse>> ConfirmEmail(ConfirmEmailRequest request, CancellationToken cancellationToken) => Ok(await authService.ConfirmEmailAsync(request, cancellationToken));

    [HttpPost("forgot-password")]
    public async Task<ActionResult<AuthMessageResponse>> ForgotPassword(ForgotPasswordRequest request, CancellationToken cancellationToken) => Ok(await authService.ForgotPasswordAsync(request, cancellationToken));

    [HttpPost("reset-password")]
    public async Task<ActionResult<AuthMessageResponse>> ResetPassword(ResetPasswordRequest request, CancellationToken cancellationToken) => Ok(await authService.ResetPasswordAsync(request, cancellationToken));
}
