using Backend.Extensions;
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

    [HttpGet("{sectionId:uint}/lessons")]
    //[Authorize]
    [RequiresPaging]
    [EndpointSummary("Get section lessons")]
    [ProducesResponseType<Wrapper<LessonResponseDto[]>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<LessonResponseDto[]>> GetSectionLessons([FromRoute] uint sectionId, [FromQuery] RequestQuery requestQuery, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        var lessons = new[] {
            new LessonResponseDto(1, "Introduction", "Dummy lesson content.", null, 1, sectionId),
            new LessonResponseDto(2, "Advanced topic", "More dummy lesson content.", null, 2, sectionId)
        };
        return OkPaged(lessons, requestQuery);
    }

    [HttpGet("{sectionId:uint}/quizzes")]
    //[Authorize]
    [RequiresPaging]
    [EndpointSummary("Get section quizzes")]
    [ProducesResponseType<Wrapper<QuizResponseDto[]>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<QuizResponseDto[]>> GetSectionQuizzes([FromRoute] uint sectionId, [FromQuery] RequestQuery requestQuery, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        var quizzes = new[] {
            new QuizResponseDto(1, "Knowledge check", null, null, 75.0m, sectionId),
            new QuizResponseDto(2, "Final quiz", 2, 1800, 80.0m, sectionId)
        };
        return OkPaged(quizzes, requestQuery);
    }

    [HttpGet("{sectionId:uint}/lessons")]
    //[Authorize]
    [EndpointSummary("Get section lessons")]
    [EndpointDescription("Returns the lessons belonging to a section.")]
    [ProducesResponseType<Wrapper<LessonResponseDto[]>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LessonResponseDto[]>> GetSectionLessons([FromRoute] uint sectionId, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Ok(new[] { new LessonResponseDto(1, "Dummy Lesson", "Dummy lesson content.", null, 1, sectionId) });
    }

    [HttpGet("{sectionId:uint}/quizzes")]
    //[Authorize]
    [EndpointSummary("Get section quizzes")]
    [EndpointDescription("Returns the quizzes belonging to a section.")]
    [ProducesResponseType<Wrapper<QuizResponseDto[]>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuizResponseDto[]>> GetSectionQuizzes([FromRoute] uint sectionId, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Ok(new[] { new QuizResponseDto(1, "Dummy Quiz", null, null, 75.0m, sectionId) });
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