using Application.Abstractions.Identity;
using Application.Features.Registrations.Get;
using Domain.Common;
using FluentAssertions;
using NSubstitute;
using static Application.UnitTests.Features.Registrations.RegistrationTestData;

namespace Application.UnitTests.Features.Registrations;

public sealed class GetRegistrationQueryHandlerTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    [Fact]
    public async Task Handle_Should_Return_Registration_When_Current_User_Is_Its_Owner()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var user = await SeedUserAsync(dbContext);
        var registration = await SeedRegistrationAsync(dbContext, conference.Id, user.Id);
        _currentUser.UserId.Returns(user.Id);
        var handler = new GetRegistrationQueryHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new GetRegistrationQuery(conference.Id, registration.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(registration.Id);
        result.Value.ConferenceId.Should().Be(conference.Id);
        result.Value.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task Handle_Should_Return_Registration_When_Current_User_Is_The_Conference_Owner()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var user = await SeedUserAsync(dbContext);
        var registration = await SeedRegistrationAsync(dbContext, conference.Id, user.Id);
        _currentUser.UserId.Returns(OrganizerId);
        var handler = new GetRegistrationQueryHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new GetRegistrationQuery(conference.Id, registration.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(registration.Id);
    }

    [Fact]
    public async Task Handle_Should_Return_Forbidden_When_Current_User_Is_Someone_Else()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var user = await SeedUserAsync(dbContext);
        var registration = await SeedRegistrationAsync(dbContext, conference.Id, user.Id);
        _currentUser.UserId.Returns(Guid.NewGuid());
        var handler = new GetRegistrationQueryHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new GetRegistrationQuery(conference.Id, registration.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public async Task Handle_Should_Return_Forbidden_When_User_Is_Not_Authenticated()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var user = await SeedUserAsync(dbContext);
        var registration = await SeedRegistrationAsync(dbContext, conference.Id, user.Id);
        _currentUser.UserId.Returns((Guid?)null);
        var handler = new GetRegistrationQueryHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new GetRegistrationQuery(conference.Id, registration.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Registration_Does_Not_Exist()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        _currentUser.UserId.Returns(OrganizerId);
        var handler = new GetRegistrationQueryHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new GetRegistrationQuery(conference.Id, Guid.NewGuid()), TestContext.Current.CancellationToken);

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
        var handler = new GetRegistrationQueryHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new GetRegistrationQuery(other.Id, registration.Id), TestContext.Current.CancellationToken);

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
        var user = await SeedUserAsync(dbContext);
        var registration = await SeedRegistrationAsync(dbContext, conference.Id, user.Id);
        conference.DeletedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        _currentUser.UserId.Returns(user.Id);
        var handler = new GetRegistrationQueryHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new GetRegistrationQuery(conference.Id, registration.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }
}
