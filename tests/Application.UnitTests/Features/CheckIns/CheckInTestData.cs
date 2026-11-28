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
        var conference = await Sessions.SessionTestData.SeedConferenceAsync(dbContext);
        conference.StartDate = startDate;
        conference.EndDate = endDate;
        conference.OrganizerId = Registrations.RegistrationTestData.OrganizerId;

        var room = await Sessions.SessionTestData.SeedRoomAsync(dbContext, conference.Id);

        var speaker = await Sessions.SessionTestData.SeedSpeakerAsync(dbContext, conference.Id);

        var sessionEnded = await Sessions.SessionTestData.SeedSessionAsync(dbContext, conference, room, speaker, startDate.AddMinutes(5), startDate.AddMinutes(10));
        var sessionRunning = await Sessions.SessionTestData.SeedSessionAsync(dbContext, conference, room, speaker, startDate.AddMinutes(5), endDate.AddMinutes(-5));

        conference.Rooms.Add(room);
        conference.Speakers.Add(speaker);
        conference.Sessions.Add(sessionEnded);
        conference.Sessions.Add(sessionRunning);

        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return conference;
    }

    public static Task<Session> SeedExtraSessionAsync(
        AppDbContext dbContext,
        Conference conference,
        DateTimeOffset start,
        DateTimeOffset end)
    {
        return Sessions.SessionTestData.SeedSessionAsync(
            dbContext,
            conference,
            conference.Rooms.First(),
            conference.Speakers.First(),
            start,
            end);
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
            CheckedInAt = DateTimeOffset.UtcNow.AddMinutes(-1)
        };
        dbContext.ConferenceCheckIns.Add(checkIn);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return checkIn;
    }
}
