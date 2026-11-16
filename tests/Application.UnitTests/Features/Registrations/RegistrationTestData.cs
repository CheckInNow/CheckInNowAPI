using Domain.Entities;
using Infrastructure.Persistence;

namespace Application.UnitTests.Features.Registrations;

internal static class RegistrationTestData
{
    public static readonly Guid OrganizerId = Guid.NewGuid();

    public static async Task<Conference> SeedConferenceAsync(AppDbContext dbContext, int startsInDays = 30, int participantLimit = 100)
    {
        var start = DateTimeOffset.UtcNow.AddDays(startsInDays);
        var conference = new Conference
        {
            Name = "Conf",
            Description = "Description",
            StartDate = start,
            EndDate = start.AddDays(1),
            Location = "Legnica",
            ParticipantLimit = participantLimit,
            OrganizerId = OrganizerId
        };
        dbContext.Conferences.Add(conference);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return conference;
    }

    public static async Task<ApplicationUser> SeedUserAsync(
        AppDbContext dbContext,
        bool emailConfirmed = true,
        string firstName = "Anna",
        string lastName = "Nowak")
    {
        var id = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = id,
            UserName = $"user-{id:N}@test.pl",
            Email = $"user-{id:N}@test.pl",
            EmailConfirmed = emailConfirmed,
            FirstName = firstName,
            LastName = lastName
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return user;
    }

    public static async Task<Registration> SeedRegistrationAsync(
        AppDbContext dbContext,
        Guid conferenceId,
        Guid userId,
        DateTimeOffset? createdAt = null)
    {
        var registration = new Registration { ConferenceId = conferenceId, UserId = userId };
        dbContext.Registrations.Add(registration);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        if (createdAt is not null)
        {
            registration.CreatedAt = createdAt.Value;
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        return registration;
    }
}
