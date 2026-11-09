using Backend.Models.DTOs.Request;

namespace Backend.Models.DTOs.Response;

/// <summary>
///     Standard API response wrapper with optional metadata.
/// </summary>
public interface IApiResponseWrapper {
    Metadata? Metadata { get; set; }
}

public record Wrapper<T>(T Data, Metadata? Metadata = null) : IApiResponseWrapper {
    public T Data { get; } = Data;
    public Metadata? Metadata { get; set; } = Metadata;
}

public record Metadata {
    public Metadata() {}

    public Metadata(PaginationMetadata pagination, RequestQuery query)
    {
        Pagination = pagination;
        Sorting = query.Sorting;
        Filters = query.FilterMetadata;
    }

    public PaginationMetadata? Pagination { get; set; }
    public SortingMetadata? Sorting { get; set; }
    public FilterMetadata? Filters { get; set; }
    public ServerMetadata? ServerInfo { get; set; }
}

public record PaginationMetadata {
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages => PageSize > 0 && TotalItems > 0 ? 1 + (TotalItems - 1) / PageSize : 0;
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

public record SortingMetadata {
    public string? SortBy { get; init; }
    public bool IsDescending { get; init; } = false;
}

public record FilterMetadata {
    public Dictionary<string, string> AppliedFilters { get; init; } = [];
}

public record ServerMetadata {
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    public long ExecutionTimeMs { get; init; }
}