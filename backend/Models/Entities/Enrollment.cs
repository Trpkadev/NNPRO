namespace backend.Models.Entities;

public class Enrollment {
    public int Id { get; set; }
    public int StudentId { get; set; }
    public required User Student { get; set; }

    public int CourseId { get; set; }
    public required Course Course { get; set; }

    // Čas zápisu studenta do kurzu v UTC.
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
}