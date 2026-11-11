using Application.Features.Rooms.Get;
using Domain.Common;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;

namespace Application.UnitTests.Features.Rooms;

public sealed class GetRoomsQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Only_Rooms_Of_The_Given_Conference_Ordered_By_Name()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var other = await SeedConferenceAsync(dbContext);
        dbContext.Rooms.AddRange(
            new Room { ConferenceId = conference.Id, Name = "B" },
            new Room { ConferenceId = conference.Id, Name = "A", Capacity = 30 },
            new Room { ConferenceId = other.Id, Name = "Other" });
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var handler = new GetRoomsQueryHandler(dbContext);

        // Act
        var result = await handler.Handle(new GetRoomsQuery(conference.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Select(r => r.Name).Should().Equal("A", "B");
        result.Value![0].Capacity.Should().Be(30);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_Conference_Has_No_Rooms()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var handler = new GetRoomsQueryHandler(dbContext);

        // Act
        var result = await handler.Handle(new GetRoomsQuery(conference.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Conference_Does_Not_Exist()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var handler = new GetRoomsQueryHandler(dbContext);

        // Act
        var result = await handler.Handle(new GetRoomsQuery(Guid.NewGuid()), TestContext.Current.CancellationToken);

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
        var handler = new GetRoomsQueryHandler(dbContext);

        // Act
        var result = await handler.Handle(new GetRoomsQuery(conference.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
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
            OrganizerId = Guid.NewGuid()
        };
        dbContext.Conferences.Add(conference);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return conference;
    }
}
