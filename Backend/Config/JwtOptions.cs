namespace Backend.Config;

public class JwtOptions {
    public const string SectionName = "Jwt";

    public required string SecretKey { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }

    public int AccessTokenExpirationMinutes { get; init; }
    public int RefreshTokenExpirationDays { get; init; }
}