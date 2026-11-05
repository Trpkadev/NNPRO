using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs.Request;

public sealed class CourseThumbnailQuery {
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 10;

    public string? Search { get; init; }
    public ushort? MaxDescriptionLength { get; init; }

    public string SortBy { get; init; } = "Id";

    public bool Descending { get; init; }
}