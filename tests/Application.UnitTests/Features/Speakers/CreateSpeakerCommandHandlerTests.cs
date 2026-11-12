using Application.Abstractions.Identity;
using Application.Features.Speakers.Create;
using Domain.Common;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using NSubstitute;

namespace Application.UnitTests.Features.Speakers;

public sealed class CreateSpeakerCommandHandlerTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();

    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    public CreateSpeakerCommandHandlerTests()
    {
        _currentUser.UserId.Returns(OwnerId);
    }

    [Fact]
    public async Task Handle_Should_Create_Speaker_When_Current_User_Is_Owner()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var handler = new CreateSpeakerCommandHandler(dbContext, _currentUser);
        var command = new CreateSpeakerCommand(conference.Id, "Jan", "Kowalski", "Backend developer.");

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var speaker = dbContext.Speakers.Single();
        speaker.Id.Should().Be(result.Value!.Id);
        speaker.ConferenceId.Should().Be(conference.Id);
        speaker.FirstName.Should().Be("Jan");
        speaker.LastName.Should().Be("Kowalski");
        speaker.Bio.Should().Be("Backend developer.");
    }

    [Fact]
    public async Task Handle_Should_Allow_Missing_Bio()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var handler = new CreateSpeakerCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(
            new CreateSpeakerCommand(conference.Id, "Jan", "Kowalski", null), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        dbContext.Speakers.Single().Bio.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_Return_Forbidden_When_Current_User_Is_Not_The_Owner()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        _currentUser.UserId.Returns(Guid.NewGuid());
        var handler = new CreateSpeakerCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(
            new CreateSpeakerCommand(conference.Id, "Jan", "Kowalski", null), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Forbidden);
        dbContext.Speakers.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Conference_Does_Not_Exist()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var handler = new CreateSpeakerCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(
            new CreateSpeakerCommand(Guid.NewGuid(), "Jan", "Kowalski", null), TestContext.Current.CancellationToken);

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
        var handler = new CreateSpeakerCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(
            new CreateSpeakerCommand(conference.Id, "Jan", "Kowalski", null), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        dbContext.Speakers.Should().BeEmpty();
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
