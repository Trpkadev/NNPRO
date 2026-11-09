using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
[Route("api/lessons")]
[Tags("Lessons")]
public class LessonController : ApiControllerBase {
    [HttpGet("{id:uint}")]
    //[Authorize]
    [EndpointSummary("Get lesson detail")]
    [EndpointDescription("Returns complete information about a specific lesson, including text content, video URLs, and attachments.")]
    [ProducesResponseType<Wrapper<LessonResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LessonResponseDto>> GetLessonById(uint id, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Ok(new LessonResponseDto(id, "Dummy Lesson", "Content", null, 1, 1));
    }

    [HttpPost]
    //[Authorize(Roles = "Instructor,Admin")]
    [EndpointSummary("Create a lesson")]
    [ProducesResponseType<Wrapper<uint>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<uint>> CreateLesson([FromBody] CreateLessonRequestDto requestDto, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        uint newLessonId = 1;
        return CreatedAtAction(nameof(GetLessonById), new { id = newLessonId }, newLessonId);
    }

    [HttpPut("{lessonId:uint}")]
    //[Authorize(Roles = "Instructor,Admin")]
    [EndpointSummary("Update a lesson")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateLesson(uint lessonId, [FromBody] CreateLessonRequestDto requestDto, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return NoContent();
    }

    [HttpDelete("{lessonId:uint}")]
    //[Authorize(Roles = "Instructor,Admin")]
    [EndpointSummary("Delete a lesson")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLesson(uint lessonId, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return NoContent();
    }

    [HttpPut("{lessonId:uint}/progress")]
    //[Authorize(Roles = "Student")]
    [EndpointSummary("Update lesson progress")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProgress(uint lessonId, [FromBody] UpdateProgressDto dto, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return NoContent();
    }

    [HttpPost("attachments")]
    //[Authorize(Roles = "Instructor,Admin")]
    [EndpointSummary("Add attachment to a lesson")]
    [ProducesResponseType<Wrapper<uint>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<uint>> AddAttachment([FromBody] AttachmentRequestDto dto, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return CreatedAtAction(nameof(GetLessonById), new { id = dto.LessonId }, 1);
    }

    [HttpDelete("{lessonId:uint}/attachments/{attachmentId:uint}")]
    //[Authorize(Roles = "Instructor,Admin")]
    [EndpointSummary("Delete lesson attachment")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAttachment(uint lessonId, uint attachmentId, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return NoContent();
    }
}