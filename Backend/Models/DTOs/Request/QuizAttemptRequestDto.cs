using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs.Request;

public record CreateQuizRequestDto(
    [Required] [MaxLength(128)] string Title,
    [Range(1, uint.MaxValue)] uint SectionId,
    [Range(1, 100)] ushort? MaxAttempts,
    [Range(1, int.MaxValue)] uint? TimeLimitSeconds,
    [Range(0, 100)] decimal PassingThresholdPercent,
    bool RandomizeQuestions,
    bool RandomizeAnswers);

public record StartAttemptRequestDto(
    [Range(typeof(uint), "1", "4294967295")] uint QuizId);

public record EndAttemptRequestDto(
    [Range(1, uint.MaxValue)] uint AttemptId,
    [Required]
    string AnswersJson);