using Backend.Data;
using Backend.Exceptions;
using Backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

internal static class CourseAuthorization {
    public async static Task EnsureCanManageCourseAsync(AppDbContext dbContext, uint courseId, uint userId, CancellationToken ct)
    {
        var check = await dbContext.Users.AsNoTracking()
                        .Where(u => u.Id == userId)
                        .Select(u => new {
                            IsAdmin = u.Role == UserRole.Admin,
                            IsInstructor = u.Role == UserRole.Instructor,
                            CourseExists = dbContext.Courses.Any(c => c.Id == courseId),
                            IsCourseInstructor = dbContext.Courses.Any(c => c.Id == courseId && c.Instructors.Any(i => i.Id == userId))
                        })
                        .FirstOrDefaultAsync(ct)
                    ?? throw ServiceException.NotFound(Resources.ErrorUserNotFound);

        if (!check.CourseExists)
            throw ServiceException.NotFound(Resources.ErrorCourseNotFound);

        if (!check.IsAdmin && !(check.IsInstructor && check.IsCourseInstructor))
            throw ServiceException.Forbidden(Resources.ErrorCourseManagementForbidden);
    }

    public async static Task EnsureCanAccessCourseAsync(AppDbContext dbContext, uint courseId, uint userId, CancellationToken ct)
    {
        var check = await dbContext.Courses.AsNoTracking()
                        .Where(c => c.Id == courseId)
                        .Select(c => new {
                            HasCourseAccess = c.Instructors.Any(i => i.Id == userId) || c.Enrollments.Any(e => e.StudentId == userId),
                            IsAdmin = dbContext.Users.Any(u => u.Id == userId && u.Role == UserRole.Admin)
                        })
                        .FirstOrDefaultAsync(ct)
                    ?? throw ServiceException.NotFound(Resources.ErrorCourseNotFound);

        if (!check.HasCourseAccess && !check.IsAdmin)
            throw ServiceException.Forbidden(Resources.ErrorCourseAccessDenied);
    }
}