using Domain.Entities;
using Infrastructure.Persistence;

namespace Application.UnitTests.Features.Sessions;

internal static class SessionTestData
{
    public static readonly Guid OwnerId = Guid.NewGuid();
    public static readonly DateTimeOffset ConferenceStart = new(2027, 5, 10, 9, 0, 0, TimeSpan.Zero);
    public static readonly DateTimeOffset ConferenceEnd = new(2027, 5, 11, 17, 0, 0, TimeSpan.Zero);

    public static DateTimeOffset At(int hour, int minute = 0) => new(2027, 5, 10, hour, minute, 0, TimeSpan.Zero);

    public static async Task<Conference> SeedConferenceAsync(AppDbContext dbContext)
    {
        var conference = new Conference
        {
            Name = "Conf",
            Description = "Description",
            StartDate = ConferenceStart,
            EndDate = ConferenceEnd,
            Location = "Legnica",
            ParticipantLimit = 100,
            OrganizerId = OwnerId
        };
        dbContext.Conferences.Add(conference);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return conference;
    }

    public static async Task<Room> SeedRoomAsync(AppDbContext dbContext, Guid conferenceId, string name = "Room A")
    {
        var room = new Room { ConferenceId = conferenceId, Name = name };
        dbContext.Rooms.Add(room);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return room;
    }

    public static async Task<Speaker> SeedSpeakerAsync(AppDbContext dbContext, Guid conferenceId, string firstName = "Jan", string lastName = "Kowalski")
    {
        var speaker = new Speaker { ConferenceId = conferenceId, FirstName = firstName, LastName = lastName };
        dbContext.Speakers.Add(speaker);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return speaker;
    }

    public static async Task<Session> SeedSessionAsync(
        AppDbContext dbContext,
        Conference conference,
        Room room,
        Speaker speaker,
        DateTimeOffset start,
        DateTimeOffset end,
        string title = "Session")
    {
        var session = new Session
        {
            ConferenceId = conference.Id,
            RoomId = room.Id,
            SpeakerId = speaker.Id,
            Title = title,
            Description = "Old description",
            StartTime = start,
            EndTime = end
        };
        dbContext.Sessions.Add(session);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return session;
    }
}
