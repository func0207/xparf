using Microsoft.EntityFrameworkCore;
using Xparf.Api.Contracts.Roles;
using Xparf.Core.Abstractions;
using Xparf.Core.Entities;
using Xparf.Infrastructure.Persistence;

namespace Xparf.Api.Services;

public interface IRoleService
{
    Task<IReadOnlyList<RoleResponse>> GetRolesAsync(CancellationToken cancellationToken);
    Task<RoleResponse> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken);
    Task<RoleResponse> UpdateRoleAsync(long id, UpdateRoleRequest request, CancellationToken cancellationToken);
    Task DeleteRoleAsync(long id, CancellationToken cancellationToken);
    Task<IReadOnlyList<PermissionResponse>> GetPermissionsAsync(CancellationToken cancellationToken);
    Task<RoleResponse> UpdatePermissionsAsync(long id, UpdateRolePermissionsRequest request, CancellationToken cancellationToken);
}

public sealed class RoleService(XparfDbContext dbContext, ICurrentUserContext currentUserContext) : IRoleService
{
    public async Task<IReadOnlyList<RoleResponse>> GetRolesAsync(CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        return await dbContext.Roles
            .Include(x => x.RolePermissions)
            .ThenInclude(x => x.Permission)
            .Where(x => x.CompanyId == companyId || x.CompanyId == null)
            .OrderBy(x => x.Name)
            .Select(x => ToResponse(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<RoleResponse> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        var name = request.Name.Trim();
        if (await dbContext.Roles.AnyAsync(x => x.CompanyId == companyId && x.Name == name, cancellationToken))
        {
            throw new InvalidOperationException("Role sudah ada.");
        }

        var role = new Role
        {
            CompanyId = companyId,
            Name = name,
            Description = request.Description,
            IsSystemRole = false
        };
        dbContext.Roles.Add(role);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(role);
    }

    public async Task<RoleResponse> UpdateRoleAsync(long id, UpdateRoleRequest request, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        var role = await GetCompanyRoleAsync(id, companyId, cancellationToken);
        if (role.IsSystemRole)
        {
            throw new InvalidOperationException("System role tidak boleh diubah.");
        }

        role.Name = request.Name.Trim();
        role.Description = request.Description;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(role);
    }

    public async Task DeleteRoleAsync(long id, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        var role = await GetCompanyRoleAsync(id, companyId, cancellationToken);
        if (role.IsSystemRole)
        {
            throw new InvalidOperationException("System role tidak boleh dihapus.");
        }

        dbContext.Roles.Remove(role);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PermissionResponse>> GetPermissionsAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Permissions
            .OrderBy(x => x.Module)
            .ThenBy(x => x.Code)
            .Select(x => new PermissionResponse(x.Id, x.Code, x.Name, x.Module))
            .ToListAsync(cancellationToken);
    }

    public async Task<RoleResponse> UpdatePermissionsAsync(long id, UpdateRolePermissionsRequest request, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        var role = await dbContext.Roles
            .Include(x => x.RolePermissions)
            .ThenInclude(x => x.Permission)
            .FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId, cancellationToken)
            ?? throw new InvalidOperationException("Role tidak ditemukan.");

        dbContext.RolePermissions.RemoveRange(role.RolePermissions);
        var permissionIds = request.PermissionIds.Distinct().ToArray();
        var permissions = await dbContext.Permissions
            .Where(x => permissionIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        foreach (var permission in permissions)
        {
            dbContext.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = permission.Id });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return await dbContext.Roles
            .Include(x => x.RolePermissions)
            .ThenInclude(x => x.Permission)
            .Where(x => x.Id == role.Id)
            .Select(x => ToResponse(x))
            .FirstAsync(cancellationToken);
    }

    private long GetCompanyId() => currentUserContext.CompanyId
        ?? throw new UnauthorizedAccessException("Company context tidak ditemukan.");

    private async Task<Role> GetCompanyRoleAsync(long id, long companyId, CancellationToken cancellationToken)
    {
        return await dbContext.Roles.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId, cancellationToken)
            ?? throw new InvalidOperationException("Role tidak ditemukan.");
    }

    private static RoleResponse ToResponse(Role role) => new(
        role.Id,
        role.Name,
        role.Description,
        role.IsSystemRole,
        role.RolePermissions.Select(x => x.Permission.Code).Order().ToArray());
}
