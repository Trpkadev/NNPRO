using Backend.Data;
using Backend.Exceptions;
using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Backend.Models.Entities;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class EnrollmentService(AppDbContext dbContext) : IEnrollmentService {
    public async Task<UserResponseDto[]> GetEnrolledUsersAsync(uint courseId, uint requestingUserId, CancellationToken ct)
    {
        await CourseAuthorization.EnsureCanManageCourseAsync(dbContext, requestingUserId, courseId, ct);
        return await dbContext.Enrollments.AsNoTracking()
            .Where(enrollment => enrollment.CourseId == courseId)
            .OrderBy(enrollment => enrollment.StudentId)
            .Select(enrollment => new UserResponseDto(
                enrollment.Student.Id,
                enrollment.Student.Username,
                enrollment.Student.Email,
                enrollment.Student.Role.ToString()))
            .ToArrayAsync(ct);
    }

    public async Task<uint> EnrollIntoCourseAsync(uint studentId, EnrollmentRequestDto request, CancellationToken ct)
    {
        var course = await dbContext.Courses.FirstOrDefaultAsync(candidate => candidate.Id == request.CourseId, ct);
        if (course is null)
            throw ServiceException.NotFound(Resources.ErrorCourseNotFound);

        if (course.AccessKey is not null && !string.Equals(course.AccessKey, request.AccessKey, StringComparison.Ordinal))
            throw ServiceException.Forbidden(Resources.ErrorCourseAccessKeyInvalid);

        await EnsureStudentAsync(studentId, ct);
        if (await dbContext.Enrollments.AnyAsync(enrollment => enrollment.StudentId == studentId && enrollment.CourseId == course.Id, ct))
            throw ServiceException.Conflict(Resources.ErrorAlreadyEnrolled);

        Enrollment enrollment = new() { StudentId = studentId, CourseId = course.Id };
        dbContext.Enrollments.Add(enrollment);
        await dbContext.SaveChangesAsync(ct);
        return enrollment.Id;
    }

    public async Task<uint> ForceEnrollIntoCourseAsync(uint requestingUserId, ForceEnrollmentRequestDto request, CancellationToken ct)
    {
        await CourseAuthorization.EnsureCanManageCourseAsync(dbContext, requestingUserId, request.CourseId, ct);
        await EnsureStudentAsync(request.StudentId, ct);
        if (await dbContext.Enrollments.AsNoTracking().AnyAsync(enrollment => enrollment.StudentId == request.StudentId && enrollment.CourseId == request.CourseId, ct))
            throw ServiceException.Conflict(Resources.ErrorAlreadyEnrolled);

        Enrollment enrollment = new() { StudentId = request.StudentId, CourseId = request.CourseId };
        dbContext.Enrollments.Add(enrollment);
        await dbContext.SaveChangesAsync(ct);
        return enrollment.Id;
    }

    public async Task UnenrollAsync(uint studentId, uint courseId, CancellationToken ct)
    {
        int deleted = await dbContext.Enrollments.Where(enrollment => enrollment.StudentId == studentId && enrollment.CourseId == courseId)
            .ExecuteDeleteAsync(ct);
        if (deleted == 0)
            throw ServiceException.NotFound(Resources.ErrorEnrollmentNotFound);
    }

    public async Task ForceUnenrollAsync(uint requestingUserId, uint courseId, uint studentId, CancellationToken ct)
    {
        await CourseAuthorization.EnsureCanManageCourseAsync(dbContext, requestingUserId, courseId, ct);
        int deleted = await dbContext.Enrollments.Where(enrollment => enrollment.StudentId == studentId && enrollment.CourseId == courseId)
            .ExecuteDeleteAsync(ct);
        if (deleted == 0)
            throw ServiceException.NotFound(Resources.ErrorEnrollmentNotFound);
    }

    private async Task EnsureStudentAsync(uint userId, CancellationToken ct)
    {
        var role = await dbContext.Users.AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => (UserRole?)user.Role)
            .FirstOrDefaultAsync(ct);
        if (!role.HasValue)
            throw ServiceException.NotFound(Resources.ErrorUserNotFound);
        if (role != UserRole.Student)
            throw ServiceException.BadRequest(Resources.ErrorOnlyStudentsCanEnroll);
    }
}