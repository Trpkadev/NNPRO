namespace Backend.Configuration;

public sealed record DatabaseLimitsOptions {
    public int MaxItemsLoadedWithoutPagination { get; init; } = 100;
}