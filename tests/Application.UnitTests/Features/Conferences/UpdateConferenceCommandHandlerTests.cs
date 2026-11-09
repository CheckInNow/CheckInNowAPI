using Application.Abstractions.Identity;
using Application.Features.Conferences.Update;
using Domain.Common;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using NSubstitute;

namespace Application.UnitTests.Features.Conferences;

public sealed class UpdateConferenceCommandHandlerTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();
    private static readonly DateTimeOffset Start = new(2027, 5, 10, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset End = new(2027, 5, 11, 17, 0, 0, TimeSpan.Zero);

    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    public UpdateConferenceCommandHandlerTests()
    {
        _currentUser.UserId.Returns(OwnerId);
    }

    [Fact]
    public async Task Handle_Should_Update_Only_Provided_Fields()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var handler = new UpdateConferenceCommandHandler(dbContext, _currentUser);
        var command = Command(conference.Id) with { Name = "New name", ParticipantLimit = 50 };

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updated = dbContext.Conferences.Single();
        updated.Name.Should().Be("New name");
        updated.ParticipantLimit.Should().Be(50);
        updated.Description.Should().Be("Old description");
        updated.Location.Should().Be("Old location");
        updated.StartDate.Should().Be(Start);
        updated.EndDate.Should().Be(End);
    }

    [Fact]
    public async Task Handle_Should_Clear_Description_When_Empty_String_Is_Provided()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var handler = new UpdateConferenceCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(Command(conference.Id) with { Description = "" }, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        dbContext.Conferences.Single().Description.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Return_Forbidden_When_Current_User_Is_Not_The_Owner()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        _currentUser.UserId.Returns(Guid.NewGuid());
        var handler = new UpdateConferenceCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(Command(conference.Id) with { Name = "Hacked" }, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Forbidden);
        dbContext.Conferences.Single().Name.Should().Be("Old name");
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Conference_Does_Not_Exist()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var handler = new UpdateConferenceCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(Command(Guid.NewGuid()), TestContext.Current.CancellationToken);

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
        var handler = new UpdateConferenceCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(Command(conference.Id) with { Name = "New" }, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_New_Start_Date_Is_After_Existing_End_Date()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var handler = new UpdateConferenceCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(Command(conference.Id) with { StartDate = End.AddDays(1) }, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Validation);
        dbContext.Conferences.Single().StartDate.Should().Be(Start);
    }

    private static UpdateConferenceCommand Command(Guid id) => new(id, null, null, null, null, null, null);

    private static async Task<Conference> SeedConferenceAsync(AppDbContext dbContext)
    {
        var conference = new Conference
        {
            Name = "Old name",
            Description = "Old description",
            StartDate = Start,
            EndDate = End,
            Location = "Old location",
            ParticipantLimit = 100,
            OrganizerId = OwnerId
        };
        dbContext.Conferences.Add(conference);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return conference;
    }
}
