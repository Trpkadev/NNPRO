using Backend.Configuration;
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
    public async Task<CourseResponseDto> GetByIdAsync(uint id, uint userId, CancellationToken ct)
    {
        bool hasAccess = true;
        try
        {
            await CourseAuthorization.EnsureCanAccessCourseAsync(dbContext, id, userId, ct);
        }
        catch (ServiceException ex)
        {
            if (ex.StatusCode != 403)
                throw;

            hasAccess = false;
        }

        var course = await dbContext.Courses.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new CourseResponseDto(x.Id, x.Title, x.Description, hasAccess))
            .FirstOrDefaultAsync(ct);


        return course ?? throw ServiceException.NotFound(Resources.ErrorCourseNotFound);
    }

    public async Task<(CourseResponseDto[] Courses, Metadata Metadata)> GetAsync(RequestQuery requestDto, CancellationToken ct)
    {
        var (courses, paginationMetadata) = await dbContext.Courses.AsNoTracking()
            .Filter(requestDto.Filters)
            .OrderByProperty(requestDto.Sorting)
            .Select(x => new CourseResponseDto(x.Id, x.Title, x.Description, true))
            .ToPagedResultAsync(requestDto, dbLimitsOptions.Value.MaxItemsLoadedWithoutPagination, ct);

        return (courses, new Metadata(paginationMetadata, requestDto));
    }

    public async Task<(CourseThumbnailResponseDto[] Courses, Metadata Metadata)> GetCourseThumbnailsAsync(uint userId, CourseThumbnailRequestDto requestDto, CancellationToken ct)
    {
        var (rawCourses, paginationMetadata) = await dbContext.Courses.AsNoTracking()
            .Filter(requestDto.Filters)
            .OrderByProperty(requestDto.Sorting)
            .Select(x => new { x.Id, x.Title, x.Description })
            .ToPagedResultAsync(requestDto, dbLimitsOptions.Value.MaxItemsLoadedWithoutPagination, ct);

        var pageCourseIds = rawCourses.Select(x => x.Id);

        var enrolledCourseIds = rawCourses.Length > 0
            ? (await dbContext.Enrollments.AsNoTracking()
                .Where(x => x.StudentId == userId && pageCourseIds.Contains(x.CourseId))
                .Select(x => x.CourseId)
                .ToListAsync(ct))
            .ToHashSet()
            : [];

        bool hasMaxLength = requestDto.MaxDescriptionLength.HasValue;
        int maxLength = requestDto.MaxDescriptionLength ?? 0;

        var thumbnails = rawCourses.Select(x => new CourseThumbnailResponseDto(
            x.Id,
            x.Title,
            hasMaxLength && x.Description.Length > maxLength ? string.Concat(x.Description.AsSpan(0, maxLength), "...") : x.Description,
            enrolledCourseIds.Contains(x.Id)
        )).ToArray();

        return (thumbnails, new Metadata(paginationMetadata, requestDto));
    }

    public async Task<uint> CreateAsync(uint userId, CourseCreateRequestDto requestDto, CancellationToken ct)
    {
        var role = await dbContext.Users.AsNoTracking()
                       .Where(candidate => candidate.Id == userId)
                       .Select(candidate => (UserRole?)candidate.Role)
                       .FirstOrDefaultAsync(ct)
                   ?? throw ServiceException.NotFound(Resources.ErrorUserNotFound);

        if (role is not (UserRole.Admin or UserRole.Instructor))
            throw ServiceException.Forbidden(Resources.ErrorCreateCourseForbidden);

        Course course = new() {
            AccessKey = requestDto.AccessKey,
            Description = requestDto.Description,
            Title = requestDto.Title
        };

        if (role == UserRole.Instructor)
        {
            User instructorStub = new() { Id = userId };
            dbContext.Attach(instructorStub);
            course.Instructors.Add(instructorStub);
        }

        dbContext.Courses.Add(course);
        await dbContext.SaveChangesAsync(ct);

        return course.Id;
    }

    public async Task RemoveAsync(uint userId, uint id, CancellationToken ct)
    {
        await CourseAuthorization.EnsureCanManageCourseAsync(dbContext, userId, id, ct);
        await dbContext.Courses.Where(c => c.Id == id).ExecuteDeleteAsync(ct);
    }

    public async Task UpdateInstructorsAsync(uint userId, uint id, CourseUpdateInstructorsRequestDto requestDto, CancellationToken ct)
    {
        await CourseAuthorization.EnsureCanManageCourseAsync(dbContext, userId, id, ct);
        var course = await dbContext.Courses.FirstOrDefaultAsync(c => c.Id == id, ct) ?? throw ServiceException.NotFound(Resources.ErrorCourseNotFound);
        var instructors = await dbContext.Users.Where(x => requestDto.InstructorIds.AsEnumerable().Contains(x.Id)).ToArrayAsync(ct);
        course.Instructors = instructors;
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(uint userId, uint id, CourseUpdateRequestDto requestDto, CancellationToken ct)
    {
        await CourseAuthorization.EnsureCanManageCourseAsync(dbContext, userId, id, ct);
        await dbContext.Courses.Where(c => c.Id == id)
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(c => c.Title, requestDto.Title)
                    .SetProperty(c => c.Description, requestDto.Description)
                    .SetProperty(c => c.AccessKey, requestDto.AccessKey),
                ct);
    }

    public async Task<(CourseResponseDto[] Courses, Metadata Metadata)> GetManageableCoursesAsync(uint userId, RequestQuery request, CancellationToken ct)
    {
        var role = await dbContext.Users.AsNoTracking()
                       .Where(user => user.Id == userId)
                       .Select(user => (UserRole?)user.Role)
                       .FirstOrDefaultAsync(ct)
                   ?? throw ServiceException.NotFound(Resources.ErrorUserNotFound);

        var query = dbContext.Courses.AsNoTracking();

        if (role != UserRole.Admin)
            query = query.Where(course => course.Instructors.Any(instructor => instructor.Id == userId));

        var (courses, metadata) = await query.Filter(request.Filters)
            .OrderByProperty(request.Sorting)
            .Select(course => new CourseResponseDto(course.Id, course.Title, course.Description, true))
            .ToPagedResultAsync(request, dbLimitsOptions.Value.MaxItemsLoadedWithoutPagination, ct);

        return (courses, new Metadata(metadata, request));
    }

    public async Task<(CourseInstructorResponseDto[] Instructors, Metadata Metadata)> GetInstructorsAsync(uint courseId, RequestQuery request, CancellationToken ct)
    {
        if (!await dbContext.Courses.AnyAsync(course => course.Id == courseId, ct))
            throw ServiceException.NotFound(Resources.ErrorCourseNotFound);

        var (instructors, metadata) = await dbContext.Courses.AsNoTracking()
            .Where(course => course.Id == courseId)
            .SelectMany(course => course.Instructors.OrderBy(instructor => instructor.Id)
                .Select(instructor => new CourseInstructorResponseDto(instructor.Id, instructor.Username)))
            .ToPagedResultAsync(request, dbLimitsOptions.Value.MaxItemsLoadedWithoutPagination, ct);

        return (instructors, new Metadata(metadata, request));
    }

    public async Task<(SectionResponseDto[] Sections, Metadata Metadata)> GetSectionsAsync(uint courseId, uint userId, RequestQuery request, CancellationToken ct)
    {
        await CourseAuthorization.EnsureCanAccessCourseAsync(dbContext, courseId, userId, ct);

        var (sections, metadata) = await dbContext.Sections.AsNoTracking()
            .Where(section => section.CourseId == courseId)
            .OrderBy(section => section.Order)
            .ThenBy(section => section.Id)
            .Select(section => new SectionResponseDto(section.Id, section.Title, section.Order, section.CourseId))
            .ToPagedResultAsync(request, dbLimitsOptions.Value.MaxItemsLoadedWithoutPagination, ct);

        return (sections, new Metadata(metadata, request));
    }
}