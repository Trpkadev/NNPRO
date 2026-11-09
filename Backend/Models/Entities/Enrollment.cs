namespace Backend.Models.Entities;

public class Enrollment {
    public uint Id { get; set; }

    public uint StudentId { get; set; }
    public User Student { get; set; } = null!;

    public uint CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public DateTime EnrolledAtUtc { get; set; } = DateTime.UtcNow;
}