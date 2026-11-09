using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
[Route("api/users")]
[Tags("Users")]
public class UserController : ApiControllerBase {
    [HttpGet]
    //[Authorize(Roles = "Admin")]
    [EndpointSummary("Get all users")]
    [EndpointDescription("Returns a paginated list of users registered in the system.")]
    [ProducesResponseType<Wrapper<UserResponseDto[]>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserResponseDto[]>> GetUsers([FromQuery] RequestQuery requestQuery, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Ok(Array.Empty<UserResponseDto>());
    }

    [HttpGet("{userId:uint}")]
    //[Authorize(Roles = "Admin")]
    [EndpointSummary("Get user detail")]
    [ProducesResponseType<Wrapper<UserResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponseDto>> GetUserById(uint userId, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Ok(new UserResponseDto(userId, "dummy_user", "dummy@email.com", "Student"));
    }

    [HttpPost]
    //[Authorize(Roles = "Admin")]
    [EndpointSummary("Create a user")]
    [ProducesResponseType<Wrapper<uint>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<uint>> CreateUser([FromBody] CreateUserRequestDto requestDto, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        uint newUserId = 1;
        return CreatedAtAction(nameof(GetUserById), new { userId = newUserId }, newUserId);
    }

    [HttpPut("{userId:uint}")]
    //[Authorize(Roles = "Admin")]
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
    //[Authorize(Roles = "Admin")]
    [EndpointSummary("Delete a user")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(uint userId, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return NoContent();
    }
}