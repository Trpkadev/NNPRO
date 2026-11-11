using Backend.Extensions;
using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Backend.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
[Route("api/questions")]
[Tags("Questions")]
public class QuestionController : ApiControllerBase {
    [HttpGet("{questionId:uint}")]
    [Authorize]
    [EndpointSummary("Get question detail")]
    [ProducesResponseType<Wrapper<QuestionResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuestionResponseDto>> GetQuestion(uint questionId, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Ok(new QuestionResponseDto(questionId, "Dummy question?", null, QuestionType.SingleChoice, 1, 1));
    }

    [HttpGet("{questionId:uint}/options")]
    [Authorize]
    [RequiresPaging]
    [EndpointSummary("Get answer options")]
    [EndpointDescription("Returns question options without correctness information.")]
    [ProducesResponseType<Wrapper<AnswerOptionResponseDto[]>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AnswerOptionResponseDto[]>> GetAnswerOptions(uint questionId, [FromQuery] RequestQuery requestQuery, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        var options = new[] {
            new AnswerOptionResponseDto(1, "Option A", null, questionId),
            new AnswerOptionResponseDto(2, "Option B", null, questionId)
        };
        return OkPaged(options, requestQuery);
    }

    [HttpGet("{questionId:uint}/options/editor")]
    [Authorize(Roles = "Admin,Instructor")]
    [RequiresPaging]
    [EndpointSummary("Get answer options for editing")]
    [ProducesResponseType<Wrapper<AnswerOptionEditorResponseDto[]>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AnswerOptionEditorResponseDto[]>> GetAnswerOptionsForEditor(uint questionId, [FromQuery] RequestQuery requestQuery, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        var options = new[] {
            new AnswerOptionEditorResponseDto(1, "Option A", null, true, questionId),
            new AnswerOptionEditorResponseDto(2, "Option B", null, false, questionId)
        };
        return OkPaged(options, requestQuery);
    }

    [HttpPost("{questionId:uint}/options")]
    [Authorize(Roles = "Admin,Instructor")]
    [EndpointSummary("Create an answer option")]
    [ProducesResponseType<Wrapper<uint>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<uint>> CreateAnswerOption(uint questionId, [FromBody] CreateAnswerOptionRequestDto requestDto, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        const uint answerOptionId = 1;
        return CreatedAtAction(nameof(GetAnswerOptionsForEditor), new { questionId, page = 1, pageSize = 10 }, answerOptionId);
    }

    [HttpPut("{questionId:uint}/options/{optionId:uint}")]
    [Authorize(Roles = "Admin,Instructor")]
    [EndpointSummary("Update an answer option")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAnswerOption(uint questionId, uint optionId, [FromBody] UpdateAnswerOptionRequestDto requestDto, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return NoContent();
    }

    [HttpDelete("{questionId:uint}/options/{optionId:uint}")]
    [Authorize(Roles = "Admin,Instructor")]
    [EndpointSummary("Delete an answer option")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAnswerOption(uint questionId, uint optionId, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return NoContent();
    }

    [HttpPost]
    [Authorize(Roles = "Instructor,Admin")]
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
    [Authorize(Roles = "Instructor,Admin")]
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
    [Authorize(Roles = "Instructor,Admin")]
    [EndpointSummary("Delete a question")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteQuestion(uint questionId, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return NoContent();
    }
}