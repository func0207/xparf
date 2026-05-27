namespace Xparf.Core.Abstractions;

public interface ICurrentUserContext
{
    long? UserId { get; }
    long? CompanyId { get; }
    bool IsAuthenticated { get; }
    bool IsSuperAdmin { get; }
}
