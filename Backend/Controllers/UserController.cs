using Backend.Extensions;
using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
[Route("api/users")]
[Tags("Users")]
public class UserController : ApiControllerBase {
    [HttpGet("me/courses")]
    [Authorize]
    [RequiresPaging]
    [EndpointSummary("Get my courses")]
    [ProducesResponseType<Wrapper<UserCourseResponseDto[]>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserCourseResponseDto[]>> GetMyCourses([FromQuery] RequestQuery requestQuery, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        var courses = new[] {
            new UserCourseResponseDto(1, "Dummy Course", "A short course description."),
            new UserCourseResponseDto(2, "Another Course", "Another short description.")
        };
        return OkPaged(courses, requestQuery);
    }

    [HttpGet("me/progress")]
    [Authorize]
    [RequiresPaging]
    [EndpointSummary("Get my learning progress")]
    [ProducesResponseType<Wrapper<UserLessonProgressResponseDto[]>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserLessonProgressResponseDto[]>> GetMyProgress([FromQuery] RequestQuery requestQuery, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        var progress = new[] {
            new UserLessonProgressResponseDto(1, "Introduction", 1, 1, true, DateTime.UtcNow.AddDays(-1)),
            new UserLessonProgressResponseDto(2, "Advanced topic", 1, 1, false, null)
        };
        return OkPaged(progress, requestQuery);
    }

    [HttpPut("{userId:uint}")]
    [Authorize]
    [EndpointSummary("Update a user")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateUser(uint userId, [FromBody] CreateUserRequestDto requestDto, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return NoContent();
    }

    [HttpDelete("{userId:uint}")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Delete a user")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(uint userId, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return NoContent();
    }
}