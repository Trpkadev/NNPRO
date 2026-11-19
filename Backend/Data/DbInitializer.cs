using System.Text.Json;
using Backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public static class DbInitializer {
    public async static Task SeedUsersAsync(IDbContextFactory<AppDbContext> dbContextFactory, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        (string Username, string Email, string Password, UserRole Role)[] defaultUsers = [
            ("admin", "admin@example.com", "admin", UserRole.Admin),
            ("student", "student@example.com", "student", UserRole.Student),
            ("instructor", "instructor@example.com", "instructor", UserRole.Instructor),
            ("student2", "student2@example.com", "student2", UserRole.Student),
            ("instructor2", "instructor2@example.com", "instructor2", UserRole.Instructor)
        ];

        foreach ((string username, string email, string password, var role) in defaultUsers)
        {
            bool usernameExists = await dbContext.Users.AnyAsync(user => user.Username == username, cancellationToken);
            if (usernameExists)
                continue;

            bool emailExists = await dbContext.Users.AnyAsync(user => user.Email == email, cancellationToken);
            if (emailExists)
                continue;

            dbContext.Users.Add(new User {
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = role
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async static Task SeedCoursesAsync(IDbContextFactory<AppDbContext> dbContextFactory, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        if (await dbContext.Courses.AnyAsync(cancellationToken))
            return;

        var instructor1 = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == "instructor", cancellationToken);
        var instructor2 = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == "instructor2", cancellationToken);
        var student1 = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == "student", cancellationToken);
        var student2 = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == "student2", cancellationToken);

        List<User> primaryInstructors = instructor1 is not null ? [instructor1] : [];
        var secondaryInstructors = instructor2 is not null ? [instructor2] : primaryInstructors;
        List<User> bothInstructors = [.. new[] { instructor1, instructor2 }.OfType<User>()];

        // 1. Příprava lekcí a příloh
        var lessonCsharp1 = new Lesson {
            Title = "Proměnné, datové typy a operátory",
            Content = "# Proměnné a datové typy\n\nV jazyce C# používáme silnou typovou kontrolu (`int`, `uint`, `string`, `decimal`, `bool`).",
            VideoUrl = "https://example.com/videos/csharp-variables",
            Order = 1,
            Attachments = [
                new LessonAttachment {
                    FileName = "csharp-datove-typy-tahak.pdf",
                    Url = "https://example.com/files/csharp-datove-typy-tahak.pdf",
                    ContentType = "application/pdf",
                    SizeBytes = 245_760
                },
                new LessonAttachment {
                    FileName = "ukazkovy-kod-lekce1.zip",
                    Url = "https://example.com/files/ukazkovy-kod-lekce1.zip",
                    ContentType = "application/zip",
                    SizeBytes = 18_432
                }
            ]
        };

        var lessonCsharp2 = new Lesson {
            Title = "Řídicí struktury: podmínky a cykly",
            Content = "# Podmínky a cykly\n\nUkázka použití `if`, `switch` expressions, `foreach` a `while`.",
            VideoUrl = "https://example.com/videos/csharp-control-flow",
            Order = 2
        };

        var lessonCsharp3 = new Lesson {
            Title = "Třídy, rozhraní a dědičnost",
            Content = "# OOP v C#\n\nZapouzdření, polymorfismus a práce s `record` typy.",
            Order = 1,
            Attachments = [
                new LessonAttachment {
                    FileName = "oop-principy-prezentace.pdf",
                    Url = "https://example.com/files/oop-principy-prezentace.pdf",
                    ContentType = "application/pdf",
                    SizeBytes = 512_000
                }
            ]
        };

        var lessonAsp1 = new Lesson {
            Title = "Návrh endpointů a DTO objektů",
            Content = "# REST API a DTO\n\nOddělení databázových entit od kontraktů API pomocí Data Transfer Objects.",
            VideoUrl = "https://example.com/videos/aspnet-rest-dto",
            Order = 1
        };

        var lessonAsp2 = new Lesson {
            Title = "DbContext, relace a migrace",
            Content = "# Entity Framework Core\n\nKonfigurace relací 1:N a M:N pomocí Fluent API.",
            Order = 1,
            Attachments = [
                new LessonAttachment {
                    FileName = "ef-core-migrace-navod.md",
                    Url = "https://example.com/files/ef-core-migrace-navod.md",
                    ContentType = "text/markdown",
                    SizeBytes = 8_192
                }
            ]
        };

        var lessonSec1 = new Lesson {
            Title = "Hashování hesel a JWT tokeny",
            Content = "# Bezpečná autentizace\n\nProč používat BCrypt/Argon2 a krátkodobé access tokeny s rotací refresh tokenů.",
            VideoUrl = "https://example.com/videos/security-jwt-hashing",
            Order = 1
        };

        // 2. Příprava kvízů, otázek a možností odpovědí
        var q1Opt1 = new AnswerOption { Text = "decimal", IsCorrect = true };
        var q1Opt2 = new AnswerOption { Text = "float", IsCorrect = false };
        var q1Opt3 = new AnswerOption { Text = "double", IsCorrect = false };

        var q2Opt1 = new AnswerOption { Text = "struct", IsCorrect = true };
        var q2Opt2 = new AnswerOption { Text = "enum", IsCorrect = true };
        var q2Opt3 = new AnswerOption { Text = "class", IsCorrect = false };

        var q3Opt1 = new AnswerOption { Text = "Pravda", IsCorrect = true };
        var q3Opt2 = new AnswerOption { Text = "Nepravda", IsCorrect = false };

        var q1 = new Question {
            Text = "Který datový typ v C# je nejvhodnější pro přesné finanční výpočty?",
            Type = QuestionType.SingleChoice,
            Points = 2,
            AnswerOptions = [q1Opt1, q1Opt2, q1Opt3]
        };

        var q2 = new Question {
            Text = "Které z následujících klíčových slov definují hodnotový typ (value type)?",
            ImageUrl = "https://example.com/images/csharp-type-hierarchy.png",
            Type = QuestionType.MultipleChoice,
            Points = 3,
            AnswerOptions = [q2Opt1, q2Opt2, q2Opt3]
        };

        var q3 = new Question {
            Text = "Typ `string` je v C# neměnný (immutable).",
            Type = QuestionType.TrueFalse,
            Points = 1,
            AnswerOptions = [q3Opt1, q3Opt2]
        };

        var csharpQuiz = new Quiz {
            Title = "Ověření znalostí: Základy C#",
            MaxAttempts = 3,
            TimeLimitSeconds = 600,
            PassingThresholdPercent = 70m,
            RandomizeQuestions = true,
            RandomizeAnswers = true,
            Questions = [q1, q2, q3]
        };

        var qAsp1Opt1 = new AnswerOption { Text = ".Include()", IsCorrect = true };
        var qAsp1Opt2 = new AnswerOption { Text = ".AsNoTracking()", IsCorrect = false };
        var qAsp1Opt3 = new AnswerOption { Text = ".SaveChangesAsync()", IsCorrect = false };

        var qAsp1 = new Question {
            Text = "Která metoda v EF Core načte navigační vlastnost v jednom dotazu (eager loading)?",
            Type = QuestionType.SingleChoice,
            Points = 2,
            AnswerOptions = [qAsp1Opt1, qAsp1Opt2, qAsp1Opt3]
        };

        var aspQuiz = new Quiz {
            Title = "Kvíz: ASP.NET Core & EF Core",
            MaxAttempts = 2,
            TimeLimitSeconds = 900,
            PassingThresholdPercent = 75m,
            RandomizeQuestions = false,
            RandomizeAnswers = true,
            Questions = [qAsp1]
        };

        // 3. Kurzy se sekcemi a přiřazenými instruktory
        var csharpCourse = new Course {
            Title = "Základy programování v C#",
            Description = "Praktický úvod do proměnných, podmínek, cyklů, metod a objektově orientovaného programování v jazyce C#. Nakopíruju to podruhý protože potřebuju strašně dlouhý string, moc děkujeme za pochopení. Praktický úvod do proměnných, podmínek, cyklů, metod a objektově orientovaného programování v jazyce C#.",
            Instructors = bothInstructors,
            Sections = [
                new Section {
                    Title = "Úvod do jazyka C# a .NET",
                    Order = 1,
                    Lessons = [lessonCsharp1, lessonCsharp2],
                    Quizzes = [csharpQuiz]
                },
                new Section {
                    Title = "Objektově orientované programování",
                    Order = 2,
                    Lessons = [lessonCsharp3]
                }
            ]
        };

        var aspNetCourse = new Course {
            Title = "Vývoj webových API v ASP.NET Core",
            Description = "Naučíte se navrhovat REST API, pracovat s controllery, DTO objekty, validací a databází pomocí Entity Framework Core.",
            Instructors = primaryInstructors,
            Sections = [
                new Section {
                    Title = "Architektura REST API a Controllery",
                    Order = 1,
                    Lessons = [lessonAsp1]
                },
                new Section {
                    Title = "Entity Framework Core a migrace",
                    Order = 2,
                    Lessons = [lessonAsp2],
                    Quizzes = [aspQuiz]
                }
            ]
        };

        var sqlCourse = new Course {
            Title = "Databáze a SQL pro vývojáře",
            Description = "Kurz pokrývá návrh relačních databází, SQL dotazy, vazby mezi tabulkami, indexy a základní optimalizaci.",
            Instructors = secondaryInstructors
        };

        var reactCourse = new Course {
            Title = "Frontend s Reactem a TypeScriptem",
            Description = "Vytvořte moderní webovou aplikaci s Reactem, komponentami, stavem, routováním a typovou kontrolou v TypeScriptu.",
            Instructors = secondaryInstructors
        };

        var gitCourse = new Course {
            Title = "Git a týmová spolupráce",
            Description = "Od základních příkazů Gitu až po práci s větvemi, pull requesty, řešením konfliktů a code review.",
            Instructors = primaryInstructors
        };

        var securityCourse = new Course {
            Title = "Úvod do kybernetické bezpečnosti",
            Description = "Seznamte se s nejčastějšími hrozbami, bezpečným ukládáním hesel, autentizací a ochranou webových aplikací.",
            AccessKey = "bezpecnost2026",
            Instructors = secondaryInstructors,
            Sections = [
                new Section {
                    Title = "Základy autentizace a kryptografie",
                    Order = 1,
                    Lessons = [lessonSec1]
                }
            ]
        };

        var algoCourse = new Course {
            Title = "Algoritmy a datové struktury",
            Description = "Pochopte principy třídění, vyhledávání, rekurze, stromů a grafů s důrazem na časovou a paměťovou složitost.",
            Instructors = primaryInstructors
        };

        var testingCourse = new Course {
            Title = "Automatizované testování v .NET",
            Description = "Praktický kurz unit testů, integračních testů, mockování závislostí a testování webových endpointů.",
            Instructors = secondaryInstructors
        };

        dbContext.Courses.AddRange(
            csharpCourse,
            aspNetCourse,
            sqlCourse,
            reactCourse,
            gitCourse,
            securityCourse,
            algoCourse,
            testingCourse
        );

        await dbContext.SaveChangesAsync(cancellationToken);

        // 4. Zápisy studentů do kurzů, postup v lekcích a pokusy o kvízy (po uložení už známe reálná DB Id)
        var enrollments = dbContext.Set<Enrollment>();
        var lessonProgresses = dbContext.Set<StudentLessonProgress>();
        var quizAttempts = dbContext.Set<QuizAttempt>();

        if (student1 is not null)
        {
            enrollments.AddRange(
                new Enrollment { StudentId = student1.Id, CourseId = csharpCourse.Id, EnrolledAtUtc = DateTime.UtcNow.AddDays(-7) },
                new Enrollment { StudentId = student1.Id, CourseId = aspNetCourse.Id, EnrolledAtUtc = DateTime.UtcNow.AddDays(-3) },
                new Enrollment { StudentId = student1.Id, CourseId = securityCourse.Id, EnrolledAtUtc = DateTime.UtcNow.AddDays(-1) }
            );

            lessonProgresses.AddRange(
                new StudentLessonProgress {
                    StudentId = student1.Id,
                    LessonId = lessonCsharp1.Id,
                    IsCompleted = true,
                    CompletedAtUtc = DateTime.UtcNow.AddDays(-6)
                },
                new StudentLessonProgress {
                    StudentId = student1.Id,
                    LessonId = lessonCsharp2.Id,
                    IsCompleted = true,
                    CompletedAtUtc = DateTime.UtcNow.AddDays(-5)
                },
                new StudentLessonProgress {
                    StudentId = student1.Id,
                    LessonId = lessonCsharp3.Id,
                    IsCompleted = false,
                    CompletedAtUtc = null
                },
                new StudentLessonProgress {
                    StudentId = student1.Id,
                    LessonId = lessonAsp1.Id,
                    IsCompleted = true,
                    CompletedAtUtc = DateTime.UtcNow.AddDays(-2)
                }
            );

            // Úspěšný pokus (6/6 bodů = 100 %)
            var passedAttempt = new QuizAttempt {
                QuizId = csharpQuiz.Id,
                StudentId = student1.Id,
                StartedAtUtc = DateTime.UtcNow.AddDays(-5).AddMinutes(-8),
                SubmittedAtUtc = DateTime.UtcNow.AddDays(-5),
                ScorePercent = 100m,
                IsPassed = true,
                StudentAnswers = [
                    new StudentAnswer {
                        QuestionId = q1.Id,
                        SelectedOptionIdsJson = JsonSerializer.Serialize(new[] { q1Opt1.Id }),
                        IsFullyCorrect = true,
                        AwardedPoints = 2
                    },
                    new StudentAnswer {
                        QuestionId = q2.Id,
                        SelectedOptionIdsJson = JsonSerializer.Serialize(new[] { q2Opt1.Id, q2Opt2.Id }),
                        IsFullyCorrect = true,
                        AwardedPoints = 3
                    },
                    new StudentAnswer {
                        QuestionId = q3.Id,
                        SelectedOptionIdsJson = JsonSerializer.Serialize(new[] { q3Opt1.Id }),
                        IsFullyCorrect = true,
                        AwardedPoints = 1
                    }
                ]
            };

            // Rozpracovaný (neodevzdaný) pokus
            var inProgressAttempt = new QuizAttempt {
                QuizId = aspQuiz.Id,
                StudentId = student1.Id,
                StartedAtUtc = DateTime.UtcNow.AddMinutes(-3),
                SubmittedAtUtc = null,
                ScorePercent = 0m,
                IsPassed = false
            };

            quizAttempts.AddRange(passedAttempt, inProgressAttempt);
        }

        if (student2 is not null)
        {
            enrollments.AddRange(
                new Enrollment { StudentId = student2.Id, CourseId = csharpCourse.Id, EnrolledAtUtc = DateTime.UtcNow.AddDays(-4) },
                new Enrollment { StudentId = student2.Id, CourseId = sqlCourse.Id, EnrolledAtUtc = DateTime.UtcNow.AddDays(-2) }
            );

            lessonProgresses.Add(
                new StudentLessonProgress {
                    StudentId = student2.Id,
                    LessonId = lessonCsharp1.Id,
                    IsCompleted = true,
                    CompletedAtUtc = DateTime.UtcNow.AddDays(-3)
                }
            );

            // Neúspěšný pokus (1/6 bodů = 16.67 %)
            var failedAttempt = new QuizAttempt {
                QuizId = csharpQuiz.Id,
                StudentId = student2.Id,
                StartedAtUtc = DateTime.UtcNow.AddDays(-3).AddMinutes(-5),
                SubmittedAtUtc = DateTime.UtcNow.AddDays(-3),
                ScorePercent = 16.67m,
                IsPassed = false,
                StudentAnswers = [
                    new StudentAnswer {
                        QuestionId = q1.Id,
                        SelectedOptionIdsJson = JsonSerializer.Serialize(new[] { q1Opt2.Id }),
                        IsFullyCorrect = false,
                        AwardedPoints = 0
                    },
                    new StudentAnswer {
                        QuestionId = q2.Id,
                        SelectedOptionIdsJson = JsonSerializer.Serialize(new[] { q2Opt1.Id }),
                        IsFullyCorrect = false,
                        AwardedPoints = 0
                    },
                    new StudentAnswer {
                        QuestionId = q3.Id,
                        SelectedOptionIdsJson = JsonSerializer.Serialize(new[] { q3Opt1.Id }),
                        IsFullyCorrect = true,
                        AwardedPoints = 1
                    }
                ]
            };

            quizAttempts.Add(failedAttempt);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}