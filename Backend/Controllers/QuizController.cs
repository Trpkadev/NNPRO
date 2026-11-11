using Backend.Extensions;
using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Backend.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
[Route("api/quizzes")]
[Tags("Quizzes")]
public class QuizController : ApiControllerBase {
    [HttpGet("{quizId:uint}")]
    [Authorize]
    [EndpointSummary("Get quiz detail")]
    [ProducesResponseType<Wrapper<QuizResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuizResponseDto>> GetQuiz(uint quizId, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Ok(new QuizResponseDto(quizId, "Dummy Quiz", null, null, 75.0m, 1));
    }

    [HttpGet("{quizId:uint}/questions")]
    [Authorize]
    [RequiresPaging]
    [EndpointSummary("Get quiz questions")]
    [EndpointDescription("Returns the questions belonging to a quiz without answer keys.")]
    [ProducesResponseType<Wrapper<QuestionResponseDto[]>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuestionResponseDto[]>> GetQuestions(uint quizId, [FromQuery] RequestQuery requestQuery, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        var questions = new[] {
            new QuestionResponseDto(1, "Dummy question?", null, QuestionType.SingleChoice, 1, quizId),
            new QuestionResponseDto(2, "Choose all that apply.", null, QuestionType.MultipleChoice, 2, quizId)
        };
        return OkPaged(questions, requestQuery);
    }

    [HttpPost]
    [Authorize(Roles = "Instructor,Admin")]
    [EndpointSummary("Create a quiz")]
    [ProducesResponseType<Wrapper<uint>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<uint>> CreateQuiz([FromBody] CreateQuizRequestDto requestDto, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        const uint newQuizId = 1;
        return CreatedAtAction(nameof(GetQuiz), new { quizId = newQuizId }, newQuizId);
    }

    [HttpPut("{quizId:uint}")]
    [Authorize(Roles = "Instructor,Admin")]
    [EndpointSummary("Update a quiz")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateQuiz(uint quizId, [FromBody] CreateQuizRequestDto requestDto, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return NoContent();
    }

    [HttpDelete("{quizId:uint}")]
    [Authorize(Roles = "Instructor,Admin")]
    [EndpointSummary("Delete a quiz")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteQuiz(uint quizId, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return NoContent();
    }

    [HttpGet("{quizId:uint}/editor")]
    [Authorize(Roles = "Instructor,Admin")]
    [EndpointSummary("Get quiz data for editor")]
    [ProducesResponseType<Wrapper<QuizResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuizResponseDto>> GetQuizEditor(uint quizId, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Ok(new QuizResponseDto(quizId, "Dummy Quiz", 1, null, 75.0m, 1));
    }

    [HttpGet("{quizId:uint}/attempts")]
    [Authorize(Roles = "Admin,Instructor")]
    [RequiresPaging]
    [EndpointSummary("Get all attempts for a quiz")]
    [ProducesResponseType<Wrapper<QuizAttemptResponseDto[]>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuizAttemptResponseDto[]>> GetAttempts(uint quizId, [FromQuery] RequestQuery requestQuery, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        var attempts = new[] {
            new QuizAttemptResponseDto(1, quizId, 1, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(-1).AddMinutes(20), 85.0m, true),
            new QuizAttemptResponseDto(2, quizId, 2, DateTime.UtcNow, null, 0, false)
        };
        return OkPaged(attempts, requestQuery);
    }

    [HttpGet("{quizId:uint}/my-attempts")]
    [Authorize]
    [RequiresPaging]
    [EndpointSummary("Get my quiz attempts")]
    [ProducesResponseType<Wrapper<MyQuizAttemptResponseDto[]>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<MyQuizAttemptResponseDto[]>> GetMyAttempts(uint quizId, [FromQuery] RequestQuery requestQuery, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        var attempts = new[] {
            new MyQuizAttemptResponseDto(1, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(-1).AddMinutes(20), 85.0m, true),
            new MyQuizAttemptResponseDto(2, DateTime.UtcNow, null, 0, false)
        };
        return OkPaged(attempts, requestQuery);
    }

    [HttpGet("attempt/{attemptId:uint}")]
    [Authorize]
    [EndpointSummary("Get specific quiz attempt")]
    [ProducesResponseType<Wrapper<QuizAttemptResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuizAttemptResponseDto>> GetAttempt(uint attemptId, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Ok(new QuizAttemptResponseDto(attemptId, 1, 1, DateTime.UtcNow, null, 0, false));
    }

    [HttpPost("attempt/start")]
    [Authorize(Roles = "Student")]
    [EndpointSummary("Start a quiz attempt")]
    [ProducesResponseType<Wrapper<uint>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<uint>> StartAttempt([FromBody] StartAttemptRequestDto requestDto, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        const uint newAttemptId = 1;
        return CreatedAtAction(nameof(GetAttempt), new { attemptId = newAttemptId }, newAttemptId);
    }

    [HttpPost("attempt/end")]
    [Authorize(Roles = "Student")]
    [EndpointSummary("Submit and end a quiz attempt")]
    [ProducesResponseType<Wrapper<QuizAttemptResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuizAttemptResponseDto>> EndAttempt([FromBody] EndAttemptRequestDto requestDto, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Ok(new QuizAttemptResponseDto(requestDto.AttemptId, 1, 1, DateTime.UtcNow, DateTime.UtcNow, 100.0m, true));
    }
}