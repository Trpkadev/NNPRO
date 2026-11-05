using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs.Response;

public class QuizDto {
    [Required] [MaxLength(128)]
    public string Title { get; set; } = string.Empty;

    // null == neomezený počet pokusů.
    [Range(1, ushort.MaxValue)]
    public ushort? MaxAttempts { get; set; }

    // Časový limit jednoho pokusu v sekundách
    // null == bez limitu.
    [Range(1, uint.MaxValue)]
    public uint? TimeLimitSeconds { get; set; }

    [Range(0, 100)]
    // Minimální procentuální skóre potřebné ke splnění kvízu.
    public double PassingThresholdPercent { get; set; }
}