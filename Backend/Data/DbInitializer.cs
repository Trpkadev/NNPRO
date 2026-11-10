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
            ("instructor", "instructor@example.com", "instructor", UserRole.Instructor)
        ];

        foreach ((string username, string email, string password, var role) in defaultUsers)
        {
            bool usernameExists = await dbContext.Users.AnyAsync(user => user.Username == username, cancellationToken);
            if (usernameExists)
                continue;

            bool emailExists = await dbContext.Users.AnyAsync(user => user.Email == email, cancellationToken);
            if (emailExists)
            {
                Console.WriteLine($"[Seed] Skipping default user '{username}': email '{email}' is already in use.");
                continue;
            }

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

        dbContext.Courses.AddRange(
            new Course {
                Title = "Základy programování v C#",
                Description = "Praktický úvod do proměnných, podmínek, cyklů, metod a objektově orientovaného programování v jazyce C#. Nakopíruju to podruhý protože potřebuju strašně dlouhý string, moc děkujeme za pochopení. Praktický úvod do proměnných, podmínek, cyklů, metod a objektově orientovaného programování v jazyce C#."
            },
            new Course {
                Title = "Vývoj webových API v ASP.NET Core",
                Description = "Naučíte se navrhovat REST API, pracovat s controllery, DTO objekty, validací a databází pomocí Entity Framework Core."
            },
            new Course {
                Title = "Databáze a SQL pro vývojáře",
                Description = "Kurz pokrývá návrh relačních databází, SQL dotazy, vazby mezi tabulkami, indexy a základní optimalizaci."
            },
            new Course {
                Title = "Frontend s Reactem a TypeScriptem",
                Description = "Vytvořte moderní webovou aplikaci s Reactem, komponentami, stavem, routováním a typovou kontrolou v TypeScriptu."
            },
            new Course {
                Title = "Git a týmová spolupráce",
                Description = "Od základních příkazů Gitu až po práci s větvemi, pull requesty, řešením konfliktů a code review."
            },
            new Course {
                Title = "Úvod do kybernetické bezpečnosti",
                Description = "Seznamte se s nejčastějšími hrozbami, bezpečným ukládáním hesel, autentizací a ochranou webových aplikací.",
                AccessKey = "bezpecnost2026"
            },
            new Course {
                Title = "Algoritmy a datové struktury",
                Description = "Pochopte principy třídění, vyhledávání, rekurze, stromů a grafů s důrazem na časovou a paměťovou složitost."
            },
            new Course {
                Title = "Automatizované testování v .NET",
                Description = "Praktický kurz unit testů, integračních testů, mockování závislostí a testování webových endpointů."
            }
        );

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}