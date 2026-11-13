using System.Globalization;
using Domain.Constants;
using Domain.Entities;
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
        await SeedRoomsAsync(context, logger);
        await SeedSpeakersAsync(context, logger);
        await SeedSessionsAsync(context, logger);
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

    private static async Task SeedRoomsAsync(AppDbContext context, ILogger logger)
    {
        if (await context.Rooms.AnyAsync())
        {
            return;
        }

        var conferences = await GetSeededConferencesAsync(context);

        var rooms = new List<Room>();
        AddRooms(rooms, conferences, "CheckInNow Tech Summit",
            ("Main Hall", 200), ("Workshop Room A", 40), ("Workshop Room B", 40));
        AddRooms(rooms, conferences, "Frontend Day",
            ("Main Stage", 80), ("Lab", 30));
        AddRooms(rooms, conferences, "Women in Tech Meetup",
            ("Online", null));
        AddRooms(rooms, conferences, "Data & AI Conference",
            ("Auditorium", 300), ("Room 1", null));
        AddRooms(rooms, conferences, "Past Conference (finished)",
            ("Aula", 100));
        AddRooms(rooms, conferences, "Cancelled Conference (soft deleted)",
            ("Room X", 50));

        if (rooms.Count == 0)
        {
            return;
        }

        context.Rooms.AddRange(rooms);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} sample rooms", rooms.Count);
    }

    private static async Task SeedSpeakersAsync(AppDbContext context, ILogger logger)
    {
        if (await context.Speakers.AnyAsync())
        {
            return;
        }

        var conferences = await GetSeededConferencesAsync(context);

        var speakers = new List<Speaker>();
        AddSpeakers(speakers, conferences, "CheckInNow Tech Summit",
            ("Anna", "Nowak", "Principal engineer, 10 years of building distributed systems in .NET."),
            ("Piotr", "Wiśniewski", "Cloud architect, speaks about Azure and Kubernetes."),
            ("Katarzyna", "Zielińska", null));
        AddSpeakers(speakers, conferences, "Frontend Day",
            ("Marek", "Kowalczyk", "Frontend lead, React and TypeScript enthusiast."),
            ("Ewa", "Lewandowska", "Designer turned developer, cares about accessibility."));
        AddSpeakers(speakers, conferences, "Women in Tech Meetup",
            ("Joanna", "Mazur", null));
        AddSpeakers(speakers, conferences, "Data & AI Conference",
            ("Tomasz", "Kamiński", "ML engineer, builds recommendation systems in production."),
            ("Magdalena", "Dąbrowska", "Data engineer working with streaming pipelines."));
        AddSpeakers(speakers, conferences, "Past Conference (finished)",
            ("Adam", "Szymański", "Speaker of a conference that is already over."));
        AddSpeakers(speakers, conferences, "Cancelled Conference (soft deleted)",
            ("Zofia", "Woźniak", null));

        if (speakers.Count == 0)
        {
            return;
        }

        context.Speakers.AddRange(speakers);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} sample speakers", speakers.Count);
    }

    private static async Task SeedSessionsAsync(AppDbContext context, ILogger logger)
    {
        if (await context.Sessions.AnyAsync())
        {
            return;
        }

        var conferences = new Dictionary<string, (Guid Id, DateTimeOffset StartDate)>();
        foreach (var conference in await context.Conferences.AsNoTracking().OrderBy(c => c.CreatedAt).ToListAsync())
        {
            conferences.TryAdd(conference.Name, (conference.Id, conference.StartDate));
        }

        var rooms = new Dictionary<(Guid ConferenceId, string Name), Guid>();
        foreach (var room in await context.Rooms.AsNoTracking().ToListAsync())
        {
            rooms.TryAdd((room.ConferenceId, room.Name), room.Id);
        }

        var speakers = new Dictionary<(Guid ConferenceId, string FullName), Guid>();
        foreach (var speaker in await context.Speakers.AsNoTracking().ToListAsync())
        {
            speakers.TryAdd((speaker.ConferenceId, $"{speaker.FirstName} {speaker.LastName}"), speaker.Id);
        }

        var lookup = new SessionLookup(conferences, rooms, speakers);
        var sessions = new List<Session>();

        AddSessions(sessions, lookup, "CheckInNow Tech Summit",
            new("Main Hall", "Anna Nowak", 0, "09:00", "09:45", "Opening keynote: building .NET systems that last",
                "Welcome and a look at what makes a system easy to change."),
            new("Main Hall", "Piotr Wiśniewski", 0, "09:45", "10:45", "Kubernetes for .NET developers",
                "Containers, probes and rollouts without the buzzwords."),
            new("Workshop Room A", "Anna Nowak", 0, "10:00", "12:00", "Workshop: Clean Architecture with MediatR",
                "Hands-on: commands, queries and validation pipelines."),
            new("Workshop Room B", "Katarzyna Zielińska", 0, "10:00", "11:30", "Workshop: testing with xUnit and NSubstitute"),
            new("Main Hall", "Piotr Wiśniewski", 0, "11:30", "12:15", "Observability with OpenTelemetry and Aspire",
                "Traces, metrics and logs in one dashboard."),
            new("Main Hall", "Anna Nowak", 0, "13:00", "14:00", "Lessons learned from production incidents"),
            new("Main Hall", "Katarzyna Zielińska", 1, "09:30", "10:30", "Entity Framework Core performance tips",
                "Tracking, projections and the queries that hurt."),
            new("Workshop Room A", "Piotr Wiśniewski", 1, "11:00", "12:30", "Workshop: deploying to Azure with GitHub Actions"),
            new("Workshop Room B", "Katarzyna Zielińska", 1, "13:00", "14:00", "Cancelled workshop (soft deleted)",
                "Soft deleted, should never be returned by the API.", Deleted: true),
            new("Main Hall", "Anna Nowak", 1, "15:00", "16:00", "Closing panel: the future of .NET"));

        AddSessions(sessions, lookup, "Frontend Day",
            new("Main Stage", "Marek Kowalczyk", 0, "10:00", "11:00", "Modern React: server components in practice",
                "What changes when the server renders your components."),
            new("Lab", "Marek Kowalczyk", 0, "11:00", "13:00", "Workshop: Vite, TypeScript and testing"),
            new("Main Stage", "Ewa Lewandowska", 0, "11:30", "12:30", "Accessibility is not optional"),
            new("Main Stage", "Ewa Lewandowska", 0, "14:00", "15:00", "Design systems from scratch",
                "Tokens, components and keeping designers and developers in sync."));

        AddSessions(sessions, lookup, "Women in Tech Meetup",
            new("Online", "Joanna Mazur", 0, "17:00", "17:45", "Breaking into tech: stories and lessons"),
            new("Online", "Joanna Mazur", 0, "18:00", "19:00", "Panel: growing as a technical leader",
                "Moderated discussion, questions from the audience welcome."));

        AddSessions(sessions, lookup, "Data & AI Conference",
            new("Auditorium", "Tomasz Kamiński", 0, "09:30", "10:30", "Machine learning in production: what breaks first",
                "Drift, latency and the boring failures nobody plans for."),
            new("Room 1", "Magdalena Dąbrowska", 0, "10:00", "11:30", "Streaming pipelines with Kafka"),
            new("Auditorium", "Tomasz Kamiński", 0, "11:00", "12:00", "Feature stores explained"),
            new("Auditorium", "Tomasz Kamiński", 1, "14:00", "15:30", "Evaluating models after deployment"),
            new("Auditorium", "Magdalena Dąbrowska", 2, "10:00", "11:00", "Data quality: tests for your pipelines"));

        AddSessions(sessions, lookup, "Past Conference (finished)",
            new("Aula", "Adam Szymański", 0, "10:00", "11:00", "Opening talk"),
            new("Aula", "Adam Szymański", 1, "10:00", "12:00", "Wrap-up and lessons learned"));

        if (sessions.Count == 0)
        {
            return;
        }

        context.Sessions.AddRange(sessions);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} sample sessions", sessions.Count);
    }

    private readonly record struct SessionSeed(
        string Room,
        string Speaker,
        int Day,
        string Start,
        string End,
        string Title,
        string? Description = null,
        bool Deleted = false);

    private sealed record SessionLookup(
        Dictionary<string, (Guid Id, DateTimeOffset StartDate)> Conferences,
        Dictionary<(Guid ConferenceId, string Name), Guid> Rooms,
        Dictionary<(Guid ConferenceId, string FullName), Guid> Speakers);

    private static void AddSessions(
        List<Session> sessions,
        SessionLookup lookup,
        string conferenceName,
        params SessionSeed[] items)
    {
        if (!lookup.Conferences.TryGetValue(conferenceName, out var conference))
        {
            return;
        }

        var firstDay = new DateTimeOffset(conference.StartDate.UtcDateTime.Date, TimeSpan.Zero);

        foreach (var item in items)
        {
            if (!lookup.Rooms.TryGetValue((conference.Id, item.Room), out var roomId)
                || !lookup.Speakers.TryGetValue((conference.Id, item.Speaker), out var speakerId))
            {
                continue;
            }

            var day = firstDay.AddDays(item.Day);
            sessions.Add(new Session
            {
                ConferenceId = conference.Id,
                RoomId = roomId,
                SpeakerId = speakerId,
                Title = item.Title,
                Description = item.Description ?? string.Empty,
                StartTime = day + TimeSpan.Parse(item.Start, CultureInfo.InvariantCulture),
                EndTime = day + TimeSpan.Parse(item.End, CultureInfo.InvariantCulture),
                DeletedAt = item.Deleted ? DateTimeOffset.UtcNow : null
            });
        }
    }

    private static async Task<Dictionary<string, Guid>> GetSeededConferencesAsync(AppDbContext context)
    {
        var conferences = await context.Conferences
            .OrderBy(c => c.CreatedAt)
            .Select(c => new { c.Name, c.Id })
            .ToListAsync();

        var byName = new Dictionary<string, Guid>();
        foreach (var conference in conferences)
        {
            byName.TryAdd(conference.Name, conference.Id);
        }

        return byName;
    }

    private static void AddRooms(
        List<Room> rooms,
        Dictionary<string, Guid> conferences,
        string conferenceName,
        params (string Name, int? Capacity)[] items)
    {
        if (!conferences.TryGetValue(conferenceName, out var conferenceId))
        {
            return;
        }

        rooms.AddRange(items.Select(i => new Room
        {
            ConferenceId = conferenceId,
            Name = i.Name,
            Capacity = i.Capacity
        }));
    }

    private static void AddSpeakers(
        List<Speaker> speakers,
        Dictionary<string, Guid> conferences,
        string conferenceName,
        params (string FirstName, string LastName, string? Bio)[] items)
    {
        if (!conferences.TryGetValue(conferenceName, out var conferenceId))
        {
            return;
        }

        speakers.AddRange(items.Select(i => new Speaker
        {
            ConferenceId = conferenceId,
            FirstName = i.FirstName,
            LastName = i.LastName,
            Bio = i.Bio
        }));
    }
}
