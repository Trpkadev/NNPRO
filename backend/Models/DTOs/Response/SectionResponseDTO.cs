using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs.Response;

public class SectionDto {
    [Required] [MaxLength(128)]
    public string Title { get; set; } = string.Empty;

    [Range(0, ushort.MaxValue)]
    public ushort Order { get; set; } // Pořadí sekce v kurzu
}