using Backend.Config;
using Backend.Data;
using Backend.Extensions;
using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Backend.Models.Entities;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class CourseService(IDbContextFactory<AppDbContext> dbContextFactory, DatabaseLimitsOptions dbLimits) : ICourseService {
    public async Task<CourseResponseDto?> GetByIdAsync(uint id, CancellationToken ct)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

        return await dbContext.Courses.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new CourseResponseDto(x.Id, x.Title, x.Description, x.AccessKey))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<(CourseResponseDto[] Courses, Metadata Metadata)> GetAsync(RequestQuery requestDto, CancellationToken ct)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

        var (courses, paginationMetadata) = await dbContext.Courses.AsNoTracking()
            .ApplyCourseSearch(requestDto.Search)
            .Filter(requestDto.Filters)
            .OrderByProperty(requestDto.Sorting)
            .Select(x => new CourseResponseDto(x.Id, x.Title, x.Description, x.AccessKey))
            .ToPagedResultAsync(requestDto, dbLimits.MaxItemsLoadedWithoutPagination, ct);


        return (courses, new Metadata { Pagination = paginationMetadata, Sorting = requestDto.Sorting, Filters = requestDto.FilterMetadata });
    }

    public async Task<(CourseThumbnailDto[]Courses, Metadata Metadata)> GetCourseThumbnailsAsync(CourseThumbnailRequestDto requestDto, CancellationToken ct)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

        var (rawCourses, paginationMetadata) = await dbContext.Courses.AsNoTracking()
            .ApplyCourseSearch(requestDto.Search)
            .Filter(requestDto.Filters)
            .OrderByProperty(requestDto.SortBy, requestDto.Descending)
            .Select(x => new { x.Id, x.Title, x.Description })
            .ToPagedResultAsync(requestDto, dbLimits.MaxItemsLoadedWithoutPagination, ct);

        CourseThumbnailDto[] a = [
            .. rawCourses.Select(x => new CourseThumbnailDto(
                x.Id,
                x.Title,
                requestDto.MaxDescriptionLength.HasValue && x.Description.Length > requestDto.MaxDescriptionLength.Value
                    ? string.Concat(x.Description.AsSpan(0, requestDto.MaxDescriptionLength.Value), "...")
                    : x.Description
            ))
        ];
        return (a, new Metadata { Pagination = paginationMetadata, Sorting = requestDto.Sorting, Filters = requestDto.FilterMetadata });
    }

    public async Task<uint> CreateAsync(CourseCreateRequestDto requestDto, CancellationToken ct)
    {
        Course course = new() {
            AccessKey = requestDto.AccessKey,
            Description = requestDto.Description,
            Title = requestDto.Title
        };

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

        await dbContext.Courses.AddAsync(course, ct);
        await dbContext.SaveChangesAsync(ct);

        return course.Id;
    }

    public async Task<bool> RemoveAsync(uint id, CancellationToken ct)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

        int deletedRows = await dbContext.Courses.Where(c => c.Id == id).ExecuteDeleteAsync(ct);
        return deletedRows > 0;
    }

    public async Task<bool> UpdateAsync(uint id, CourseUpdateRequestDto requestDto, CancellationToken ct)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

        int updatedRows = await dbContext.Courses.Where(c => c.Id == id)
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(c => c.Title, requestDto.Title)
                    .SetProperty(c => c.Description, requestDto.Description)
                    .SetProperty(c => c.AccessKey, requestDto.AccessKey),
                ct);

        return updatedRows > 0;
    }
}