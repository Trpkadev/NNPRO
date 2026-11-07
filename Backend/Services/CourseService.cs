using Backend.Data;
using Backend.Models.DTOs.Response;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Backend.Extensions;
using Backend.Models.DTOs.Request;

namespace Backend.Services;

public class CourseService(IDbContextFactory<AppDbContext> dbContextFactory) : ICourseService {
    public Task<CourseResponseDto?> GetByIdAsync(uint id, CancellationToken ct) => throw new NotImplementedException();

    public async Task<CourseThumbnailDto[]> GetCourseThumbnailsAsync(CourseThumbnailQuery query, CancellationToken ct)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

        var coursesQuery = dbContext.Courses.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
            coursesQuery = coursesQuery.Where(x => x.Title.Contains(query.Search) || x.Description.Contains(query.Search));

        var courses = await coursesQuery.OrderByProperty(query.SortBy, query.Descending)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new { x.Title, x.Description })
            .ToArrayAsync(ct);

        return [
            .. courses.Select(x => new CourseThumbnailDto(x.Title,
                query.MaxDescriptionLength.HasValue && x.Description.Length > query.MaxDescriptionLength.Value
                    ? string.Concat(x.Description.AsSpan(0, query.MaxDescriptionLength.Value), "...")
                    : x.Description
            ))
        ];
    }
}