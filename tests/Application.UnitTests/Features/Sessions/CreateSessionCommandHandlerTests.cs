using Application.Abstractions.Identity;
using Application.Features.Sessions.Create;
using Domain.Common;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using NSubstitute;
using static Application.UnitTests.Features.Sessions.SessionTestData;

namespace Application.UnitTests.Features.Sessions;

public sealed class CreateSessionCommandHandlerTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    public CreateSessionCommandHandlerTests()
    {
        _currentUser.UserId.Returns(OwnerId);
    }

    [Fact]
    public async Task Handle_Should_Create_Session_And_Return_It()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var (conference, room, speaker) = await SeedAsync(dbContext);
        var handler = new CreateSessionCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(Command(conference, room, speaker), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Title.Should().Be("New session");
        var session = dbContext.Sessions.Single();
        session.Id.Should().Be(result.Value.Id);
        session.ConferenceId.Should().Be(conference.Id);
        session.RoomId.Should().Be(room.Id);
        session.SpeakerId.Should().Be(speaker.Id);
        session.StartTime.Should().Be(At(10));
        session.EndTime.Should().Be(At(11));
        session.DeletedAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_Store_Empty_Description_When_Description_Is_Null()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var (conference, room, speaker) = await SeedAsync(dbContext);
        var handler = new CreateSessionCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(Command(conference, room, speaker) with { Description = null }, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        dbContext.Sessions.Single().Description.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Normalize_Times_To_Utc()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var (conference, room, speaker) = await SeedAsync(dbContext);
        var handler = new CreateSessionCommandHandler(dbContext, _currentUser);
        var plusTwo = TimeSpan.FromHours(2);
        var command = Command(conference, room, speaker) with
        {
            StartTime = new DateTimeOffset(2027, 5, 10, 12, 0, 0, plusTwo),
            EndTime = new DateTimeOffset(2027, 5, 10, 13, 0, 0, plusTwo)
        };

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var session = dbContext.Sessions.Single();
        session.StartTime.Offset.Should().Be(TimeSpan.Zero);
        session.StartTime.Should().Be(At(10));
        session.EndTime.Should().Be(At(11));
    }

    [Fact]
    public async Task Handle_Should_Return_Forbidden_When_Current_User_Is_Not_The_Owner()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var (conference, room, speaker) = await SeedAsync(dbContext);
        _currentUser.UserId.Returns(Guid.NewGuid());
        var handler = new CreateSessionCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(Command(conference, room, speaker), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Forbidden);
        dbContext.Sessions.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Conference_Does_Not_Exist()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var (conference, room, speaker) = await SeedAsync(dbContext);
        var handler = new CreateSessionCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(Command(conference, room, speaker) with { ConferenceId = Guid.NewGuid() }, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("Conference.NotFound");
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Conference_Is_Deleted()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var (conference, room, speaker) = await SeedAsync(dbContext);
        conference.DeletedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var handler = new CreateSessionCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(Command(conference, room, speaker), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Conference.NotFound");
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Room_Belongs_To_Another_Conference()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var (conference, _, speaker) = await SeedAsync(dbContext);
        var other = await SeedConferenceAsync(dbContext);
        var foreignRoom = await SeedRoomAsync(dbContext, other.Id, "Foreign");
        var handler = new CreateSessionCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(Command(conference, foreignRoom, speaker), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("Room.NotFound");
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Speaker_Belongs_To_Another_Conference()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var (conference, room, _) = await SeedAsync(dbContext);
        var other = await SeedConferenceAsync(dbContext);
        var foreignSpeaker = await SeedSpeakerAsync(dbContext, other.Id, "Foreign", "Speaker");
        var handler = new CreateSessionCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(Command(conference, room, foreignSpeaker), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("Speaker.NotFound");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Session_Starts_Before_The_Conference()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var (conference, room, speaker) = await SeedAsync(dbContext);
        var handler = new CreateSessionCommandHandler(dbContext, _currentUser);
        var command = Command(conference, room, speaker) with { StartTime = ConferenceStart.AddHours(-1), EndTime = At(10) };

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Validation);
        result.Error.Code.Should().Be("Session.InvalidTime");
        dbContext.Sessions.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Session_Ends_After_The_Conference()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var (conference, room, speaker) = await SeedAsync(dbContext);
        var handler = new CreateSessionCommandHandler(dbContext, _currentUser);
        var command = Command(conference, room, speaker) with { StartTime = ConferenceEnd.AddHours(-1), EndTime = ConferenceEnd.AddHours(1) };

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Session.InvalidTime");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Session_Overlaps_Another_One_In_The_Same_Room()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var (conference, room, speaker) = await SeedAsync(dbContext);
        var otherSpeaker = await SeedSpeakerAsync(dbContext, conference.Id, "Anna", "Nowak");
        await SeedSessionAsync(dbContext, conference, room, otherSpeaker, At(10, 30), At(11, 30));
        var handler = new CreateSessionCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(Command(conference, room, speaker), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Session.RoomOverlapping");
        dbContext.Sessions.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_Should_Allow_Session_Starting_Exactly_When_Another_Ends_In_The_Same_Room()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var (conference, room, speaker) = await SeedAsync(dbContext);
        var otherSpeaker = await SeedSpeakerAsync(dbContext, conference.Id, "Anna", "Nowak");
        await SeedSessionAsync(dbContext, conference, room, otherSpeaker, At(9), At(10));
        var handler = new CreateSessionCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(Command(conference, room, speaker), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_Allow_Parallel_Session_In_Another_Room()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var (conference, room, speaker) = await SeedAsync(dbContext);
        var roomB = await SeedRoomAsync(dbContext, conference.Id, "Room B");
        var otherSpeaker = await SeedSpeakerAsync(dbContext, conference.Id, "Anna", "Nowak");
        await SeedSessionAsync(dbContext, conference, roomB, otherSpeaker, At(10), At(11));
        var handler = new CreateSessionCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(Command(conference, room, speaker), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_Ignore_Deleted_Sessions_When_Checking_Room_Overlap()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var (conference, room, speaker) = await SeedAsync(dbContext);
        var otherSpeaker = await SeedSpeakerAsync(dbContext, conference.Id, "Anna", "Nowak");
        var deleted = await SeedSessionAsync(dbContext, conference, room, otherSpeaker, At(10), At(11));
        deleted.DeletedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var handler = new CreateSessionCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(Command(conference, room, speaker), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Speaker_Already_Has_A_Session_At_The_Same_Time()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var (conference, room, speaker) = await SeedAsync(dbContext);
        var roomB = await SeedRoomAsync(dbContext, conference.Id, "Room B");
        await SeedSessionAsync(dbContext, conference, roomB, speaker, At(10, 30), At(11, 30));
        var handler = new CreateSessionCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(Command(conference, room, speaker), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Session.SpeakerBusy");
    }

    private static CreateSessionCommand Command(Conference conference, Room room, Speaker speaker) =>
        new(conference.Id, "New session", "Description", At(10), At(11), room.Id, speaker.Id);

    private static async Task<(Conference Conference, Room Room, Speaker Speaker)> SeedAsync(AppDbContext dbContext)
    {
        var conference = await SeedConferenceAsync(dbContext);
        var room = await SeedRoomAsync(dbContext, conference.Id);
        var speaker = await SeedSpeakerAsync(dbContext, conference.Id);
        return (conference, room, speaker);
    }
}
