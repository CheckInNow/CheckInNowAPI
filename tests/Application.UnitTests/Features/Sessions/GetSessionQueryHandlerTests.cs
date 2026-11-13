using Application.Features.Sessions.Get;
using Domain.Common;
using FluentAssertions;
using static Application.UnitTests.Features.Sessions.SessionTestData;

namespace Application.UnitTests.Features.Sessions;

public sealed class GetSessionQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Session_With_Room_And_Speaker_Names()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var room = await SeedRoomAsync(dbContext, conference.Id, "Main hall");
        var speaker = await SeedSpeakerAsync(dbContext, conference.Id, "Anna", "Nowak");
        var session = await SeedSessionAsync(dbContext, conference, room, speaker, At(10), At(11), "Keynote");
        var handler = new GetSessionQueryHandler(dbContext);

        // Act
        var result = await handler.Handle(new GetSessionQuery(conference.Id, session.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Title.Should().Be("Keynote");
        result.Value.RoomName.Should().Be("Main hall");
        result.Value.SpeakerName.Should().Be("Anna Nowak");
        result.Value.StartTime.Should().Be(At(10));
        result.Value.EndTime.Should().Be(At(11));
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Session_Does_Not_Exist()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var handler = new GetSessionQueryHandler(dbContext);

        // Act
        var result = await handler.Handle(new GetSessionQuery(conference.Id, Guid.NewGuid()), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Session_Belongs_To_Another_Conference()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var other = await SeedConferenceAsync(dbContext);
        var room = await SeedRoomAsync(dbContext, conference.Id);
        var speaker = await SeedSpeakerAsync(dbContext, conference.Id);
        var session = await SeedSessionAsync(dbContext, conference, room, speaker, At(10), At(11));
        var handler = new GetSessionQueryHandler(dbContext);

        // Act
        var result = await handler.Handle(new GetSessionQuery(other.Id, session.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Session_Is_Deleted()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var room = await SeedRoomAsync(dbContext, conference.Id);
        var speaker = await SeedSpeakerAsync(dbContext, conference.Id);
        var session = await SeedSessionAsync(dbContext, conference, room, speaker, At(10), At(11));
        session.DeletedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var handler = new GetSessionQueryHandler(dbContext);

        // Act
        var result = await handler.Handle(new GetSessionQuery(conference.Id, session.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }
}
