using System.ComponentModel.DataAnnotations;

namespace Backend.Models.Entities;

public class Quiz {
    public uint Id { get; set; }

    [Required]
    [MaxLength(128)]
    public string Title { get; set; } = string.Empty;

    public uint SectionId { get; set; }
    public Section Section { get; set; } = null!;

    [Range(1, ushort.MaxValue)]
    public ushort? MaxAttempts { get; set; }

    [Range(1, uint.MaxValue)]
    public uint? TimeLimitSeconds { get; set; }

    [Range(0, 100)]
    public decimal PassingThresholdPercent { get; set; }

    public bool RandomizeQuestions { get; set; }
    public bool RandomizeAnswers { get; set; }

    public ICollection<Question> Questions { get; set; } = [];
}