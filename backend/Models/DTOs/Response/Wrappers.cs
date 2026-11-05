namespace Backend.Models.DTOs.Response;

/// <summary>
/// Standard API response wrapper with optional metadata.
/// </summary>
public interface IApiResponseWrapper {
    Metadata? Metadata { get; set; }
}

public class Wrapper<T>(T data, Metadata? metadata = null) : IApiResponseWrapper {
    public T Data { get; } = data;
    public Metadata? Metadata { get; set; } = metadata;
}

public class Metadata {
    public PaginationMetadata? Pagination { get; set; }
    public SortingMetadata? Sorting { get; set; }
    public FilterMetadata? Filters { get; set; }
    public ServerMetadata? ServerInfo { get; set; }
}

public record PaginationMetadata 
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; } 
    public int TotalPages => PageSize > 0 ? (TotalItems + PageSize - 1) / PageSize : 0;
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

public class SortingMetadata {
    public string SortBy { get; set; } = string.Empty;
    public bool IsDescending { get; set; }
}

public class FilterMetadata {
    public string? SearchQuery { get; set; }
    public Dictionary<string, string> AppliedFilters { get; set; } = [];
}

public class ServerMetadata {
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public long ExecutionTimeMs { get; set; }
}