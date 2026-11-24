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
using StackExchange.Redis;

namespace Backend.Services;

public class UserService(AppDbContext dbContext, IAuthService authService, IOptions<DatabaseLimitsOptions> dbLimitsOptions) : IUserService {
    public async Task<(UserCourseResponseDto[] Courses, Metadata Metadata)> GetMyCoursesAsync(uint userId, RequestQuery request, CancellationToken ct)
    {
        var (courses, metadata) = await dbContext.Enrollments.AsNoTracking()
            .Where(enrollment => enrollment.StudentId == userId)
            .Filter(request.Filters)
            .OrderByProperty(request.Sorting)
            .Select(enrollment => new UserCourseResponseDto(enrollment.Course.Id, enrollment.Course.Title, enrollment.Course.Description))
            .ToPagedResultAsync(request, dbLimitsOptions.Value.MaxItemsLoadedWithoutPagination, ct);

        return (courses, new Metadata(metadata, request));
    }

    public async Task<(UserResponseDto[] Users, Metadata Metadata)> GetUsersAsync(uint userId, RequestQuery request, CancellationToken ct)
    {
        var role = await dbContext.Users.AsNoTracking()
                       .Where(user => user.Id == userId)
                       .Select(user => (UserRole?)user.Role)
                       .FirstOrDefaultAsync(ct)
                   ?? throw ServiceException.NotFound(Resources.ErrorUserNotFound);

        var query = dbContext.Users.AsNoTracking();

        if (role == UserRole.Instructor)
            query = query.Where(user => user.Role != UserRole.Admin);

        var (users, pagination) = await query
            .Filter(request.Filters)
            .OrderByProperty(request.Sorting)
            .Select(user => new UserResponseDto(user.Id, user.Username, user.Email, user.Role.ToString()))
            .ToPagedResultAsync(request, dbLimitsOptions.Value.MaxItemsLoadedWithoutPagination, ct);

        return (users, new Metadata(pagination, request));
    }

    public async Task<(UserLessonProgressResponseDto[] Progress, Metadata Metadata)> GetMyProgressAsync(uint userId, RequestQuery request, CancellationToken ct)
    {
        var (progress, metadata) = await dbContext.StudentLessonProgresses.AsNoTracking()
            .Where(progress => progress.StudentId == userId)
            .Filter(request.Filters)
            .OrderByProperty(request.Sorting)
            .Select(progress => new UserLessonProgressResponseDto(
                progress.LessonId,
                progress.Lesson.Title,
                progress.Lesson.SectionId,
                progress.Lesson.Section.CourseId,
                progress.IsCompleted,
                progress.CompletedAtUtc))
            .ToPagedResultAsync(request, dbLimitsOptions.Value.MaxItemsLoadedWithoutPagination, ct);

        return (progress, new Metadata(metadata, request));
    }

    public async Task UpdateAsync(uint userId, UpdateUserRequestDto request, CancellationToken ct)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(candidate => candidate.Id == userId, ct);
        if (user is null)
            throw ServiceException.NotFound(Resources.ErrorUserNotFound);

        bool duplicateIdentity = await dbContext.Users.AsNoTracking()
            .AnyAsync(candidate => candidate.Id != userId && (candidate.Username == request.Username || candidate.Email == request.Email),
                ct);
        if (duplicateIdentity)
            throw ServiceException.Conflict(Resources.ErrorUserIdentityAlreadyInUse);

        user.Username = request.Username;
        user.Email = request.Email;
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        await dbContext.SaveChangesAsync(ct);
        await authService.RevokeAllSessionsAsync(userId, null, ct);
    }

    public async Task DeleteAsync(uint userId, CancellationToken ct)
    {
        bool userExists = await dbContext.Users.AnyAsync(user => user.Id == userId, ct);
        if (!userExists)
            throw ServiceException.NotFound(Resources.ErrorUserNotFound);

        await authService.RevokeAllSessionsAsync(userId, null, ct);
        await dbContext.Users.Where(user => user.Id == userId).ExecuteDeleteAsync(ct);
    }
}