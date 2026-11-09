using Backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options) {
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Section> Sections => Set<Section>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<LessonAttachment> LessonAttachments => Set<LessonAttachment>();
    public DbSet<StudentLessonProgress> StudentLessonProgresses => Set<StudentLessonProgress>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<AnswerOption> AnswerOptions => Set<AnswerOption>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
    public DbSet<StudentAnswer> StudentAnswers => Set<StudentAnswer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ==========================================
        // USER & AUTH
        // ==========================================
        modelBuilder.Entity<User>(entity => {
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<RefreshToken>(entity => {
            entity.HasIndex(r => r.TokenHash).IsUnique();
            entity.HasIndex(r => r.JwtId);
            entity.HasOne(r => r.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ==========================================
        // COURSE, SECTION & LESSON
        // ==========================================
        modelBuilder.Entity<Course>()
            .HasMany(c => c.Instructors)
            .WithMany(u => u.TeachingCourses)
            .UsingEntity("CourseInstructors");

        modelBuilder.Entity<Section>(entity => {
            entity.HasIndex(s => new { s.CourseId, s.Order });
            entity.HasOne(s => s.Course)
                .WithMany(c => c.Sections)
                .HasForeignKey(s => s.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Lesson>(entity => {
            entity.HasIndex(l => new { l.SectionId, l.Order });
            entity.HasOne(l => l.Section)
                .WithMany(s => s.Lessons)
                .HasForeignKey(l => l.SectionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LessonAttachment>()
            .HasOne(a => a.Lesson)
            .WithMany(l => l.Attachments)
            .HasForeignKey(a => a.LessonId)
            .OnDelete(DeleteBehavior.Cascade);

        // ==========================================
        // QUIZ, QUESTION & ANSWER OPTION
        // ==========================================
        modelBuilder.Entity<Quiz>(entity => {
            entity.Property(q => q.PassingThresholdPercent).HasPrecision(5, 2);
            entity.HasOne(q => q.Section)
                .WithMany(s => s.Quizzes)
                .HasForeignKey(q => q.SectionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Question>(entity => {
            entity.HasOne(q => q.Quiz)
                .WithMany(qz => qz.Questions)
                .HasForeignKey(q => q.QuizId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AnswerOption>()
            .HasOne(a => a.Question)
            .WithMany(q => q.AnswerOptions)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        // ==========================================
        // STUDENT RELATIONS (Enrollments, Progress, Attempts)
        // ==========================================
        modelBuilder.Entity<Enrollment>(entity => {
            entity.HasIndex(e => new { e.StudentId, e.CourseId }).IsUnique();
            entity.HasOne(e => e.Student)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StudentLessonProgress>(entity => {
            entity.HasIndex(p => new { p.StudentId, p.LessonId }).IsUnique();
            entity.HasOne(p => p.Student)
                .WithMany(u => u.LessonProgresses)
                .HasForeignKey(p => p.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(p => p.Lesson)
                .WithMany()
                .HasForeignKey(p => p.LessonId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuizAttempt>(entity => {
            entity.Property(qa => qa.ScorePercent).HasPrecision(5, 2);
            entity.HasIndex(a => new { a.StudentId, a.QuizId });
            entity.HasOne(a => a.Student)
                .WithMany(u => u.QuizAttempts)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(a => a.Quiz)
                .WithMany()
                .HasForeignKey(a => a.QuizId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StudentAnswer>(entity => {
            entity.HasIndex(a => new { a.QuizAttemptId, a.QuestionId }).IsUnique();
            entity.HasOne(a => a.QuizAttempt)
                .WithMany(qa => qa.StudentAnswers)
                .HasForeignKey(a => a.QuizAttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ponecháno na Restrict pro prevenci "multiple cascade paths" problému v SQL Server/MariaDB
            entity.HasOne(a => a.Question)
                .WithMany()
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}