using System.Security.Claims;
using Xparf.Core.Abstractions;

namespace Xparf.Api.Services;

public sealed class HttpCurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
{
    public long? UserId => GetLongClaim(ClaimTypes.NameIdentifier) ?? GetLongClaim("sub");
    public long? CompanyId => GetLongClaim("company_id");
    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;
    public bool IsSuperAdmin => GetBoolClaim("is_super_admin");

    private long? GetLongClaim(string type)
    {
        var value = httpContextAccessor.HttpContext?.User.FindFirstValue(type);
        return long.TryParse(value, out var result) ? result : null;
    }

    private bool GetBoolClaim(string type)
    {
        var value = httpContextAccessor.HttpContext?.User.FindFirstValue(type);
        return bool.TryParse(value, out var result) && result;
    }
}
