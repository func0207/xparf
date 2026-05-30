namespace Xparf.Api.Contracts.Auth;

public sealed record LogoutRequest(string RefreshToken);
public sealed record ConfirmEmailRequest(string Email);
public sealed record ForgotPasswordRequest(string Email);
public sealed record ResetPasswordRequest(string Email, string NewPassword);
public sealed record AuthMessageResponse(string Message);
