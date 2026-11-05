using System.ComponentModel.DataAnnotations;

namespace Backend.Models.Entities;

public class Quiz {
    public uint Id { get; set; }

    [Required] [MaxLength(128)]
    public string Title { get; set; } = string.Empty;

    public uint SectionId { get; set; }
    public required Section Section { get; set; }

    // null == neomezený počet pokusů.
    [Range(1, ushort.MaxValue)]
    public ushort? MaxAttempts { get; set; }

    // Časový limit jednoho pokusu v sekundách
    // null == bez limitu.
    [Range(1, uint.MaxValue)]
    public uint? TimeLimitSeconds { get; set; }

    [Range(0, 100)]
    // Minimální procentuální skóre potřebné ke splnění kvízu.
    public decimal PassingThresholdPercent { get; set; }

    // Při každém pokusu se zamíchá pořadí otázek.
    public bool RandomizeQuestions { get; set; }

    // Při každém pokusu se zamíchá pořadí variant odpovědí.
    public bool RandomizeAnswers { get; set; }
}