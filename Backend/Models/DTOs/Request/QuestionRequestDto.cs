using System.ComponentModel.DataAnnotations;
using Backend.Models.Entities;

namespace Backend.Models.DTOs.Request;

public record CreateQuestionRequestDto(
    [Required] [MaxLength(2048)] string Text,
    [Url] [MaxLength(2048)] string? ImageUrl,
    [Required] [EnumDataType(typeof(QuestionType))] string Type,
    [Range(1, ushort.MaxValue)] ushort Points,
    [Required] uint QuizId);