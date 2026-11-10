using Backend.Extensions;
using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[Route("api/enrollments")]
[Tags("Enrollments")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
public class EnrollmentController : ApiControllerBase {
    [HttpGet("{courseId:uint}")]
    //[Authorize(Roles = "Admin,Instructor")]
    [RequiresPaging]
    [EndpointSummary("Get list of enrolled users in a course")]
    [EndpointDescription("Returns a paginated list of all students enrolled in a specific course.")]
    [ProducesResponseType<Wrapper<UserResponseDto[]>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponseDto[]>> GetEnrolledUsers([FromRoute] uint courseId, [FromQuery] RequestQuery requestQuery, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        var students = new[] {
            new UserResponseDto(1, "dummy_student", "student@example.com", "Student"),
            new UserResponseDto(2, "another_student", "another@example.com", "Student")
        };
        return OkPaged(students, requestQuery);
    }

    [HttpPost]
    //[Authorize(Roles = "Student")]
    [EndpointSummary("Enroll currently authenticated student into a course")]
    [EndpointDescription("Enrolls the currently authenticated student into a course, requiring an access key if the course is private.")]
    [ProducesResponseType<Wrapper<uint>>(StatusCodes.Status201Created)]
    public async Task<ActionResult> EnrollIntoCourse([FromBody] EnrollmentRequestDto enrollmentRequest, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        const uint newEnrollmentId = 1;
        return CreatedAtAction(nameof(GetEnrolledUsers), new { courseId = enrollmentRequest.CourseId, page = 1, pageSize = 10 }, newEnrollmentId);
    }

    [HttpPost("forceenroll")]
    //[Authorize(Roles = "Admin, Instructor")]
    [EndpointSummary("Force enroll student into a course")]
    [ProducesResponseType<Wrapper<uint>>(StatusCodes.Status201Created)]
    public async Task<ActionResult> ForceEnrollIntoCourse([FromBody] ForceEnrollmentRequestDto forceEnrollmentRequest, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        const uint newEnrollmentId = 1;
        return CreatedAtAction(nameof(GetEnrolledUsers), new { courseId = forceEnrollmentRequest.CourseId, page = 1, pageSize = 10 }, newEnrollmentId);
    }

    [HttpDelete("{courseId:uint}")]
    //[Authorize(Roles = "Student")]
    [EndpointSummary("Unenroll currently authenticated student from a course")]
    [EndpointDescription("Removes the currently authenticated student from the specified course.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Unenroll([FromRoute] uint courseId, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return NoContent();
    }

    [HttpDelete("{courseId:uint}/{studentId:uint}")]
    //[Authorize(Roles = "Admin, Instructor")]
    [EndpointSummary("Remove student from course")]
    [EndpointDescription("Allows an instructor or admin to forcibly remove a specific student from a course.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> ForceUnenroll([FromRoute] uint courseId, [FromRoute] uint studentId, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return NoContent();
    }
}