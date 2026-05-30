namespace Xparf.Api.Contracts.Roles;

public sealed record RoleResponse(long Id, string Name, string? Description, bool IsSystemRole, string[] Permissions);
public sealed record CreateRoleRequest(string Name, string? Description);
public sealed record UpdateRoleRequest(string Name, string? Description);
public sealed record UpdateRolePermissionsRequest(long[] PermissionIds);
public sealed record PermissionResponse(long Id, string Code, string Name, string Module);
