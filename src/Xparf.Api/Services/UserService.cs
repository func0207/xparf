using Microsoft.EntityFrameworkCore;
using Xparf.Api.Contracts.Users;
using Xparf.Core.Abstractions;
using Xparf.Core.Entities;
using Xparf.Infrastructure.Persistence;

namespace Xparf.Api.Services;

public interface IUserService
{
    Task<IReadOnlyList<UserResponse>> GetUsersAsync(CancellationToken cancellationToken);
    Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken);
    Task<UserResponse> InviteUserAsync(InviteUserRequest request, CancellationToken cancellationToken);
    Task<UserResponse> UpdateUserAsync(long id, UpdateUserRequest request, CancellationToken cancellationToken);
    Task DeleteUserAsync(long id, CancellationToken cancellationToken);
    Task<UserResponse> UpdateRoleAsync(long id, UpdateUserRoleRequest request, CancellationToken cancellationToken);
    Task<UserResponse> UpdateBranchesAsync(long id, UpdateUserBranchesRequest request, CancellationToken cancellationToken);
}

public sealed class UserService(
    XparfDbContext dbContext,
    ICurrentUserContext currentUserContext,
    IPasswordHasherService passwordHasher) : IUserService
{
    public async Task<IReadOnlyList<UserResponse>> GetUsersAsync(CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        return await dbContext.Users
            .Include(x => x.Role)
            .Include(x => x.UserBranches)
            .Where(x => x.CompanyId == companyId)
            .OrderBy(x => x.UserName)
            .Select(x => ToResponse(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        var email = request.Email.Trim().ToLowerInvariant();
        await EnsureEmailAvailableAsync(email, cancellationToken);
        await EnsureRoleExistsAsync(companyId, request.RoleId, cancellationToken);
        await EnsureBranchesExistAsync(companyId, request.BranchIds, cancellationToken);

        var user = new User
        {
            CompanyId = companyId,
            RoleId = request.RoleId,
            Email = email,
            UserName = request.UserName.Trim(),
            EmailConfirmed = false,
            IsOwner = false,
            IsActive = request.IsActive
        };
        user.PasswordHash = passwordHasher.Hash(user, request.Password);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        await ReplaceUserBranchesAsync(user.Id, request.BranchIds, request.BranchIds.FirstOrDefault(), cancellationToken);

        return await GetUserResponseAsync(companyId, user.Id, cancellationToken);
    }

    public async Task<UserResponse> InviteUserAsync(InviteUserRequest request, CancellationToken cancellationToken)
    {
        var randomPassword = $"Invite-{Guid.NewGuid():N}!";
        var createRequest = new CreateUserRequest(
            request.Email,
            request.UserName,
            randomPassword,
            request.RoleId,
            request.BranchIds,
            false);

        return await CreateUserAsync(createRequest, cancellationToken);
    }

    public async Task<UserResponse> UpdateUserAsync(long id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        var user = await GetCompanyUserAsync(companyId, id, cancellationToken);
        EnsureNotOwnerMutation(user);

        user.UserName = request.UserName.Trim();
        user.IsActive = request.IsActive;
        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetUserResponseAsync(companyId, id, cancellationToken);
    }

    public async Task DeleteUserAsync(long id, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        var user = await GetCompanyUserAsync(companyId, id, cancellationToken);
        EnsureNotOwnerMutation(user);

        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserResponse> UpdateRoleAsync(long id, UpdateUserRoleRequest request, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        var user = await GetCompanyUserAsync(companyId, id, cancellationToken);
        EnsureNotOwnerMutation(user);
        await EnsureRoleExistsAsync(companyId, request.RoleId, cancellationToken);

        user.RoleId = request.RoleId;
        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetUserResponseAsync(companyId, id, cancellationToken);
    }

    public async Task<UserResponse> UpdateBranchesAsync(long id, UpdateUserBranchesRequest request, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        var user = await GetCompanyUserAsync(companyId, id, cancellationToken);
        await EnsureBranchesExistAsync(companyId, request.BranchIds, cancellationToken);

        var defaultBranchId = request.DefaultBranchId ?? request.BranchIds.FirstOrDefault();
        await ReplaceUserBranchesAsync(user.Id, request.BranchIds, defaultBranchId, cancellationToken);

        return await GetUserResponseAsync(companyId, id, cancellationToken);
    }

    private long GetCompanyId() => currentUserContext.CompanyId
        ?? throw new UnauthorizedAccessException("Company context tidak ditemukan.");

    private async Task EnsureEmailAvailableAsync(string email, CancellationToken cancellationToken)
    {
        if (await dbContext.Users.AnyAsync(x => x.Email == email, cancellationToken))
        {
            throw new InvalidOperationException("Email sudah terdaftar.");
        }
    }

    private async Task EnsureRoleExistsAsync(long companyId, long roleId, CancellationToken cancellationToken)
    {
        var exists = await dbContext.Roles.AnyAsync(x => x.Id == roleId && (x.CompanyId == companyId || x.CompanyId == null), cancellationToken);
        if (!exists)
        {
            throw new InvalidOperationException("Role tidak ditemukan.");
        }
    }

    private async Task EnsureBranchesExistAsync(long companyId, long[] branchIds, CancellationToken cancellationToken)
    {
        if (branchIds.Length == 0)
        {
            return;
        }

        var distinctIds = branchIds.Distinct().ToArray();
        var count = await dbContext.Branches.CountAsync(x => x.CompanyId == companyId && distinctIds.Contains(x.Id), cancellationToken);
        if (count != distinctIds.Length)
        {
            throw new InvalidOperationException("Branch tidak valid.");
        }
    }

    private async Task ReplaceUserBranchesAsync(long userId, long[] branchIds, long defaultBranchId, CancellationToken cancellationToken)
    {
        var existing = await dbContext.UserBranches.Where(x => x.UserId == userId).ToListAsync(cancellationToken);
        dbContext.UserBranches.RemoveRange(existing);

        foreach (var branchId in branchIds.Distinct())
        {
            dbContext.UserBranches.Add(new UserBranch
            {
                UserId = userId,
                BranchId = branchId,
                IsDefault = branchId == defaultBranchId
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<User> GetCompanyUserAsync(long companyId, long id, CancellationToken cancellationToken)
    {
        return await dbContext.Users.FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("User tidak ditemukan.");
    }

    private async Task<UserResponse> GetUserResponseAsync(long companyId, long id, CancellationToken cancellationToken)
    {
        return await dbContext.Users
            .Include(x => x.Role)
            .Include(x => x.UserBranches)
            .Where(x => x.CompanyId == companyId && x.Id == id)
            .Select(x => ToResponse(x))
            .FirstAsync(cancellationToken);
    }

    private static void EnsureNotOwnerMutation(User user)
    {
        if (user.IsOwner)
        {
            throw new InvalidOperationException("Owner utama tidak boleh diubah lewat endpoint user.");
        }
    }

    private static UserResponse ToResponse(User user) => new(
        user.Id,
        user.Email,
        user.UserName,
        user.RoleId,
        user.Role.Name,
        user.EmailConfirmed,
        user.IsOwner,
        user.IsActive,
        user.UserBranches.Select(x => x.BranchId).Order().ToArray());
}
