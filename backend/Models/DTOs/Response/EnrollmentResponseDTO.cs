namespace Backend.Models.DTOs.Response;

public class EnrollmentDto {
    // Čas zápisu studenta do kurzu v UTC.
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
}