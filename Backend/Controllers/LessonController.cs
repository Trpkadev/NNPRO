using Backend.Extensions;
using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
[Route("api/lessons")]
[Tags("Lessons")]
public class LessonController(ILessonService lessonService) : ApiControllerBase {
    [HttpGet("{id:uint}")]
    [Authorize]
    [EndpointSummary("Get lesson detail")]
    [EndpointDescription("Returns complete information about a specific lesson, including text content, video URLs, and attachments.")]
    [ProducesResponseType<Wrapper<LessonResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LessonResponseDto>> GetLessonById(uint id, CancellationToken cancellationToken) => Ok(await lessonService.GetByIdAsync(id, AuthenticatedUserId, cancellationToken));

    [HttpGet("{lessonId:uint}/attachments")]
    [Authorize]
    [RequiresPaging]
    [EndpointSummary("Get lesson attachments")]
    [ProducesResponseType<Wrapper<AttachmentResponseDto[]>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AttachmentResponseDto[]>> GetAttachments([FromRoute] uint lessonId, [FromQuery] RequestQuery requestQuery, CancellationToken cancellationToken)
    {
        var (attachments, metadata) = await lessonService.GetAttachmentsAsync(lessonId, AuthenticatedUserId, requestQuery, cancellationToken);
        return Ok(attachments, metadata);
    }

    [HttpPost]
    [Authorize(Roles = "Instructor,Admin")]
    [EndpointSummary("Create a lesson")]
    [ProducesResponseType<Wrapper<uint>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<uint>> CreateLesson([FromBody] CreateLessonRequestDto requestDto, CancellationToken cancellationToken)
    {
        uint newLessonId = await lessonService.CreateAsync(AuthenticatedUserId, requestDto, cancellationToken);
        return CreatedAtAction(nameof(GetLessonById), new { id = newLessonId }, newLessonId);
    }

    [HttpPut("{lessonId:uint}")]
    [Authorize(Roles = "Instructor,Admin")]
    [EndpointSummary("Update a lesson")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateLesson(uint lessonId, [FromBody] CreateLessonRequestDto requestDto, CancellationToken cancellationToken)
    {
        await lessonService.UpdateAsync(AuthenticatedUserId, lessonId, requestDto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{lessonId:uint}")]
    [Authorize(Roles = "Instructor,Admin")]
    [EndpointSummary("Delete a lesson")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLesson(uint lessonId, CancellationToken cancellationToken)
    {
        await lessonService.DeleteAsync(AuthenticatedUserId, lessonId, cancellationToken);
        return NoContent();
    }

    [HttpPut("{lessonId:uint}/progress")]
    [Authorize(Roles = "Student")]
    [EndpointSummary("Update lesson progress")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateProgress(uint lessonId, [FromBody] UpdateProgressDto dto, CancellationToken cancellationToken)
    {
        await lessonService.UpdateProgressAsync(AuthenticatedUserId, lessonId, dto, cancellationToken);
        return NoContent();
    }

    [HttpPost("attachments")]
    [Authorize(Roles = "Instructor,Admin")]
    [EndpointSummary("Add attachment to a lesson")]
    [ProducesResponseType<Wrapper<uint>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<uint>> AddAttachment([FromBody] AttachmentRequestDto dto, CancellationToken cancellationToken)
    {
        uint attachmentId = await lessonService.AddAttachmentAsync(AuthenticatedUserId, dto, cancellationToken);
        return CreatedAtAction(nameof(GetLessonById), new { id = dto.LessonId }, attachmentId);
    }

    [HttpDelete("{lessonId:uint}/attachments/{attachmentId:uint}")]
    [Authorize(Roles = "Instructor,Admin")]
    [EndpointSummary("Delete lesson attachment")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAttachment(uint lessonId, uint attachmentId, CancellationToken cancellationToken)
    {
        await lessonService.DeleteAttachmentAsync(AuthenticatedUserId, lessonId, attachmentId, cancellationToken);
        return NoContent();
    }
}