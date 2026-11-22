namespace Backend.Configuration;

public sealed record CorsSettings {
    public string[] AllowedOrigins { get; init; } = [];
}