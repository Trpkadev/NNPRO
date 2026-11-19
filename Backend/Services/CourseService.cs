using Backend.Config;
using Backend.Data;
using Backend.Exceptions;
using Backend.Extensions;
using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Backend.Models.Entities;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Backend.Services;

public class CourseService(AppDbContext dbContext, IOptions<DatabaseLimitsOptions> dbLimitsOptions) : ICourseService {
    public async Task<CourseResponseDto> GetByIdAsync(uint id, CancellationToken ct)
    {
        var course = await dbContext.Courses.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new CourseResponseDto(x.Id, x.Title, x.Description))
            .FirstOrDefaultAsync(ct);
        return course ?? throw ServiceException.NotFound(Resources.ErrorCourseNotFound);
    }

    public async Task<(CourseResponseDto[] Courses, Metadata Metadata)> GetAsync(RequestQuery requestDto, CancellationToken ct)
    {
        var (courses, paginationMetadata) = await dbContext.Courses.AsNoTracking()
            .Filter(requestDto.Filters)
            .OrderByProperty(requestDto.Sorting)
            .Select(x => new CourseResponseDto(x.Id, x.Title, x.Description))
            .ToPagedResultAsync(requestDto, dbLimitsOptions.Value.MaxItemsLoadedWithoutPagination, ct);

        return (courses, new Metadata(paginationMetadata, requestDto));
    }

    public async Task<(CourseThumbnailResponseDto[] Courses, Metadata Metadata)> GetCourseThumbnailsAsync(CourseThumbnailRequestDto requestDto, CancellationToken ct)
    {
        var (rawCourses, paginationMetadata) = await dbContext.Courses.AsNoTracking()
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

    public async Task<uint> CreateAsync(uint requestingUserId, CourseCreateRequestDto requestDto, CancellationToken ct)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(candidate => candidate.Id == requestingUserId, ct);
        if (user is null)
            throw ServiceException.NotFound(Resources.ErrorUserNotFound);
        if (user.Role is not (UserRole.Admin or UserRole.Instructor))
            throw ServiceException.Forbidden(Resources.ErrorCreateCourseForbidden);

        Course course = new() {
            AccessKey = requestDto.AccessKey,
            Description = requestDto.Description,
            Title = requestDto.Title
        };
        if (user.Role == UserRole.Instructor)
            course.Instructors.Add(user);

        dbContext.Courses.Add(course);
        await dbContext.SaveChangesAsync(ct);

        return course.Id;
    }

    public async Task RemoveAsync(uint requestingUserId, uint id, CancellationToken ct)
    {
        await CourseAuthorization.EnsureCanManageCourseAsync(dbContext, requestingUserId, id, ct);
        await dbContext.Courses.Where(c => c.Id == id).ExecuteDeleteAsync(ct);
    }

    public async Task UpdateAsync(uint requestingUserId, uint id, CourseUpdateRequestDto requestDto, CancellationToken ct)
    {
        await CourseAuthorization.EnsureCanManageCourseAsync(dbContext, requestingUserId, id, ct);
        await dbContext.Courses.Where(c => c.Id == id)
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(c => c.Title, requestDto.Title)
                    .SetProperty(c => c.Description, requestDto.Description)
                    .SetProperty(c => c.AccessKey, requestDto.AccessKey),
                ct);
    }

    public async Task<CourseInstructorResponseDto[]> GetInstructorsAsync(uint courseId, CancellationToken ct)
    {
        if (!await dbContext.Courses.AnyAsync(course => course.Id == courseId, ct))
            throw ServiceException.NotFound(Resources.ErrorCourseNotFound);

        return await dbContext.Courses.AsNoTracking()
            .Where(course => course.Id == courseId)
            .SelectMany(course => course.Instructors.OrderBy(instructor => instructor.Id)
                .Select(instructor => new CourseInstructorResponseDto(instructor.Id, instructor.Username)))
            .ToArrayAsync(ct);
    }

    public async Task<SectionResponseDto[]> GetSectionsAsync(uint courseId, CancellationToken ct)
    {
        if (!await dbContext.Courses.AnyAsync(course => course.Id == courseId, ct))
            throw ServiceException.NotFound(Resources.ErrorCourseNotFound);

        return await dbContext.Sections.AsNoTracking()
            .Where(section => section.CourseId == courseId)
            .OrderBy(section => section.Order)
            .ThenBy(section => section.Id)
            .Select(section => new SectionResponseDto(section.Id, section.Title, section.Order, section.CourseId))
            .ToArrayAsync(ct);
    }
}