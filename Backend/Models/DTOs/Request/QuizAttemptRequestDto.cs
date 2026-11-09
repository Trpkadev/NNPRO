using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs.Request;

public record CreateQuizRequestDto(
    [Required] [MaxLength(128)] string Title,
    [Required] uint SectionId,
    [Range(1, 100)] ushort? MaxAttempts,
    [Range(1, int.MaxValue)] uint? TimeLimitSeconds,
    [Range(0, 100)] decimal PassingThresholdPercent,
    bool RandomizeQuestions,
    bool RandomizeAnswers);

public record StartAttemptRequestDto(
    uint QuizId);

public record EndAttemptRequestDto(
    uint AttemptId,
    string AnswersJson);