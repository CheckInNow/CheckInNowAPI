using Domain.Entities;
using Infrastructure.Persistence;

namespace Application.UnitTests.Features.CheckIns;

internal static class CheckInTestData
{
    public static async Task<Conference> SeedConferenceWithDatesAsync(
        AppDbContext dbContext,
        DateTimeOffset startDate,
        DateTimeOffset endDate)
    {
        var conference = await Registrations.RegistrationTestData.SeedConferenceAsync(dbContext);
        conference.StartDate = startDate;
        conference.EndDate = endDate;
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return conference;
    }

    public static Task<Conference> SeedRunningConferenceAsync(AppDbContext dbContext)
    {
        var now = DateTimeOffset.UtcNow;
        return SeedConferenceWithDatesAsync(dbContext, now.AddHours(-1), now.AddHours(1));
    }

    public static async Task<ConferenceCheckIn> SeedCheckInAsync(AppDbContext dbContext, Guid registrationId)
    {
        var checkIn = new ConferenceCheckIn
        {
            RegistrationId = registrationId,
            CheckedInAt = DateTimeOffset.UtcNow
        };
        dbContext.ConferenceCheckIns.Add(checkIn);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return checkIn;
    }
}
