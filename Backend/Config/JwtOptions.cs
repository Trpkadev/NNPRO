namespace Backend.Config;

public class JwtOptions {
    public const string SectionName = "Jwt";

    public required string SecretKey { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }

    // Krátká životnost Access Tokenu (např. 15 minut)
    public int AccessTokenExpirationMinutes { get; init; } = 15;

    // Dlouhá životnost Refresh Tokenu (např. 7 dní / 10080 minut)
    public int RefreshTokenExpirationDays { get; init; } = 7;
}