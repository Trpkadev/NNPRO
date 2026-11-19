using Backend.Extensions;
using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[Route("api/enrollments")]
[Tags("Enrollments")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
public class EnrollmentController(IEnrollmentService enrollmentService) : ApiControllerBase {
    [HttpGet("{courseId:uint}")]
    [Authorize(Roles = "Admin,Instructor")]
    [RequiresPaging]
    [EndpointSummary("Get list of enrolled users in a course")]
    [EndpointDescription("Returns a paginated list of all students enrolled in a specific course.")]
    [ProducesResponseType<Wrapper<UserResponseDto[]>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponseDto[]>> GetEnrolledUsers([FromRoute] uint courseId, [FromQuery] RequestQuery requestQuery, CancellationToken cancellationToken)
    {
        var students = await enrollmentService.GetEnrolledUsersAsync(courseId, AuthenticatedUserId, cancellationToken);
        return OkPaged(students, requestQuery);
    }

    [HttpPost]
    [Authorize(Roles = "Student")]
    [EndpointSummary("Enroll currently authenticated student into a course")]
    [EndpointDescription("Enrolls the currently authenticated student into a course, requiring an access key if the course is private.")]
    [ProducesResponseType<Wrapper<uint>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> EnrollIntoCourse([FromBody] EnrollmentRequestDto enrollmentRequest, CancellationToken cancellationToken)
    {
        uint newEnrollmentId = await enrollmentService.EnrollIntoCourseAsync(AuthenticatedUserId, enrollmentRequest, cancellationToken);
        return CreatedAtAction(nameof(GetEnrolledUsers), new { courseId = enrollmentRequest.CourseId, page = 1, pageSize = 10 }, newEnrollmentId);
    }

    [HttpPost("forceenroll")]
    [Authorize(Roles = "Admin, Instructor")]
    [EndpointSummary("Force enroll student into a course")]
    [ProducesResponseType<Wrapper<uint>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> ForceEnrollIntoCourse([FromBody] ForceEnrollmentRequestDto forceEnrollmentRequest, CancellationToken cancellationToken)
    {
        uint newEnrollmentId = await enrollmentService.ForceEnrollIntoCourseAsync(AuthenticatedUserId, forceEnrollmentRequest, cancellationToken);
        return CreatedAtAction(nameof(GetEnrolledUsers), new { courseId = forceEnrollmentRequest.CourseId, page = 1, pageSize = 10 }, newEnrollmentId);
    }

    [HttpDelete("{courseId:uint}")]
    [Authorize(Roles = "Student")]
    [EndpointSummary("Unenroll currently authenticated student from a course")]
    [EndpointDescription("Removes the currently authenticated student from the specified course.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Unenroll([FromRoute] uint courseId, CancellationToken cancellationToken)
    {
        await enrollmentService.UnenrollAsync(AuthenticatedUserId, courseId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{courseId:uint}/{studentId:uint}")]
    [Authorize(Roles = "Admin, Instructor")]
    [EndpointSummary("Remove student from course")]
    [EndpointDescription("Allows an instructor or admin to forcibly remove a specific student from a course.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ForceUnenroll([FromRoute] uint courseId, [FromRoute] uint studentId, CancellationToken cancellationToken)
    {
        await enrollmentService.ForceUnenrollAsync(AuthenticatedUserId, courseId, studentId, cancellationToken);
        return NoContent();
    }
}