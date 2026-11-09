using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
[Route("api/sections")]
[Tags("Sections")]
public class SectionController : ApiControllerBase {
    [HttpGet("{sectionId:uint}")]
    //[Authorize(Roles = "Instructor,Admin")]
    [EndpointSummary("Get section detail")]
    [EndpointDescription("Returns the details of a section.")]
    [ProducesResponseType<Wrapper<SectionResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SectionResponseDto>> GetSectionById([FromRoute] uint sectionId, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Ok(new SectionResponseDto(sectionId, "Dummy Section", 1, 1));
    }

    [HttpPost]
    //[Authorize(Roles = "Instructor,Admin")]
    [EndpointSummary("Create a section")]
    [EndpointDescription("Creates a new section for a specific course.")]
    [ProducesResponseType<Wrapper<uint>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<uint>> CreateSection([FromBody] CreateSectionRequestDto requestDto, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        const uint newSectionId = 1;
        return CreatedAtAction(nameof(GetSectionById), new { sectionId = newSectionId }, newSectionId);
    }

    [HttpPut("{sectionId:uint}")]
    //[Authorize(Roles = "Instructor,Admin")]
    [EndpointSummary("Update a section")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSection(uint sectionId, [FromBody] CreateSectionRequestDto requestDto, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return NoContent();
    }

    [HttpDelete("{sectionId:uint}")]
    //[Authorize(Roles = "Instructor,Admin")]
    [EndpointSummary("Delete a section")]
    [EndpointDescription("Removes a section and cascades deletion to its lessons and quizzes.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSection(uint sectionId, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return NoContent();
    }
}