using Xparf.Core.Abstractions;

namespace Xparf.Infrastructure.Services;

public sealed class SystemCurrentUserContext : ICurrentUserContext
{
    public long? UserId => null;
    public long? CompanyId => null;
    public bool IsAuthenticated => false;
    public bool IsSuperAdmin => false;
}
