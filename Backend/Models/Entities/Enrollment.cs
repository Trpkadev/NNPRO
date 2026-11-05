namespace Backend.Models.Entities;

public class Enrollment {
    public uint Id { get; set; }
    public uint StudentId { get; set; }
    public required User Student { get; set; }

    public uint CourseId { get; set; }
    public required Course Course { get; set; }

    // Čas zápisu studenta do kurzu v UTC.
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
}