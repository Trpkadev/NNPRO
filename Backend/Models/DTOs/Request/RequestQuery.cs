using System.ComponentModel.DataAnnotations;
using Backend.Extensions;
using Backend.Models.DTOs.Response;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Models.DTOs.Request;

public record RequestQuery : IValidatableObject {
    [Range(1, int.MaxValue)]
    public int? Page { get; init; }

    [Range(1, 100)]
    public int? PageSize { get; init; }

    public string? SortBy { get; init; } = "Id";
    public bool? Descending { get; init; }

    [FromQuery(Name = "Filters")]
    [ModelBinder(BinderType = typeof(JsonOrDictionaryModelBinder))]
    public Dictionary<string, string> Filters { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    internal SortingMetadata Sorting => new() {
        SortBy = string.IsNullOrWhiteSpace(SortBy) ? "Id" : SortBy,
        IsDescending = Descending ?? false
    };

    internal FilterMetadata FilterMetadata => new() {
        AppliedFilters = Filters
    };

    public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Page.HasValue == PageSize.HasValue)
            yield break;

        List<string> invalidMemberNames = [];

        if (!Page.HasValue)
            invalidMemberNames.Add(nameof(Page));

        if (!PageSize.HasValue)
            invalidMemberNames.Add(nameof(PageSize));

        yield return new ValidationResult(Resources.ErrorPagingInvalidParams, invalidMemberNames);
    }
}