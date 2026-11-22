namespace Backend.Configuration;

public sealed record Config {
    public ushort NetworkDelayToleranceSeconds { get; init; } = 5;
}