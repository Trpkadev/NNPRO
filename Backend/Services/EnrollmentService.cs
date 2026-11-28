using Backend.Exceptions;
using Backend.Extensions;
using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Backend.Models.Entities;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class EnrollmentService(ServiceContext<EnrollmentService> ctx) : IEnrollmentService {
    public async Task<(UserResponseDto[] Users, Metadata Metadata)> GetEnrolledUsersAsync(uint id, RequestQuery request, CancellationToken ct)
    {
        await CourseAuthorization.EnsureCanManageCourseAsync(ctx.Db, id, ctx.AuthUserId, ct);

        var (users, metadata) = await ctx.Db.Enrollments.AsNoTracking()
            .Where(enrollment => enrollment.CourseId == id)
            .OrderBy(enrollment => enrollment.StudentId)
            .Select(enrollment => new UserResponseDto(
                enrollment.Student.Id,
                enrollment.Student.Username,
                enrollment.Student.Email,
                enrollment.Student.Role.ToString()))
            .ToPagedResultAsync(request, ctx.MaxUnpagedItems, ct);

        return (users, new Metadata(metadata, request));
    }

    public async Task<uint> EnrollIntoCourseAsync(EnrollmentRequestDto request, CancellationToken ct)
    {
        var course = await ctx.Db.Courses.FirstOrDefaultAsync(candidate => candidate.Id == request.CourseId, ct);
        if (course is null)
            throw ServiceException.NotFound(Resources.ErrorCourseNotFound);

        if (course.AccessKey is not null && !string.Equals(course.AccessKey, request.AccessKey, StringComparison.Ordinal))
            throw ServiceException.Forbidden(Resources.ErrorCourseAccessKeyInvalid);

        await EnsureStudentAsync(ct);
        if (await ctx.Db.Enrollments.AnyAsync(enrollment => enrollment.StudentId == ctx.AuthUserId && enrollment.CourseId == course.Id, ct))
            throw ServiceException.Conflict(Resources.ErrorAlreadyEnrolled);

        Enrollment enrollment = new() { StudentId = ctx.AuthUserId, CourseId = course.Id };
        ctx.Db.Enrollments.Add(enrollment);
        await ctx.Db.SaveChangesAsync(ct);
        return enrollment.Id;
    }

    public async Task<uint> ForceEnrollIntoCourseAsync(ForceEnrollmentRequestDto request, CancellationToken ct)
    {
        await CourseAuthorization.EnsureCanManageCourseAsync(ctx.Db, request.CourseId, ctx.AuthUserId, ct);
        await EnsureStudentAsync(ct);
        if (await ctx.Db.Enrollments.AsNoTracking().AnyAsync(enrollment => enrollment.StudentId == request.StudentId && enrollment.CourseId == request.CourseId, ct))
            throw ServiceException.Conflict(Resources.ErrorAlreadyEnrolledForce);

        Enrollment enrollment = new() { StudentId = request.StudentId, CourseId = request.CourseId };
        ctx.Db.Enrollments.Add(enrollment);
        await ctx.Db.SaveChangesAsync(ct);
        return enrollment.Id;
    }

    public async Task UnenrollAsync(uint courseId, CancellationToken ct)
    {
        int deleted = await ctx.Db.Enrollments.Where(enrollment => enrollment.StudentId == ctx.AuthUserId && enrollment.CourseId == courseId)
            .ExecuteDeleteAsync(ct);
        if (deleted == 0)
            throw ServiceException.NotFound(Resources.ErrorEnrollmentNotFound);
    }

    public async Task ForceUnenrollAsync(ForceEnrollmentRequestDto request, CancellationToken ct)
    {
        int deleted = await ctx.Db.Enrollments.Where(enrollment => enrollment.StudentId == request.StudentId && enrollment.CourseId == request.StudentId)
            .ExecuteDeleteAsync(ct);
        if (deleted == 0)
            throw ServiceException.NotFound(Resources.ErrorEnrollmentNotFound);
    }

    private async Task EnsureStudentAsync(CancellationToken ct)
    {
        var role = await ctx.Db.Users.AsNoTracking()
            .Where(user => user.Id == ctx.AuthUserId)
            .Select(user => (UserRole?)user.Role)
            .FirstOrDefaultAsync(ct);
        if (!role.HasValue)
            throw ServiceException.NotFound(Resources.ErrorUserNotFound);
        if (role != UserRole.Student)
            throw ServiceException.BadRequest(Resources.ErrorOnlyStudentsCanEnroll);
    }
}