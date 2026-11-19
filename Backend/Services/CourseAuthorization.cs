using Backend.Data;
using Backend.Exceptions;
using Backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

internal static class CourseAuthorization {
    public async static Task EnsureCanManageCourseAsync(AppDbContext dbContext, uint userId, uint courseId, CancellationToken ct)
    {
        var role = await dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => (UserRole?)user.Role)
            .FirstOrDefaultAsync(ct);
        if (!role.HasValue)
            throw ServiceException.NotFound(Resources.ErrorUserNotFound);

        bool courseExists = await dbContext.Courses.AnyAsync(course => course.Id == courseId, ct);
        if (!courseExists)
            throw ServiceException.NotFound(Resources.ErrorCourseNotFound);

        if (role == UserRole.Admin)
            return;

        if (role != UserRole.Instructor || !await dbContext.Courses.AnyAsync(course => course.Id == courseId && course.Instructors.Any(instructor => instructor.Id == userId), ct))
            throw ServiceException.Forbidden(Resources.ErrorCourseManagementForbidden);
    }
}