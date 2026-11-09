using Backend.Config;
using Backend.Data;
using Backend.Extensions;
using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Backend.Models.Entities;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Backend.Services;

public class CourseService(AppDbContext dbContext, IOptions<DatabaseLimitsOptions> dbLimitsOptions) : ICourseService {
    public async Task<CourseResponseDto?> GetByIdAsync(uint id, CancellationToken ct)
    {
        return await dbContext.Courses.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new CourseResponseDto(x.Id, x.Title, x.Description))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<(CourseResponseDto[] Courses, Metadata Metadata)> GetAsync(RequestQuery requestDto, CancellationToken ct)
    {
        var (courses, paginationMetadata) = await dbContext.Courses.AsNoTracking()
            .ApplyCourseSearch(requestDto.Search)
            .Filter(requestDto.Filters)
            .OrderByProperty(requestDto.Sorting)
            .Select(x => new CourseResponseDto(x.Id, x.Title, x.Description))
            .ToPagedResultAsync(requestDto, dbLimitsOptions.Value.MaxItemsLoadedWithoutPagination, ct);

        return (courses, new Metadata(paginationMetadata, requestDto));
    }

    public async Task<(CourseThumbnailResponseDto[] Courses, Metadata Metadata)> GetCourseThumbnailsAsync(CourseThumbnailRequestDto requestDto, CancellationToken ct)
    {
        var (rawCourses, paginationMetadata) = await dbContext.Courses.AsNoTracking()
            .ApplyCourseSearch(requestDto.Search)
            .Filter(requestDto.Filters)
            .OrderByProperty(requestDto.Sorting)
            .Select(x => new { x.Id, x.Title, x.Description })
            .ToPagedResultAsync(requestDto, dbLimitsOptions.Value.MaxItemsLoadedWithoutPagination, ct);

        bool hasMaxLength = requestDto.MaxDescriptionLength.HasValue;
        int maxLength = requestDto.MaxDescriptionLength ?? 0;

        var thumbnails = rawCourses.Select(x => new CourseThumbnailResponseDto(
            x.Id,
            x.Title,
            hasMaxLength && x.Description.Length > maxLength ? string.Concat(x.Description.AsSpan(0, maxLength), "...") : x.Description
        )).ToArray();

        return (thumbnails, new Metadata(paginationMetadata, requestDto));
    }

    public async Task<uint> CreateAsync(CourseCreateRequestDto requestDto, CancellationToken ct)
    {
        Course course = new() {
            AccessKey = requestDto.AccessKey,
            Description = requestDto.Description,
            Title = requestDto.Title
        };

        dbContext.Courses.Add(course);
        await dbContext.SaveChangesAsync(ct);

        return course.Id;
    }

    public async Task<bool> RemoveAsync(uint id, CancellationToken ct)
    {
        int deletedRows = await dbContext.Courses.Where(c => c.Id == id).ExecuteDeleteAsync(ct);
        return deletedRows > 0;
    }

    public async Task<bool> UpdateAsync(uint id, CourseUpdateRequestDto requestDto, CancellationToken ct)
    {
        int updatedRows = await dbContext.Courses.Where(c => c.Id == id)
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(c => c.Title, requestDto.Title)
                    .SetProperty(c => c.Description, requestDto.Description)
                    .SetProperty(c => c.AccessKey, requestDto.AccessKey),
                ct);

        return updatedRows > 0;
    }
}