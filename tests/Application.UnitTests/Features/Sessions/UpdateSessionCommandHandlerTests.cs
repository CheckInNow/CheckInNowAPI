using Application.Abstractions.Identity;
using Application.Features.Sessions.Update;
using Domain.Common;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using NSubstitute;
using static Application.UnitTests.Features.Sessions.SessionTestData;

namespace Application.UnitTests.Features.Sessions;

public sealed class UpdateSessionCommandHandlerTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    public UpdateSessionCommandHandlerTests()
    {
        _currentUser.UserId.Returns(OwnerId);
    }

    [Fact]
    public async Task Handle_Should_Update_Only_Provided_Fields()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var (conference, room, speaker, session) = await SeedAsync(dbContext);
        var handler = new UpdateSessionCommandHandler(dbContext, _currentUser);
        var command = Command(conference, session) with { Title = "New title", EndTime = At(12) };

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updated = dbContext.Sessions.Single();
        updated.Title.Should().Be("New title");
        updated.EndTime.Should().Be(At(12));
        updated.StartTime.Should().Be(At(10));
        updated.Description.Should().Be("Old description");
        updated.RoomId.Should().Be(room.Id);
        updated.SpeakerId.Should().Be(speaker.Id);
    }

    [Fact]
    public async Task Handle_Should_Clear_Description_When_Empty_String_Is_Provided()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var (conference, _, _, session) = await SeedAsync(dbContext);
        var handler = new UpdateSessionCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(Command(conference, session) with { Description = "" }, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        dbContext.Sessions.Single().Description.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Move_Session_To_Another_Room_And_Speaker()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var (conference, _, _, session) = await SeedAsync(dbContext);
        var newRoom = await SeedRoomAsync(dbContext, conference.Id, "Room B");
        var newSpeaker = await SeedSpeakerAsync(dbContext, conference.Id, "Anna", "Nowak");
        var handler = new UpdateSessionCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(
            Command(conference, session) with { RoomId = newRoom.Id, SpeakerId = newSpeaker.Id },
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        dbContext.Sessions.Single().RoomId.Should().Be(newRoom.Id);
        dbContext.Sessions.Single().SpeakerId.Should().Be(newSpeaker.Id);
    }

    [Fact]
    public async Task Handle_Should_Allow_Shifting_Session_Within_Its_Own_Slot()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var (conference, _, _, session) = await SeedAsync(dbContext);
        var handler = new UpdateSessionCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(
            Command(conference, session) with { StartTime = At(10, 30), EndTime = At(11, 30) },
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        dbContext.Sessions.Single().StartTime.Should().Be(At(10, 30));
    }

    [Fact]
    public async Task Handle_Should_Return_Forbidden_When_Current_User_Is_Not_The_Owner()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var (conference, _, _, session) = await SeedAsync(dbContext);
        _currentUser.UserId.Returns(Guid.NewGuid());
        var handler = new UpdateSessionCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(Command(conference, session) with { Title = "Hacked" }, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Forbidden);
        dbContext.Sessions.Single().Title.Should().Be("Session");
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Session_Does_Not_Exist()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var handler = new UpdateSessionCommandHandler(dbContext, _currentUser);
        var command = new UpdateSessionCommand(conference.Id, Guid.NewGuid(), "Title", null, null, null, null, null);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Session_Is_Deleted()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var (conference, _, _, session) = await SeedAsync(dbContext);
        session.DeletedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var handler = new UpdateSessionCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(Command(conference, session) with { Title = "New" }, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_New_Start_Time_Is_After_Existing_End_Time()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var (conference, _, _, session) = await SeedAsync(dbContext);
        var handler = new UpdateSessionCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(Command(conference, session) with { StartTime = At(12) }, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Validation);
        dbContext.Sessions.Single().StartTime.Should().Be(At(10));
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Session_Would_Leave_The_Conference_Time_Range()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var (conference, _, _, session) = await SeedAsync(dbContext);
        var handler = new UpdateSessionCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(Command(conference, session) with { EndTime = ConferenceEnd.AddHours(1) }, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Validation);
        result.Error.Code.Should().Be("Session.InvalidTime");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_New_Time_Overlaps_Another_Session_In_The_Same_Room()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var (conference, room, _, session) = await SeedAsync(dbContext);
        var otherSpeaker = await SeedSpeakerAsync(dbContext, conference.Id, "Anna", "Nowak");
        await SeedSessionAsync(dbContext, conference, room, otherSpeaker, At(11), At(12), "Next");
        var handler = new UpdateSessionCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(Command(conference, session) with { EndTime = At(11, 30) }, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Session.RoomOverlapping");
    }

    [Fact]
    public async Task Handle_Should_Allow_Session_Right_After_Another_One_In_The_Same_Room()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var (conference, room, _, session) = await SeedAsync(dbContext);
        var otherSpeaker = await SeedSpeakerAsync(dbContext, conference.Id, "Anna", "Nowak");
        await SeedSessionAsync(dbContext, conference, room, otherSpeaker, At(11), At(12), "Next");
        var handler = new UpdateSessionCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(Command(conference, session) with { EndTime = At(11) }, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Moving_To_A_Room_That_Is_Busy()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var (conference, _, _, session) = await SeedAsync(dbContext);
        var roomB = await SeedRoomAsync(dbContext, conference.Id, "Room B");
        var otherSpeaker = await SeedSpeakerAsync(dbContext, conference.Id, "Anna", "Nowak");
        await SeedSessionAsync(dbContext, conference, roomB, otherSpeaker, At(10), At(11), "Parallel");
        var handler = new UpdateSessionCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(Command(conference, session) with { RoomId = roomB.Id }, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Session.RoomOverlapping");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Speaker_Is_Busy_At_The_New_Time()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var (conference, _, speaker, session) = await SeedAsync(dbContext);
        var roomB = await SeedRoomAsync(dbContext, conference.Id, "Room B");
        await SeedSessionAsync(dbContext, conference, roomB, speaker, At(12), At(13), "Same speaker");
        var handler = new UpdateSessionCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(Command(conference, session) with { EndTime = At(12, 30) }, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Session.SpeakerBusy");
    }

    [Fact]
    public async Task Handle_Should_Ignore_Deleted_Sessions_When_Checking_Overlaps()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var (conference, room, _, session) = await SeedAsync(dbContext);
        var otherSpeaker = await SeedSpeakerAsync(dbContext, conference.Id, "Anna", "Nowak");
        var deleted = await SeedSessionAsync(dbContext, conference, room, otherSpeaker, At(11), At(12), "Deleted");
        deleted.DeletedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var handler = new UpdateSessionCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(Command(conference, session) with { EndTime = At(11, 30) }, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Room_Belongs_To_Another_Conference()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var (conference, _, _, session) = await SeedAsync(dbContext);
        var other = await SeedConferenceAsync(dbContext);
        var foreignRoom = await SeedRoomAsync(dbContext, other.Id, "Foreign");
        var handler = new UpdateSessionCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(Command(conference, session) with { RoomId = foreignRoom.Id }, TestContext.Current.CancellationToken);

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
        var (conference, _, _, session) = await SeedAsync(dbContext);
        var other = await SeedConferenceAsync(dbContext);
        var foreignSpeaker = await SeedSpeakerAsync(dbContext, other.Id, "Foreign", "Speaker");
        var handler = new UpdateSessionCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(Command(conference, session) with { SpeakerId = foreignSpeaker.Id }, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("Speaker.NotFound");
    }

    private static UpdateSessionCommand Command(Conference conference, Session session) =>
        new(conference.Id, session.Id, null, null, null, null, null, null);

    private static async Task<(Conference Conference, Room Room, Speaker Speaker, Session Session)> SeedAsync(AppDbContext dbContext)
    {
        var conference = await SeedConferenceAsync(dbContext);
        var room = await SeedRoomAsync(dbContext, conference.Id);
        var speaker = await SeedSpeakerAsync(dbContext, conference.Id);
        var session = await SeedSessionAsync(dbContext, conference, room, speaker, At(10), At(11));
        return (conference, room, speaker, session);
    }
}
