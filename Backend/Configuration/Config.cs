namespace Backend.Configuration;

public sealed record Config {
    public const string SectionName = "Config";

    public ushort NetworkDelayToleranceSeconds { get; init; } = 5;
}