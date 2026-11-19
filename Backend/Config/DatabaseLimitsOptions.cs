namespace Backend.Config;

public sealed record DatabaseLimitsOptions {
    public const string SectionName = "DatabaseLimits";

    public int MaxItemsLoadedWithoutPagination { get; init; } = 100;
}