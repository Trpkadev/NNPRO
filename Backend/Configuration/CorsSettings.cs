namespace Backend.Configuration;

public sealed record CorsSettings {
    public const string SectionName = "Cors";

    public string[] AllowedOrigins { get; init; } = [];
}