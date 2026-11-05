namespace Backend.Models.DTOs.Response;

public class StudentLessonProgressDto {
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
}