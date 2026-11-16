using Application.Abstractions.Identity;
using Application.Features.Registrations.Delete;
using Domain.Common;
using FluentAssertions;
using NSubstitute;
using static Application.UnitTests.Features.Registrations.RegistrationTestData;

namespace Application.UnitTests.Features.Registrations;

public sealed class DeleteRegistrationCommandHandlerTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    [Fact]
    public async Task Handle_Should_Hard_Delete_Registration_When_Current_User_Is_Its_Owner()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var user = await SeedUserAsync(dbContext);
        var registration = await SeedRegistrationAsync(dbContext, conference.Id, user.Id);
        _currentUser.UserId.Returns(user.Id);
        var handler = new DeleteRegistrationCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new DeleteRegistrationCommand(conference.Id, registration.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        dbContext.Registrations.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Return_Forbidden_When_Current_User_Is_Not_The_Owner()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var user = await SeedUserAsync(dbContext);
        var registration = await SeedRegistrationAsync(dbContext, conference.Id, user.Id);
        _currentUser.UserId.Returns(Guid.NewGuid());
        var handler = new DeleteRegistrationCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new DeleteRegistrationCommand(conference.Id, registration.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Forbidden);
        dbContext.Registrations.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_Should_Return_Conflict_When_Conference_Has_Already_Started()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext, startsInDays: -1);
        var user = await SeedUserAsync(dbContext);
        var registration = await SeedRegistrationAsync(dbContext, conference.Id, user.Id);
        _currentUser.UserId.Returns(user.Id);
        var handler = new DeleteRegistrationCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new DeleteRegistrationCommand(conference.Id, registration.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Conflict);
        result.Error.Code.Should().Be("Registration.ConferenceStarted");
        dbContext.Registrations.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Registration_Does_Not_Exist()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        _currentUser.UserId.Returns(Guid.NewGuid());
        var handler = new DeleteRegistrationCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new DeleteRegistrationCommand(conference.Id, Guid.NewGuid()), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Registration_Belongs_To_Another_Conference()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var other = await SeedConferenceAsync(dbContext);
        var user = await SeedUserAsync(dbContext);
        var registration = await SeedRegistrationAsync(dbContext, conference.Id, user.Id);
        _currentUser.UserId.Returns(user.Id);
        var handler = new DeleteRegistrationCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new DeleteRegistrationCommand(other.Id, registration.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        dbContext.Registrations.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Conference_Is_Deleted()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var user = await SeedUserAsync(dbContext);
        var registration = await SeedRegistrationAsync(dbContext, conference.Id, user.Id);
        conference.DeletedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        _currentUser.UserId.Returns(user.Id);
        var handler = new DeleteRegistrationCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new DeleteRegistrationCommand(conference.Id, registration.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        dbContext.Registrations.Should().HaveCount(1);
    }
}
