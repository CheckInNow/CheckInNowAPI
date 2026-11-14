using Application.Features.Sessions.GetAll;
using Domain.Common;
using FluentAssertions;
using static Application.UnitTests.Features.Sessions.SessionTestData;

namespace Application.UnitTests.Features.Sessions;

public sealed class GetAllSessionsQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Only_Active_Sessions_Of_The_Conference_Ordered_By_Start_Time()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var other = await SeedConferenceAsync(dbContext);
        var room = await SeedRoomAsync(dbContext, conference.Id);
        var speaker = await SeedSpeakerAsync(dbContext, conference.Id);
        var otherRoom = await SeedRoomAsync(dbContext, other.Id);
        var otherSpeaker = await SeedSpeakerAsync(dbContext, other.Id);

        await SeedSessionAsync(dbContext, conference, room, speaker, At(12), At(13), "Later");
        await SeedSessionAsync(dbContext, conference, room, speaker, At(10), At(11), "Earlier");
        var deleted = await SeedSessionAsync(dbContext, conference, room, speaker, At(14), At(15), "Deleted");
        deleted.DeletedAt = DateTimeOffset.UtcNow;
        await SeedSessionAsync(dbContext, other, otherRoom, otherSpeaker, At(9), At(10), "Other conference");
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var handler = new GetAllSessionsQueryHandler(dbContext);

        // Act
        var result = await handler.Handle(new GetAllSessionsQuery(conference.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Select(s => s.Title).Should().Equal("Earlier", "Later");
        result.Value.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_Should_Return_Requested_Page()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var room = await SeedRoomAsync(dbContext, conference.Id);
        var speaker = await SeedSpeakerAsync(dbContext, conference.Id);
        await SeedSessionAsync(dbContext, conference, room, speaker, At(10), At(11), "First");
        await SeedSessionAsync(dbContext, conference, room, speaker, At(11), At(12), "Second");
        await SeedSessionAsync(dbContext, conference, room, speaker, At(12), At(13), "Third");
        var handler = new GetAllSessionsQueryHandler(dbContext);

        // Act
        var result = await handler.Handle(new GetAllSessionsQuery(conference.Id, Page: 2, PageSize: 2), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Select(s => s.Title).Should().Equal("Third");
        result.Value.TotalCount.Should().Be(3);
        result.Value.HasPreviousPage.Should().BeTrue();
        result.Value.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_Page_When_Conference_Has_No_Sessions()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var handler = new GetAllSessionsQueryHandler(dbContext);

        // Act
        var result = await handler.Handle(new GetAllSessionsQuery(conference.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Conference_Does_Not_Exist()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var handler = new GetAllSessionsQueryHandler(dbContext);

        // Act
        var result = await handler.Handle(new GetAllSessionsQuery(Guid.NewGuid()), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }
}
