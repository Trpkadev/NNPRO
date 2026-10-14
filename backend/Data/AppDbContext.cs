using backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options) {
    public DbSet<User> Users => Set<User>();
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

        // Jeden student může být v konkrétním kurzu zapsaný pouze jednou.
        modelBuilder.Entity<Enrollment>()
            .HasIndex(e => new { e.StudentId, e.CourseId })
            .IsUnique();

        // Přihlašovací údaje uživatele musí být v systému jedinečné.
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Zápis vždy odkazuje na existujícího studenta a kurz.
        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Student)
            .WithMany()
            .HasForeignKey(e => e.StudentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Course)
            .WithMany()
            .HasForeignKey(e => e.CourseId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // Kurz může spravovat více lektorů; jeden lektor může spravovat více kurzů.
        modelBuilder.Entity<Course>()
            .HasMany(c => c.Instructors)
            .WithMany()
            .UsingEntity("CourseInstructors");

        // Pro každého studenta a lekci existuje nejvýše jeden záznam průchodu.
        modelBuilder.Entity<StudentLessonProgress>()
            .HasIndex(p => new { p.StudentId, p.LessonId })
            .IsUnique();
        modelBuilder.Entity<StudentLessonProgress>()
            .HasIndex(p => p.LessonId);

        // Každý kvíz patří právě do jedné sekce; sekce může mít více kvízů.
        // Vazba používá SectionId jako cizí klíč na straně kvízu.
        modelBuilder.Entity<Quiz>()
            .HasIndex(q => q.SectionId);
        modelBuilder.Entity<Quiz>()
            .HasOne(q => q.Section)
            .WithMany()
            .HasForeignKey(q => q.SectionId)
            // Smazáním sekce se smažou i její kvízy.
            .OnDelete(DeleteBehavior.Cascade);

        // Lekce se načítají v pořadí v rámci tematické sekce.
        modelBuilder.Entity<Lesson>()
            .HasIndex(l => new { l.SectionId, l.Order });

        // Sekce se načítají v pořadí v rámci kurzu.
        modelBuilder.Entity<Section>()
            .HasIndex(s => new { s.CourseId, s.Order });

        // Každá příloha patří právě k jedné lekci.
        modelBuilder.Entity<LessonAttachment>()
            .HasOne(a => a.Lesson)
            .WithMany()
            .HasForeignKey(a => a.LessonId)
            // Smazáním lekce se smažou i její přílohy.
            .OnDelete(DeleteBehavior.Cascade);

        // Jedna otázka může mít více odpovědí, ale odpověď patří právě k jedné otázce.
        modelBuilder.Entity<Question>()
            .HasIndex(q => q.QuizId);
        modelBuilder.Entity<AnswerOption>()
            .HasIndex(a => a.QuestionId);
        modelBuilder.Entity<AnswerOption>()
            .HasOne(a => a.Question)
            .WithMany()
            .HasForeignKey(a => a.QuestionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Jeden pokus může obsahovat nejvýše jednu odpověď na konkrétní otázku.
        modelBuilder.Entity<QuizAttempt>()
            .HasIndex(a => new { a.StudentId, a.QuizId });
        modelBuilder.Entity<StudentAnswer>()
            .HasIndex(a => new { a.QuizAttemptId, a.QuestionId })
            .IsUnique();
        modelBuilder.Entity<StudentAnswer>()
            .HasOne(a => a.QuizAttempt)
            .WithMany()
            .HasForeignKey(a => a.QuizAttemptId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<StudentAnswer>()
            .HasOne(a => a.Question)
            .WithMany()
            .HasForeignKey(a => a.QuestionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // Průchod lekcí i pokus o kvíz vždy patří konkrétnímu studentovi.
        modelBuilder.Entity<StudentLessonProgress>()
            .HasOne(p => p.Student)
            .WithMany()
            .HasForeignKey(p => p.StudentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<QuizAttempt>()
            .HasOne(a => a.Student)
            .WithMany()
            .HasForeignKey(a => a.StudentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}