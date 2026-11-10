namespace Backend.Config;

public class JwtOptions {
    public const string SectionName = "Jwt";

    public required string SecretKey { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }

    // Access-token lifetime in minutes.
    public int AccessTokenExpirationMinutes { get; init; } = 15;

    // Refresh-token lifetime in days.
    public int RefreshTokenExpirationDays { get; init; } = 7;
}