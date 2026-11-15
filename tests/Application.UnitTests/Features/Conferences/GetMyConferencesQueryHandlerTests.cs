using Application.Abstractions.Identity;
using Application.Features.Conferences.GetMy;
using Domain.Common;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using NSubstitute;

namespace Application.UnitTests.Features.Conferences;

public sealed class GetMyConferencesQueryHandlerTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();
    private static readonly Guid OtherOrganizerId = Guid.NewGuid();

    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    public GetMyConferencesQueryHandlerTests()
    {
        _currentUser.UserId.Returns(OwnerId);
    }

    [Fact]
    public async Task Handle_Should_Return_Only_Own_Active_Conferences_Ordered_By_Start_Date()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        await SeedConferenceAsync(dbContext, OwnerId, "Later", StartAt(20));
        await SeedConferenceAsync(dbContext, OwnerId, "Earlier", StartAt(10));
        await SeedConferenceAsync(dbContext, OwnerId, "Deleted", StartAt(5), deleted: true);
        await SeedConferenceAsync(dbContext, OtherOrganizerId, "Foreign", StartAt(1));
        var handler = new GetMyConferencesQueryHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new GetMyConferencesQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Select(c => c.Name).Should().Equal("Earlier", "Later");
        result.Value.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_Should_Not_Return_Conferences_Of_Another_Organizer()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        await SeedConferenceAsync(dbContext, OwnerId, "Mine", StartAt(10));
        _currentUser.UserId.Returns(OtherOrganizerId);
        var handler = new GetMyConferencesQueryHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new GetMyConferencesQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Should_Return_Requested_Page()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        await SeedConferenceAsync(dbContext, OwnerId, "First", StartAt(10));
        await SeedConferenceAsync(dbContext, OwnerId, "Second", StartAt(11));
        await SeedConferenceAsync(dbContext, OwnerId, "Third", StartAt(12));
        await SeedConferenceAsync(dbContext, OwnerId, "Deleted", StartAt(13), deleted: true);
        await SeedConferenceAsync(dbContext, OtherOrganizerId, "Foreign", StartAt(14));
        var handler = new GetMyConferencesQueryHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new GetMyConferencesQuery(Page: 2, PageSize: 2), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Select(c => c.Name).Should().Equal("Third");
        result.Value.TotalCount.Should().Be(3);
        result.Value.HasPreviousPage.Should().BeTrue();
        result.Value.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_Return_Forbidden_When_Current_User_Is_Unknown()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        await SeedConferenceAsync(dbContext, OwnerId, "Mine", StartAt(10));
        _currentUser.UserId.Returns((Guid?)null);
        var handler = new GetMyConferencesQueryHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new GetMyConferencesQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Forbidden);
    }

    private static DateTimeOffset StartAt(int day) => new(2027, 5, day, 9, 0, 0, TimeSpan.Zero);

    private static async Task SeedConferenceAsync(
        AppDbContext dbContext, Guid organizerId, string name, DateTimeOffset startDate, bool deleted = false)
    {
        dbContext.Conferences.Add(new Conference
        {
            Name = name,
            Description = "Description",
            StartDate = startDate,
            EndDate = startDate.AddDays(1),
            Location = "Legnica",
            ParticipantLimit = 100,
            OrganizerId = organizerId,
            DeletedAt = deleted ? DateTimeOffset.UtcNow : null
        });
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }
}
