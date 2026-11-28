using Backend.Exceptions;
using Backend.Extensions;
using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Backend.Models.Entities;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class UserService(ServiceContext<UserService> ctx, IAuthService authService) : IUserService {
    public async Task<(UserCourseResponseDto[] Courses, Metadata Metadata)> GetMyCoursesAsync(RequestQuery request, CancellationToken ct)
    {
        var (courses, metadata) = await ctx.Db.Enrollments.AsNoTracking()
            .Where(enrollment => enrollment.StudentId == ctx.AuthUserId)
            .Filter(request.Filters)
            .OrderByProperty(request.Sorting)
            .Select(enrollment => new UserCourseResponseDto(enrollment.Course.Id, enrollment.Course.Title, enrollment.Course.Description))
            .ToPagedResultAsync(request, ctx.MaxUnpagedItems, ct);

        return (courses, new Metadata(metadata, request));
    }

    public async Task<(UserResponseDto[] Users, Metadata Metadata)> GetUsersAsync(RequestQuery request, CancellationToken ct)
    {
        var role = await ctx.Db.Users.AsNoTracking()
                       .Where(user => user.Id == ctx.AuthUserId)
                       .Select(user => (UserRole?)user.Role)
                       .FirstOrDefaultAsync(ct)
                   ?? throw ServiceException.NotFound(Resources.ErrorUserNotFound);

        var query = ctx.Db.Users.AsNoTracking();

        if (role == UserRole.Instructor)
            query = query.Where(user => user.Role != UserRole.Admin);

        var (users, pagination) = await query
            .Filter(request.Filters)
            .OrderByProperty(request.Sorting)
            .Select(user => new UserResponseDto(user.Id, user.Username, user.Email, user.Role.ToString()))
            .ToPagedResultAsync(request, ctx.MaxUnpagedItems, ct);

        return (users, new Metadata(pagination, request));
    }

    public async Task<(UserLessonProgressResponseDto[] Progress, Metadata Metadata)> GetMyProgressAsync(RequestQuery request, CancellationToken ct)
    {
        var (progress, metadata) = await ctx.Db.StudentLessonProgresses.AsNoTracking()
            .Where(progress => progress.StudentId == ctx.AuthUserId)
            .Filter(request.Filters)
            .OrderByProperty(request.Sorting)
            .Select(progress => new UserLessonProgressResponseDto(
                progress.LessonId,
                progress.Lesson.Title,
                progress.Lesson.SectionId,
                progress.Lesson.Section.CourseId,
                progress.IsCompleted,
                progress.CompletedAtUtc))
            .ToPagedResultAsync(request, ctx.MaxUnpagedItems, ct);

        return (progress, new Metadata(metadata, request));
    }

    public async Task UpdateAsync(uint userId, UpdateUserRequestDto request, CancellationToken ct)
    {
        var user = await ctx.Db.Users.FirstOrDefaultAsync(candidate => candidate.Id == userId, ct);
        if (user is null)
            throw ServiceException.NotFound(Resources.ErrorUserNotFound);

        bool duplicateIdentity = await ctx.Db.Users.AsNoTracking()
            .AnyAsync(candidate => candidate.Id != userId && (candidate.Username == request.Username || candidate.Email == request.Email),
                ct);
        if (duplicateIdentity)
            throw ServiceException.Conflict(Resources.ErrorUserIdentityAlreadyInUse);

        user.Username = request.Username;
        user.Email = request.Email;
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        await ctx.Db.SaveChangesAsync(ct);
        await authService.RevokeAllSessionsAsync(userId, null, ct);
    }

    public async Task UpdateAsync(UpdateUserRequestDto request, CancellationToken ct) => await UpdateAsync(ctx.AuthUserId, request, ct);

    public async Task DeleteAsync(uint userId, CancellationToken ct)
    {
        bool userExists = await ctx.Db.Users.AnyAsync(user => user.Id == ctx.AuthUserId, ct);
        if (!userExists)
            throw ServiceException.NotFound(Resources.ErrorUserNotFound);

        await authService.RevokeAllSessionsAsync(ctx.AuthUserId, null, ct);
        await ctx.Db.Users.Where(user => user.Id == ctx.AuthUserId).ExecuteDeleteAsync(ct);
    }

    public async Task DeleteAsync(CancellationToken ct) => await DeleteAsync(ctx.AuthUserId, ct);
}