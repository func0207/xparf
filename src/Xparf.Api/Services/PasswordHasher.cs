using Microsoft.AspNetCore.Identity;
using Xparf.Core.Entities;

namespace Xparf.Api.Services;

public interface IPasswordHasherService
{
    string Hash(User user, string password);
    bool Verify(User user, string passwordHash, string password);
}

public sealed class PasswordHasherService(IPasswordHasher<User> passwordHasher) : IPasswordHasherService
{
    public string Hash(User user, string password) => passwordHasher.HashPassword(user, password);

    public bool Verify(User user, string passwordHash, string password)
    {
        var result = passwordHasher.VerifyHashedPassword(user, passwordHash, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
