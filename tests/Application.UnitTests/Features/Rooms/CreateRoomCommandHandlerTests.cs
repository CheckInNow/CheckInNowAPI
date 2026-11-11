using Application.Abstractions.Identity;
using Application.Features.Rooms.Create;
using Domain.Common;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using NSubstitute;

namespace Application.UnitTests.Features.Rooms;

public sealed class CreateRoomCommandHandlerTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();

    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    public CreateRoomCommandHandlerTests()
    {
        _currentUser.UserId.Returns(OwnerId);
    }

    [Fact]
    public async Task Handle_Should_Create_Room_When_Current_User_Is_Owner()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var handler = new CreateRoomCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new CreateRoomCommand(conference.Id, "Room A", 50), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var room = dbContext.Rooms.Single();
        room.Id.Should().Be(result.Value!.Id);
        room.ConferenceId.Should().Be(conference.Id);
        room.Name.Should().Be("Room A");
        room.Capacity.Should().Be(50);
    }

    [Fact]
    public async Task Handle_Should_Return_Forbidden_When_Current_User_Is_Not_The_Owner()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        _currentUser.UserId.Returns(Guid.NewGuid());
        var handler = new CreateRoomCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new CreateRoomCommand(conference.Id, "Room A", null), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Forbidden);
        dbContext.Rooms.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Conference_Does_Not_Exist()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var handler = new CreateRoomCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new CreateRoomCommand(Guid.NewGuid(), "Room A", null), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Conference_Is_Deleted()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        conference.DeletedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var handler = new CreateRoomCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new CreateRoomCommand(conference.Id, "Room A", null), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        dbContext.Rooms.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Return_Conflict_When_Room_Name_Is_Taken_In_The_Same_Conference()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var handler = new CreateRoomCommandHandler(dbContext, _currentUser);
        await handler.Handle(new CreateRoomCommand(conference.Id, "Room A", 10), TestContext.Current.CancellationToken);

        // Act
        var result = await handler.Handle(new CreateRoomCommand(conference.Id, "Room A", 20), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Conflict);
        dbContext.Rooms.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_Should_Allow_Same_Room_Name_In_Different_Conferences()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var first = await SeedConferenceAsync(dbContext);
        var second = await SeedConferenceAsync(dbContext);
        var handler = new CreateRoomCommandHandler(dbContext, _currentUser);
        await handler.Handle(new CreateRoomCommand(first.Id, "Room A", null), TestContext.Current.CancellationToken);

        // Act
        var result = await handler.Handle(new CreateRoomCommand(second.Id, "Room A", null), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        dbContext.Rooms.Should().HaveCount(2);
    }

    private static async Task<Conference> SeedConferenceAsync(AppDbContext dbContext)
    {
        var conference = new Conference
        {
            Name = "Conf",
            Description = "Description",
            StartDate = new DateTimeOffset(2027, 5, 10, 9, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2027, 5, 11, 17, 0, 0, TimeSpan.Zero),
            Location = "Legnica",
            ParticipantLimit = 100,
            OrganizerId = OwnerId
        };
        dbContext.Conferences.Add(conference);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return conference;
    }
}
