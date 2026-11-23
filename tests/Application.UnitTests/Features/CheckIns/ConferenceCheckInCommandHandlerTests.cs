using Application.Abstractions.Identity;
using Application.Features.CheckIns.Conference;
using Domain.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using static Application.UnitTests.Features.CheckIns.CheckInTestData;
using static Application.UnitTests.Features.Registrations.RegistrationTestData;

namespace Application.UnitTests.Features.CheckIns;

public sealed class ConferenceCheckInCommandHandlerTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    public ConferenceCheckInCommandHandlerTests()
    {
        _currentUser.UserId.Returns(OrganizerId);
    }

    [Fact]
    public async Task Handle_Should_Check_In_Participant_When_Organizer_Scans_Valid_Ticket()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedRunningConferenceAsync(dbContext);
        var user = await SeedUserAsync(dbContext, firstName: "Jan", lastName: "Kowalski");
        var registration = await SeedRegistrationAsync(dbContext, conference.Id, user.Id);
        var handler = new ConferenceCheckInCommandHandler(dbContext, _currentUser);
        var before = DateTimeOffset.UtcNow;

        // Act
        var result = await handler.Handle(
            new ConferenceCheckInCommand(conference.Id, registration.TicketCode),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.RegistrationId.Should().Be(registration.Id);
        result.Value.Name.Should().Be("Jan Kowalski");

        var checkIn = await dbContext.ConferenceCheckIns.SingleAsync(TestContext.Current.CancellationToken);
        checkIn.Id.Should().Be(result.Value.Id);
        checkIn.RegistrationId.Should().Be(registration.Id);
        checkIn.CheckedInAt.Should().BeOnOrAfter(before);
        result.Value.CheckInTime.Should().Be(checkIn.CheckedInAt);
    }

    [Fact]
    public async Task Handle_Should_Check_In_Only_The_Scanned_Registration()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedRunningConferenceAsync(dbContext);
        var anna = await SeedUserAsync(dbContext);
        var jan = await SeedUserAsync(dbContext);
        var annaRegistration = await SeedRegistrationAsync(dbContext, conference.Id, anna.Id);
        var janRegistration = await SeedRegistrationAsync(dbContext, conference.Id, jan.Id);
        var handler = new ConferenceCheckInCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(
            new ConferenceCheckInCommand(conference.Id, janRegistration.TicketCode),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var checkIn = await dbContext.ConferenceCheckIns.SingleAsync(TestContext.Current.CancellationToken);
        checkIn.RegistrationId.Should().Be(janRegistration.Id);
        checkIn.RegistrationId.Should().NotBe(annaRegistration.Id);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Ticket_Code_Does_Not_Exist()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedRunningConferenceAsync(dbContext);
        var handler = new ConferenceCheckInCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(
            new ConferenceCheckInCommand(conference.Id, Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("Ticket.Invalid");
        (await dbContext.ConferenceCheckIns.AnyAsync(TestContext.Current.CancellationToken)).Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Ticket_Belongs_To_Another_Conference()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var scannedConference = await SeedRunningConferenceAsync(dbContext);
        var otherConference = await SeedRunningConferenceAsync(dbContext);
        var user = await SeedUserAsync(dbContext);
        var registration = await SeedRegistrationAsync(dbContext, otherConference.Id, user.Id);
        var handler = new ConferenceCheckInCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(
            new ConferenceCheckInCommand(scannedConference.Id, registration.TicketCode),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("Ticket.Invalid");
        (await dbContext.ConferenceCheckIns.AnyAsync(TestContext.Current.CancellationToken)).Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Conference_Is_Deleted()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedRunningConferenceAsync(dbContext);
        var user = await SeedUserAsync(dbContext);
        var registration = await SeedRegistrationAsync(dbContext, conference.Id, user.Id);
        conference.DeletedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var handler = new ConferenceCheckInCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(
            new ConferenceCheckInCommand(conference.Id, registration.TicketCode),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        (await dbContext.ConferenceCheckIns.AnyAsync(TestContext.Current.CancellationToken)).Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_Return_Conflict_When_Ticket_Was_Already_Scanned()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedRunningConferenceAsync(dbContext);
        var user = await SeedUserAsync(dbContext);
        var registration = await SeedRegistrationAsync(dbContext, conference.Id, user.Id);
        await SeedCheckInAsync(dbContext, registration.Id);
        var handler = new ConferenceCheckInCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(
            new ConferenceCheckInCommand(conference.Id, registration.TicketCode),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Conflict);
        result.Error.Code.Should().Be("Ticket.AlreadyScanned");
        (await dbContext.ConferenceCheckIns.CountAsync(TestContext.Current.CancellationToken)).Should().Be(1);
    }

    [Fact]
    public async Task Handle_Should_Return_Conflict_When_Same_Ticket_Is_Scanned_Twice()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedRunningConferenceAsync(dbContext);
        var user = await SeedUserAsync(dbContext);
        var registration = await SeedRegistrationAsync(dbContext, conference.Id, user.Id);
        var handler = new ConferenceCheckInCommandHandler(dbContext, _currentUser);
        var command = new ConferenceCheckInCommand(conference.Id, registration.TicketCode);

        // Act
        var first = await handler.Handle(command, TestContext.Current.CancellationToken);
        var second = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        first.IsSuccess.Should().BeTrue();
        second.IsFailure.Should().BeTrue();
        second.Error!.Type.Should().Be(ErrorType.Conflict);
        second.Error.Code.Should().Be("Ticket.AlreadyScanned");
        (await dbContext.ConferenceCheckIns.CountAsync(TestContext.Current.CancellationToken)).Should().Be(1);
    }

    [Fact]
    public async Task Handle_Should_Return_Forbidden_When_Current_User_Is_Not_The_Organizer()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedRunningConferenceAsync(dbContext);
        var user = await SeedUserAsync(dbContext);
        var registration = await SeedRegistrationAsync(dbContext, conference.Id, user.Id);
        _currentUser.UserId.Returns(Guid.NewGuid());
        var handler = new ConferenceCheckInCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(
            new ConferenceCheckInCommand(conference.Id, registration.TicketCode),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Forbidden);
        result.Error.Code.Should().Be("CheckIn.Forbidden");
        (await dbContext.ConferenceCheckIns.AnyAsync(TestContext.Current.CancellationToken)).Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_Return_Forbidden_When_User_Is_Not_Authenticated()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedRunningConferenceAsync(dbContext);
        var user = await SeedUserAsync(dbContext);
        var registration = await SeedRegistrationAsync(dbContext, conference.Id, user.Id);
        _currentUser.UserId.Returns((Guid?)null);
        var handler = new ConferenceCheckInCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(
            new ConferenceCheckInCommand(conference.Id, registration.TicketCode),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Forbidden);
        (await dbContext.ConferenceCheckIns.AnyAsync(TestContext.Current.CancellationToken)).Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_Return_Conflict_When_Conference_Has_Not_Started()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var now = DateTimeOffset.UtcNow;
        var conference = await SeedConferenceWithDatesAsync(dbContext, now.AddHours(1), now.AddHours(3));
        var user = await SeedUserAsync(dbContext);
        var registration = await SeedRegistrationAsync(dbContext, conference.Id, user.Id);
        var handler = new ConferenceCheckInCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(
            new ConferenceCheckInCommand(conference.Id, registration.TicketCode),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Conflict);
        result.Error.Code.Should().Be("Conference.NotStarted");
        (await dbContext.ConferenceCheckIns.AnyAsync(TestContext.Current.CancellationToken)).Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_Return_Conflict_When_Conference_Has_Ended()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var now = DateTimeOffset.UtcNow;
        var conference = await SeedConferenceWithDatesAsync(dbContext, now.AddHours(-3), now.AddHours(-1));
        var user = await SeedUserAsync(dbContext);
        var registration = await SeedRegistrationAsync(dbContext, conference.Id, user.Id);
        var handler = new ConferenceCheckInCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(
            new ConferenceCheckInCommand(conference.Id, registration.TicketCode),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Conflict);
        result.Error.Code.Should().Be("Conference.AlreadyEnded");
        (await dbContext.ConferenceCheckIns.AnyAsync(TestContext.Current.CancellationToken)).Should().BeFalse();
    }
}
