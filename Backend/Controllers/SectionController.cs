using Backend.Extensions;
using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
[Route("api/sections")]
[Tags("Sections")]
public class SectionController(ISectionService sectionService) : ApiControllerBase {
    [HttpGet("{sectionId:uint}")]
    [Authorize(Roles = "Instructor,Admin")]
    [EndpointSummary("Get section detail")]
    [EndpointDescription("Returns the details of a section.")]
    [ProducesResponseType<Wrapper<SectionResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SectionResponseDto>> GetSectionById([FromRoute] uint sectionId, CancellationToken cancellationToken) => Ok(await sectionService.GetByIdAsync(AuthenticatedUserId, sectionId, cancellationToken));

    [HttpGet("{sectionId:uint}/lessons")]
    [Authorize]
    [RequiresPaging]
    [EndpointSummary("Get section lessons")]
    [ProducesResponseType<Wrapper<LessonResponseWrapperDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LessonResponseWrapperDto>> GetSectionLessons([FromRoute] uint sectionId, [FromQuery] RequestQuery requestQuery, CancellationToken cancellationToken)
    {
        var (lessons, metadata) = await sectionService.GetLessonsAsync(sectionId, AuthenticatedUserId, requestQuery, cancellationToken);
        return Ok(lessons, metadata);
    }

    [HttpGet("{sectionId:uint}/quizzes")]
    [Authorize]
    [RequiresPaging]
    [EndpointSummary("Get section quizzes")]
    [ProducesResponseType<Wrapper<QuizResponseDto[]>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuizResponseDto[]>> GetSectionQuizzes([FromRoute] uint sectionId, [FromQuery] RequestQuery requestQuery, CancellationToken cancellationToken)
    {
        var (quizzes, metadata) = await sectionService.GetQuizzesAsync(sectionId, AuthenticatedUserId, requestQuery, cancellationToken);
        return Ok(quizzes, metadata);
    }

    [HttpPost]
    [Authorize(Roles = "Instructor,Admin")]
    [EndpointSummary("Create a section")]
    [EndpointDescription("Creates a new section for a specific course.")]
    [ProducesResponseType<Wrapper<uint>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<uint>> CreateSection([FromBody] CreateSectionRequestDto requestDto, CancellationToken cancellationToken)
    {
        uint newSectionId = await sectionService.CreateAsync(AuthenticatedUserId, requestDto, cancellationToken);
        return CreatedAtAction(nameof(GetSectionById), new { sectionId = newSectionId }, newSectionId);
    }

    [HttpPut("{sectionId:uint}")]
    [Authorize(Roles = "Instructor,Admin")]
    [EndpointSummary("Update a section")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSection(uint sectionId, [FromBody] CreateSectionRequestDto requestDto, CancellationToken cancellationToken)
    {
        await sectionService.UpdateAsync(AuthenticatedUserId, sectionId, requestDto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{sectionId:uint}")]
    [Authorize(Roles = "Instructor,Admin")]
    [EndpointSummary("Delete a section")]
    [EndpointDescription("Removes a section and cascades deletion to its lessons and quizzes.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSection(uint sectionId, CancellationToken cancellationToken)
    {
        await sectionService.DeleteAsync(AuthenticatedUserId, sectionId, cancellationToken);
        return NoContent();
    }
}