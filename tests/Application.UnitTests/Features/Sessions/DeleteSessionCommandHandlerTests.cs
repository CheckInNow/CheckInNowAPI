using Application.Abstractions.Identity;
using Application.Features.Sessions.Delete;
using Domain.Common;
using FluentAssertions;
using NSubstitute;
using static Application.UnitTests.Features.Sessions.SessionTestData;

namespace Application.UnitTests.Features.Sessions;

public sealed class DeleteSessionCommandHandlerTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    public DeleteSessionCommandHandlerTests()
    {
        _currentUser.UserId.Returns(OwnerId);
    }

    [Fact]
    public async Task Handle_Should_Soft_Delete_Session_When_Current_User_Is_Owner()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var room = await SeedRoomAsync(dbContext, conference.Id);
        var speaker = await SeedSpeakerAsync(dbContext, conference.Id);
        var session = await SeedSessionAsync(dbContext, conference, room, speaker, At(10), At(11));
        var handler = new DeleteSessionCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new DeleteSessionCommand(conference.Id, session.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        dbContext.Sessions.Should().HaveCount(1);
        dbContext.Sessions.Single().DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_Should_Return_Forbidden_When_Current_User_Is_Not_The_Owner()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var room = await SeedRoomAsync(dbContext, conference.Id);
        var speaker = await SeedSpeakerAsync(dbContext, conference.Id);
        var session = await SeedSessionAsync(dbContext, conference, room, speaker, At(10), At(11));
        _currentUser.UserId.Returns(Guid.NewGuid());
        var handler = new DeleteSessionCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new DeleteSessionCommand(conference.Id, session.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Forbidden);
        dbContext.Sessions.Single().DeletedAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Session_Does_Not_Exist()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var handler = new DeleteSessionCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new DeleteSessionCommand(conference.Id, Guid.NewGuid()), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Session_Is_Already_Deleted()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var room = await SeedRoomAsync(dbContext, conference.Id);
        var speaker = await SeedSpeakerAsync(dbContext, conference.Id);
        var session = await SeedSessionAsync(dbContext, conference, room, speaker, At(10), At(11));
        session.DeletedAt = DateTimeOffset.UtcNow.AddDays(-1);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var handler = new DeleteSessionCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new DeleteSessionCommand(conference.Id, session.Id), TestContext.Current.CancellationToken);

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
        var handler = new DeleteSessionCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new DeleteSessionCommand(other.Id, session.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        dbContext.Sessions.Single().DeletedAt.Should().BeNull();
    }
}
