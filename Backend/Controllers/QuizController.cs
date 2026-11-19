using Backend.Extensions;
using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
[Route("api/quizzes")]
[Tags("Quizzes")]
public class QuizController(IQuizService quizService) : ApiControllerBase {
    [HttpGet("{quizId:uint}")]
    [Authorize]
    [EndpointSummary("Get quiz detail")]
    [ProducesResponseType<Wrapper<QuizResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuizResponseDto>> GetQuiz(uint quizId, CancellationToken cancellationToken) => Ok(await quizService.GetByIdAsync(quizId, cancellationToken));

    [HttpGet("{quizId:uint}/questions")]
    [Authorize]
    [RequiresPaging]
    [EndpointSummary("Get quiz questions")]
    [EndpointDescription("Returns the questions belonging to a quiz without answer keys.")]
    [ProducesResponseType<Wrapper<QuestionResponseDto[]>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuestionResponseDto[]>> GetQuestions(uint quizId, [FromQuery] RequestQuery requestQuery, CancellationToken cancellationToken)
    {
        var questions = await quizService.GetQuestionsAsync(quizId, cancellationToken);
        return OkPaged(questions, requestQuery);
    }

    [HttpPost]
    [Authorize(Roles = "Instructor,Admin")]
    [EndpointSummary("Create a quiz")]
    [ProducesResponseType<Wrapper<uint>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<uint>> CreateQuiz([FromBody] CreateQuizRequestDto requestDto, CancellationToken cancellationToken)
    {
        uint newQuizId = await quizService.CreateAsync(AuthenticatedUserId, requestDto, cancellationToken);
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
        await quizService.UpdateAsync(AuthenticatedUserId, quizId, requestDto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{quizId:uint}")]
    [Authorize(Roles = "Instructor,Admin")]
    [EndpointSummary("Delete a quiz")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteQuiz(uint quizId, CancellationToken cancellationToken)
    {
        await quizService.DeleteAsync(AuthenticatedUserId, quizId, cancellationToken);
        return NoContent();
    }

    [HttpGet("{quizId:uint}/editor")]
    [Authorize(Roles = "Instructor,Admin")]
    [EndpointSummary("Get quiz data for editor")]
    [ProducesResponseType<Wrapper<QuizResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuizResponseDto>> GetQuizEditor(uint quizId, CancellationToken cancellationToken) => Ok(await quizService.GetForEditorAsync(AuthenticatedUserId, quizId, cancellationToken));

    [HttpGet("{quizId:uint}/attempts")]
    [Authorize(Roles = "Admin,Instructor")]
    [RequiresPaging]
    [EndpointSummary("Get all attempts for a quiz")]
    [ProducesResponseType<Wrapper<QuizAttemptResponseDto[]>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<QuizAttemptResponseDto[]>> GetAttempts(uint quizId, [FromQuery] RequestQuery requestQuery, CancellationToken cancellationToken)
    {
        var attempts = await quizService.GetAttemptsAsync(AuthenticatedUserId, quizId, cancellationToken);
        return OkPaged(attempts, requestQuery);
    }

    [HttpGet("{quizId:uint}/my-attempts")]
    [Authorize]
    [RequiresPaging]
    [EndpointSummary("Get my quiz attempts")]
    [ProducesResponseType<Wrapper<MyQuizAttemptResponseDto[]>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MyQuizAttemptResponseDto[]>> GetMyAttempts(uint quizId, [FromQuery] RequestQuery requestQuery, CancellationToken cancellationToken)
    {
        var attempts = await quizService.GetMyAttemptsAsync(AuthenticatedUserId, quizId, cancellationToken);
        return OkPaged(attempts, requestQuery);
    }

    [HttpGet("attempt/{attemptId:uint}")]
    [Authorize]
    [EndpointSummary("Get specific quiz attempt")]
    [ProducesResponseType<Wrapper<QuizAttemptResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuizAttemptResponseDto>> GetAttempt(uint attemptId, CancellationToken cancellationToken) => Ok(await quizService.GetAttemptAsync(AuthenticatedUserId, attemptId, cancellationToken));

    [HttpPost("attempt/start")]
    [Authorize(Roles = "Student")]
    [EndpointSummary("Start a quiz attempt")]
    [ProducesResponseType<Wrapper<uint>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<uint>> StartAttempt([FromBody] StartAttemptRequestDto requestDto, CancellationToken cancellationToken)
    {
        uint newAttemptId = await quizService.StartAttemptAsync(AuthenticatedUserId, requestDto, cancellationToken);
        return CreatedAtAction(nameof(GetAttempt), new { attemptId = newAttemptId }, newAttemptId);
    }

    [HttpPost("attempt/end")]
    [Authorize(Roles = "Student")]
    [EndpointSummary("Submit and end a quiz attempt")]
    [EndpointDescription("AnswersJson must be a JSON object mapping question IDs to arrays of selected option IDs.")]
    [ProducesResponseType<Wrapper<QuizAttemptResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<QuizAttemptResponseDto>> EndAttempt([FromBody] EndAttemptRequestDto requestDto, CancellationToken cancellationToken) => Ok(await quizService.EndAttemptAsync(AuthenticatedUserId, requestDto, cancellationToken));
}