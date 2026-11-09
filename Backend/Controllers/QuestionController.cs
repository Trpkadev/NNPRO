using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Backend.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
[Route("api/questions")]
[Tags("Questions")]
public class QuestionController : ApiControllerBase {
    [HttpGet("{questionId:uint}")]
    //[Authorize]
    [EndpointSummary("Get question detail")]
    [ProducesResponseType<Wrapper<QuestionResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuestionResponseDto>> GetQuestion(uint questionId, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Ok(new QuestionResponseDto(questionId, "Dummy question?", null, QuestionType.SingleChoice, 1, 1));
    }

    [HttpPost]
    //[Authorize(Roles = "Instructor,Admin")]
    [EndpointSummary("Create a question")]
    [ProducesResponseType<Wrapper<uint>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<uint>> CreateQuestion([FromBody] CreateQuestionRequestDto requestDto, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        const uint newQuestionId = 1;
        return CreatedAtAction(nameof(GetQuestion), new { questionId = newQuestionId }, newQuestionId);
    }

    [HttpPut("{questionId:uint}")]
    //[Authorize(Roles = "Instructor,Admin")]
    [EndpointSummary("Update a question")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateQuestion(uint questionId, [FromBody] CreateQuestionRequestDto requestDto, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return NoContent();
    }

    [HttpDelete("{questionId:uint}")]
    //[Authorize(Roles = "Instructor,Admin")]
    [EndpointSummary("Delete a question")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteQuestion(uint questionId, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return NoContent();
    }
}