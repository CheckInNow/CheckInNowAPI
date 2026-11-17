using Application.Abstractions.Identity;
using Application.Features.Registrations.GetTicket;
using Domain.Common;
using FluentAssertions;
using NSubstitute;
using static Application.UnitTests.Features.Registrations.RegistrationTestData;

namespace Application.UnitTests.Features.Registrations;

public sealed class GetTicketQueryHandlerTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    [Fact]
    public async Task Handle_Should_Return_Ticket_When_Current_User_Is_The_Owner()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var user = await SeedUserAsync(dbContext);
        var registration = await SeedRegistrationAsync(dbContext, conference.Id, user.Id);
        _currentUser.UserId.Returns(user.Id);
        var handler = new GetTicketQueryHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new GetTicketQuery(conference.Id, registration.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.TicketCode.Should().Be(registration.TicketCode);
        result.Value.ConferenceId.Should().Be(conference.Id);
        result.Value.ConferenceName.Should().Be(conference.Name);
    }

    [Fact]
    public async Task Handle_Should_Return_Own_Ticket_When_Other_Registrations_Exist()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var anna = await SeedUserAsync(dbContext);
        var jan = await SeedUserAsync(dbContext);
        var annaRegistration = await SeedRegistrationAsync(dbContext, conference.Id, anna.Id);
        var janRegistration = await SeedRegistrationAsync(dbContext, conference.Id, jan.Id);
        _currentUser.UserId.Returns(jan.Id);
        var handler = new GetTicketQueryHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new GetTicketQuery(conference.Id, janRegistration.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.TicketCode.Should().Be(janRegistration.TicketCode);
        result.Value.TicketCode.Should().NotBe(annaRegistration.TicketCode);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Current_User_Is_Not_The_Owner()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var user = await SeedUserAsync(dbContext);
        var registration = await SeedRegistrationAsync(dbContext, conference.Id, user.Id);
        _currentUser.UserId.Returns(Guid.NewGuid());
        var handler = new GetTicketQueryHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new GetTicketQuery(conference.Id, registration.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Current_User_Is_The_Conference_Owner()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var user = await SeedUserAsync(dbContext);
        var registration = await SeedRegistrationAsync(dbContext, conference.Id, user.Id);
        _currentUser.UserId.Returns(OrganizerId);
        var handler = new GetTicketQueryHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new GetTicketQuery(conference.Id, registration.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_User_Is_Not_Authenticated()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var user = await SeedUserAsync(dbContext);
        var registration = await SeedRegistrationAsync(dbContext, conference.Id, user.Id);
        _currentUser.UserId.Returns((Guid?)null);
        var handler = new GetTicketQueryHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new GetTicketQuery(conference.Id, registration.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Registration_Does_Not_Exist()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var user = await SeedUserAsync(dbContext);
        _currentUser.UserId.Returns(user.Id);
        var handler = new GetTicketQueryHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new GetTicketQuery(conference.Id, Guid.NewGuid()), TestContext.Current.CancellationToken);

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
        var handler = new GetTicketQueryHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new GetTicketQuery(other.Id, registration.Id), TestContext.Current.CancellationToken);

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
        var handler = new GetTicketQueryHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new GetTicketQuery(conference.Id, registration.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }
}
