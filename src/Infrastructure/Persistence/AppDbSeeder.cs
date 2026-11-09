using Domain.Entities;
using Domain.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence;

public static class AppDbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        await context.Database.MigrateAsync();

        await SeedRolesAsync(roleManager, logger);
        await SeedAdminUserAsync(userManager, logger);
        await SeedOrganizersAsync(userManager, logger);
        await SeedConferencesAsync(context, userManager, logger);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager, ILogger logger)
    {
        List<string> roles = new([Roles.Admin, Roles.User, Roles.Organizer]);
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                logger.LogInformation("Created role: {Role}", role);
            }
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, ILogger logger)
    {
        const string adminEmail = "admin@example.dev";

        if (await userManager.FindByEmailAsync(adminEmail) is not null)
        {
            return;
        }

        var admin = new ApplicationUser
        {
            FirstName = "Admin",
            LastName = "User",
            Email = adminEmail,
            UserName = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, "Admin123!");

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, Roles.Admin);
            logger.LogInformation("Seeded admin user: {Email}", adminEmail);
        }
    }

    private static async Task SeedOrganizersAsync(UserManager<ApplicationUser> userManager, ILogger logger)
    {
        await SeedOrganizerAsync(userManager, logger, "organizer@example.dev", "Olga", "Organizer");
        await SeedOrganizerAsync(userManager, logger, "organizer2@example.dev", "Piotr", "Organizer");
    }

    private static async Task SeedOrganizerAsync(
        UserManager<ApplicationUser> userManager,
        ILogger logger,
        string email,
        string firstName,
        string lastName)
    {
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return;
        }

        var organizer = new ApplicationUser
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            UserName = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(organizer, "Organizer123!");

        if (result.Succeeded)
        {
            await userManager.AddToRolesAsync(organizer, [Roles.User, Roles.Organizer]);
            logger.LogInformation("Seeded organizer user: {Email}", email);
        }
    }

    private static async Task SeedConferencesAsync(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger logger)
    {
        if (await context.Conferences.AnyAsync())
        {
            return;
        }

        var organizer = await userManager.FindByEmailAsync("organizer@example.dev");
        var otherOrganizer = await userManager.FindByEmailAsync("organizer2@example.dev");
        if (organizer is null || otherOrganizer is null)
        {
            return;
        }

        var today = new DateTimeOffset(DateTimeOffset.UtcNow.Date, TimeSpan.Zero);

        var conferences = new List<Conference>
        {
            new()
            {
                Name = "CheckInNow Tech Summit",
                Description = "Two days of talks about modern .NET, cloud and architecture.",
                StartDate = today.AddDays(30).AddHours(9),
                EndDate = today.AddDays(31).AddHours(17),
                Location = "Collegium Witelona, ul. Sejmowa 5, Legnica",
                ParticipantLimit = 200,
                OrganizerId = organizer.Id
            },
            new()
            {
                Name = "Frontend Day",
                Description = "React, Vite and the future of the web.",
                StartDate = today.AddDays(45).AddHours(10),
                EndDate = today.AddDays(45).AddHours(18),
                Location = "Centrum Kultury, Wrocław",
                ParticipantLimit = 80,
                OrganizerId = organizer.Id
            },
            new()
            {
                Name = "Women in Tech Meetup",
                Description = string.Empty,
                StartDate = today.AddDays(60).AddHours(17),
                EndDate = today.AddDays(60).AddHours(20),
                Location = "Online",
                ParticipantLimit = 500,
                OrganizerId = organizer.Id
            },
            new()
            {
                Name = "Data & AI Conference",
                Description = "Machine learning in production, data pipelines and lessons learned.",
                StartDate = today.AddDays(90).AddHours(9),
                EndDate = today.AddDays(92).AddHours(16),
                Location = "Hotel Qubus, Kraków",
                ParticipantLimit = 300,
                OrganizerId = otherOrganizer.Id
            },
            new()
            {
                Name = "Past Conference (finished)",
                Description = "Already finished conference, useful for testing past events.",
                StartDate = today.AddDays(-60).AddHours(9),
                EndDate = today.AddDays(-59).AddHours(17),
                Location = "Politechnika Wrocławska",
                ParticipantLimit = 100,
                OrganizerId = otherOrganizer.Id
            },
            new()
            {
                Name = "Cancelled Conference (soft deleted)",
                Description = "Soft deleted, should never be returned by the API.",
                StartDate = today.AddDays(20).AddHours(9),
                EndDate = today.AddDays(20).AddHours(17),
                Location = "Gdańsk",
                ParticipantLimit = 50,
                OrganizerId = organizer.Id,
                DeletedAt = DateTimeOffset.UtcNow
            }
        };

        context.Conferences.AddRange(conferences);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} sample conferences", conferences.Count);
    }
}
