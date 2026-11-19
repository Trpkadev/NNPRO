using Backend.Data;
using Backend.Exceptions;
using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class UserService(AppDbContext dbContext, IAuthService authService) : IUserService {
    public Task<UserCourseResponseDto[]> GetMyCoursesAsync(uint userId, CancellationToken ct) =>
        dbContext.Enrollments.AsNoTracking()
            .Where(enrollment => enrollment.StudentId == userId)
            .OrderBy(enrollment => enrollment.CourseId)
            .Select(enrollment => new UserCourseResponseDto(enrollment.Course.Id, enrollment.Course.Title, enrollment.Course.Description))
            .ToArrayAsync(ct);

    public Task<UserLessonProgressResponseDto[]> GetMyProgressAsync(uint userId, CancellationToken ct) =>
        dbContext.StudentLessonProgresses.AsNoTracking()
            .Where(progress => progress.StudentId == userId)
            .OrderBy(progress => progress.LessonId)
            .Select(progress => new UserLessonProgressResponseDto(
                progress.LessonId,
                progress.Lesson.Title,
                progress.Lesson.SectionId,
                progress.Lesson.Section.CourseId,
                progress.IsCompleted,
                progress.CompletedAtUtc))
            .ToArrayAsync(ct);

    public async Task UpdateAsync(uint userId, UpdateUserRequestDto request, CancellationToken ct)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(candidate => candidate.Id == userId, ct);
        if (user is null)
            throw ServiceException.NotFound(Resources.ErrorUserNotFound);

        bool duplicateIdentity = await dbContext.Users.AsNoTracking()
            .AnyAsync(candidate => candidate.Id != userId
                                   && (candidate.Username == request.Username || candidate.Email == request.Email),
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