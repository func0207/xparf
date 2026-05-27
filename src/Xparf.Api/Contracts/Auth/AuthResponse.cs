namespace Xparf.Api.Contracts.Auth;

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    long UserId,
    long CompanyId,
    string Email,
    string UserName,
    bool IsOwner,
    bool IsSuperAdmin);
