using Application.Abstractions.Identity;
using Application.Features.Registrations.Create;
using Domain.Common;
using FluentAssertions;
using NSubstitute;
using static Application.UnitTests.Features.Registrations.RegistrationTestData;

namespace Application.UnitTests.Features.Registrations;

public sealed class CreateRegistrationCommandHandlerTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    [Fact]
    public async Task Handle_Should_Create_Registration_For_Verified_User()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var user = await SeedUserAsync(dbContext);
        _currentUser.UserId.Returns(user.Id);
        var handler = new CreateRegistrationCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new CreateRegistrationCommand(conference.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.ConferenceId.Should().Be(conference.Id);
        result.Value.UserId.Should().Be(user.Id);
        dbContext.Registrations.Should().ContainSingle(r => r.Id == result.Value.Id && r.UserId == user.Id && r.ConferenceId == conference.Id);
    }

    [Fact]
    public async Task Handle_Should_Allow_Registration_When_Only_The_Last_Place_Is_Left()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext, participantLimit: 2);
        var first = await SeedUserAsync(dbContext);
        var second = await SeedUserAsync(dbContext);
        await SeedRegistrationAsync(dbContext, conference.Id, first.Id);
        _currentUser.UserId.Returns(second.Id);
        var handler = new CreateRegistrationCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new CreateRegistrationCommand(conference.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        dbContext.Registrations.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_Should_Ignore_Registrations_To_Other_Conferences_When_Counting_The_Limit()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext, participantLimit: 1);
        var other = await SeedConferenceAsync(dbContext);
        var someone = await SeedUserAsync(dbContext);
        var user = await SeedUserAsync(dbContext);
        await SeedRegistrationAsync(dbContext, other.Id, someone.Id);
        await SeedRegistrationAsync(dbContext, other.Id, user.Id);
        _currentUser.UserId.Returns(user.Id);
        var handler = new CreateRegistrationCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new CreateRegistrationCommand(conference.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_Return_Forbidden_When_User_Is_Not_Authenticated()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        _currentUser.UserId.Returns((Guid?)null);
        var handler = new CreateRegistrationCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new CreateRegistrationCommand(conference.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Forbidden);
        dbContext.Registrations.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Conference_Does_Not_Exist()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var user = await SeedUserAsync(dbContext);
        _currentUser.UserId.Returns(user.Id);
        var handler = new CreateRegistrationCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new CreateRegistrationCommand(Guid.NewGuid()), TestContext.Current.CancellationToken);

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
        var user = await SeedUserAsync(dbContext);
        _currentUser.UserId.Returns(user.Id);
        var handler = new CreateRegistrationCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new CreateRegistrationCommand(conference.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        dbContext.Registrations.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Return_Forbidden_When_Email_Is_Not_Confirmed()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var user = await SeedUserAsync(dbContext, emailConfirmed: false);
        _currentUser.UserId.Returns(user.Id);
        var handler = new CreateRegistrationCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new CreateRegistrationCommand(conference.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Forbidden);
        result.Error.Code.Should().Be("Registration.EmailNotConfirmed");
        dbContext.Registrations.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Return_Conflict_When_Conference_Has_Already_Started()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext, startsInDays: -1);
        var user = await SeedUserAsync(dbContext);
        _currentUser.UserId.Returns(user.Id);
        var handler = new CreateRegistrationCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new CreateRegistrationCommand(conference.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Conflict);
        result.Error.Code.Should().Be("Registration.ConferenceStarted");
        dbContext.Registrations.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Return_Conflict_When_User_Is_Already_Registered()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var user = await SeedUserAsync(dbContext);
        await SeedRegistrationAsync(dbContext, conference.Id, user.Id);
        _currentUser.UserId.Returns(user.Id);
        var handler = new CreateRegistrationCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new CreateRegistrationCommand(conference.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Conflict);
        result.Error.Code.Should().Be("Registration.AlreadyRegistered");
        dbContext.Registrations.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_Should_Return_Conflict_When_Participant_Limit_Is_Reached()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext, participantLimit: 1);
        var first = await SeedUserAsync(dbContext);
        var second = await SeedUserAsync(dbContext);
        await SeedRegistrationAsync(dbContext, conference.Id, first.Id);
        _currentUser.UserId.Returns(second.Id);
        var handler = new CreateRegistrationCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new CreateRegistrationCommand(conference.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Conflict);
        result.Error.Code.Should().Be("Registration.ParticipantLimitReached");
        dbContext.Registrations.Should().HaveCount(1);
    }
}
