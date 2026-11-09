using Application.Abstractions.Identity;
using Application.Features.Conferences.Create;
using Domain.Common;
using FluentAssertions;
using NSubstitute;

namespace Application.UnitTests.Features.Conferences;

public sealed class CreateConferenceCommandHandlerTests
{
    private static readonly Guid OrganizerId = Guid.NewGuid();

    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    public CreateConferenceCommandHandlerTests()
    {
        _currentUser.UserId.Returns(OrganizerId);
    }

    [Fact]
    public async Task Handle_Should_Create_Conference_And_Set_Current_User_As_Owner()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var handler = new CreateConferenceCommandHandler(dbContext, _currentUser);
        var command = ValidCommand();

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var conference = dbContext.Conferences.Single();
        conference.Id.Should().Be(result.Value!.Id);
        conference.Name.Should().Be("CheckIn Conf");
        conference.Location.Should().Be("Legnica");
        conference.ParticipantLimit.Should().Be(100);
        conference.OrganizerId.Should().Be(OrganizerId);
    }

    [Fact]
    public async Task Handle_Should_Store_Empty_Description_When_Description_Is_Null()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var handler = new CreateConferenceCommandHandler(dbContext, _currentUser);

        // Act
        await handler.Handle(ValidCommand() with { Description = null }, TestContext.Current.CancellationToken);

        // Assert
        dbContext.Conferences.Single().Description.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Store_Dates_In_Utc()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var handler = new CreateConferenceCommandHandler(dbContext, _currentUser);
        var start = new DateTimeOffset(2027, 5, 10, 10, 0, 0, TimeSpan.FromHours(2));
        var command = ValidCommand() with { StartDate = start, EndDate = start.AddDays(1) };

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var conference = dbContext.Conferences.Single();
        conference.StartDate.Offset.Should().Be(TimeSpan.Zero);
        conference.StartDate.Should().Be(start);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Current_User_Is_Unknown()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        _currentUser.UserId.Returns((Guid?)null);
        var handler = new CreateConferenceCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(ValidCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Forbidden);
        dbContext.Conferences.Should().BeEmpty();
    }

    private static CreateConferenceCommand ValidCommand() => new(
        "CheckIn Conf",
        "Description",
        new DateTimeOffset(2027, 5, 10, 9, 0, 0, TimeSpan.Zero),
        new DateTimeOffset(2027, 5, 11, 17, 0, 0, TimeSpan.Zero),
        "Legnica",
        100);
}
