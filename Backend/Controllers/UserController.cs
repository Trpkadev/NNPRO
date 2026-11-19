using Backend.Extensions;
using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
[Route("api/users")]
[Tags("Users")]
public class UserController(IUserService userService) : ApiControllerBase {
    [HttpGet("me/courses")]
    [Authorize]
    [RequiresPaging]
    [EndpointSummary("Get my courses")]
    [ProducesResponseType<Wrapper<UserCourseResponseDto[]>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserCourseResponseDto[]>> GetMyCourses([FromQuery] RequestQuery requestQuery, CancellationToken cancellationToken)
    {
        var (courses, metadata) = await userService.GetMyCoursesAsync(AuthenticatedUserId, requestQuery, cancellationToken);
        return Ok(courses, metadata);
    }

    [HttpGet("me/progress")]
    [Authorize]
    [RequiresPaging]
    [EndpointSummary("Get my learning progress")]
    [ProducesResponseType<Wrapper<UserLessonProgressResponseDto[]>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserLessonProgressResponseDto[]>> GetMyProgress([FromQuery] RequestQuery requestQuery, CancellationToken cancellationToken)
    {
        var (progress, metadata) = await userService.GetMyProgressAsync(AuthenticatedUserId, requestQuery, cancellationToken);
        return Ok(progress, metadata);
    }

    [HttpPut]
    [Authorize]
    [EndpointSummary("Update a currently logged in user")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequestDto requestDto, CancellationToken cancellationToken)
    {
        await userService.UpdateAsync(AuthenticatedUserId, requestDto, cancellationToken);
        return NoContent();
    }

    [HttpPut("{userId:uint}")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Update a user")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateUser(uint userId, [FromBody] UpdateUserRequestDto requestDto, CancellationToken cancellationToken)
    {
        await userService.UpdateAsync(userId, requestDto, cancellationToken);
        return NoContent();
    }

    [HttpDelete]
    [Authorize]
    [EndpointSummary("Delete a currently logged in user")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteUser(CancellationToken cancellationToken)
    {
        await userService.DeleteAsync(AuthenticatedUserId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{userId:uint}")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Delete a user")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(uint userId, CancellationToken cancellationToken)
    {
        await userService.DeleteAsync(userId, cancellationToken);
        return NoContent();
    }
}