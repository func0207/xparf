namespace Xparf.Api.Contracts.Users;

public sealed record UserResponse(
    long Id,
    string Email,
    string UserName,
    long RoleId,
    string RoleName,
    bool EmailConfirmed,
    bool IsOwner,
    bool IsActive,
    long[] BranchIds);

public sealed record CreateUserRequest(
    string Email,
    string UserName,
    string Password,
    long RoleId,
    long[] BranchIds,
    bool IsActive);

public sealed record InviteUserRequest(
    string Email,
    string UserName,
    long RoleId,
    long[] BranchIds);

public sealed record UpdateUserRequest(
    string UserName,
    bool IsActive);

public sealed record UpdateUserRoleRequest(long RoleId);
public sealed record UpdateUserBranchesRequest(long[] BranchIds, long? DefaultBranchId);
