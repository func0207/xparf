using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xparf.Api.Contracts.Auth;
using Xparf.Api.Options;
using Xparf.Core.Entities;
using Xparf.Infrastructure.Persistence;

namespace Xparf.Api.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterCompanyAsync(RegisterCompanyRequest request, CancellationToken cancellationToken);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<AuthResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken);
    Task<AuthMessageResponse> LogoutAsync(LogoutRequest request, CancellationToken cancellationToken);
    Task<AuthMessageResponse> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken);
    Task<AuthMessageResponse> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken);
    Task<AuthMessageResponse> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken);
}

public sealed class AuthService(
    XparfDbContext dbContext,
    IPasswordHasherService passwordHasher,
    ITokenService tokenService,
    IOptions<JwtOptions> jwtOptions) : IAuthService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<AuthResponse> RegisterCompanyAsync(RegisterCompanyRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await dbContext.Users.AnyAsync(x => x.Email == email, cancellationToken))
        {
            throw new InvalidOperationException("Email sudah terdaftar.");
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var company = new Company
        {
            Name = request.CompanyName.Trim(),
            Email = email,
            Phone = request.Phone,
            Address = request.Address
        };
        dbContext.Companies.Add(company);
        await dbContext.SaveChangesAsync(cancellationToken);

        var ownerRole = new Role
        {
            CompanyId = company.Id,
            Name = "Owner",
            Description = "Owner company",
            IsSystemRole = true
        };
        dbContext.Roles.Add(ownerRole);
        await dbContext.SaveChangesAsync(cancellationToken);

        var branch = new Branch
        {
            CompanyId = company.Id,
            Code = "MAIN",
            Name = "Main Branch",
            IsActive = true
        };
        dbContext.Branches.Add(branch);

        var user = new User
        {
            CompanyId = company.Id,
            RoleId = ownerRole.Id,
            Email = email,
            UserName = request.UserName.Trim(),
            EmailConfirmed = false,
            IsOwner = true,
            IsActive = true
        };
        user.PasswordHash = passwordHasher.Hash(user, request.Password);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        dbContext.UserBranches.Add(new UserBranch { UserId = user.Id, BranchId = branch.Id, IsDefault = true });
        await dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return await CreateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken)
            ?? throw new UnauthorizedAccessException("Email atau password salah.");

        if (!user.IsActive || !passwordHasher.Verify(user, user.PasswordHash, request.Password))
        {
            throw new UnauthorizedAccessException("Email atau password salah.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return await CreateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var refreshTokenHash = tokenService.HashRefreshToken(request.RefreshToken);
        var token = await dbContext.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == refreshTokenHash, cancellationToken)
            ?? throw new UnauthorizedAccessException("Refresh token tidak valid.");

        if (token.IsRevoked || token.ExpiresAt <= DateTime.UtcNow || !token.User.IsActive)
        {
            throw new UnauthorizedAccessException("Refresh token tidak valid.");
        }

        token.RevokedAt = DateTime.UtcNow;
        token.RevokedReason = "Rotated";
        await dbContext.SaveChangesAsync(cancellationToken);

        return await CreateAuthResponseAsync(token.User, cancellationToken);
    }

    public async Task<AuthMessageResponse> LogoutAsync(LogoutRequest request, CancellationToken cancellationToken)
    {
        var refreshTokenHash = tokenService.HashRefreshToken(request.RefreshToken);
        var token = await dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == refreshTokenHash, cancellationToken);
        if (token is not null && !token.IsRevoked)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedReason = "Logout";
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        return new AuthMessageResponse("Logout berhasil.");
    }

    public async Task<AuthMessageResponse> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken) ?? throw new InvalidOperationException("User tidak ditemukan.");
        user.EmailConfirmed = true;
        await dbContext.SaveChangesAsync(cancellationToken);
        return new AuthMessageResponse("Email confirmed.");
    }

    public Task<AuthMessageResponse> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new AuthMessageResponse("Jika email terdaftar, instruksi reset password akan dikirim."));
    }

    public async Task<AuthMessageResponse> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken) ?? throw new InvalidOperationException("User tidak ditemukan.");
        user.PasswordHash = passwordHasher.Hash(user, request.NewPassword);
        foreach (var token in await dbContext.RefreshTokens.Where(x => x.UserId == user.Id && x.RevokedAt == null).ToListAsync(cancellationToken))
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedReason = "Password reset";
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        return new AuthMessageResponse("Password reset berhasil.");
    }

    private async Task<AuthResponse> CreateAuthResponseAsync(User user, CancellationToken cancellationToken)
    {
        var accessToken = tokenService.CreateAccessToken(user);
        var refreshToken = tokenService.CreateRefreshToken();
        var refreshTokenHash = tokenService.HashRefreshToken(refreshToken);

        dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays)
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            accessToken.Token,
            refreshToken,
            accessToken.ExpiresAt,
            user.Id,
            user.CompanyId,
            user.Email,
            user.UserName,
            user.IsOwner,
            user.IsSuperAdmin);
    }
}
