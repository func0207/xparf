namespace Xparf.Api.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; set; } = "xparf";
    public string Audience { get; set; } = "xparf";
    public string SigningKey { get; set; } = "dev-only-change-this-signing-key-minimum-32-chars";
    public int AccessTokenMinutes { get; set; } = 60;
    public int RefreshTokenDays { get; set; } = 30;
}
