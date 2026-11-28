using Application.Abstractions.Identity;
using Application.Features.CheckIns.Session;
using Domain.Common;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using static Application.UnitTests.Features.CheckIns.CheckInTestData;
using static Application.UnitTests.Features.Registrations.RegistrationTestData;

namespace Application.UnitTests.Features.CheckIns;

public sealed class SessionCheckInCommandHandlerTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    public SessionCheckInCommandHandlerTests()
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
        var handler = new SessionCheckInCommandHandler(dbContext, _currentUser);
        var before = DateTimeOffset.UtcNow;
        var now = DateTimeOffset.UtcNow;
        var session = conference.Sessions.First(s => s.StartTime < now && s.EndTime > now);

        // Act
        var result = await handler.Handle(
            new SessionCheckInCommand(conference.Id, session.Id, registration.TicketCode),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.RegistrationId.Should().Be(registration.Id);
        result.Value!.SessionId.Should().Be(session.Id);
        result.Value.Name.Should().Be("Jan Kowalski");

        var checkIn = await dbContext.SessionCheckIns.SingleAsync(TestContext.Current.CancellationToken);
        checkIn.Id.Should().Be(result.Value.Id);
        checkIn.RegistrationId.Should().Be(registration.Id);
        checkIn.SessionId.Should().Be(result.Value.SessionId);
        checkIn.CheckedInAt.Should().BeOnOrAfter(before);
        result.Value.CheckInTime.Should().Be(checkIn.CheckedInAt);
    }

    [Fact]
    public async Task Handle_Should_Return_Conflict_When_Session_Has_Ended()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedRunningConferenceAsync(dbContext);
        var user = await SeedUserAsync(dbContext, firstName: "Jan", lastName: "Kowalski");
        var registration = await SeedRegistrationAsync(dbContext, conference.Id, user.Id);
        var handler = new SessionCheckInCommandHandler(dbContext, _currentUser);
        var now = DateTimeOffset.UtcNow;
        var session = conference.Sessions.First(s => s.StartTime < now && s.EndTime < now);

        // Act
        var result = await handler.Handle(
            new SessionCheckInCommand(conference.Id, session.Id, registration.TicketCode),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Conflict);
        result.Error!.Code.Should().Be("Session.AlreadyEnded");
        await AssertNothingCheckedInAsync(dbContext);
    }

    [Fact]
    public async Task Handle_Should_Create_Conference_Check_In_For_Session_When_Not_Created()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedRunningConferenceAsync(dbContext);
        var user = await SeedUserAsync(dbContext, firstName: "Jan", lastName: "Kowalski");
        var registration = await SeedRegistrationAsync(dbContext, conference.Id, user.Id);
        var handler = new SessionCheckInCommandHandler(dbContext, _currentUser);
        var now = DateTimeOffset.UtcNow;
        var session = conference.Sessions.First(s => s.StartTime < now && s.EndTime > now);

        // Act
        var result = await handler.Handle(
            new SessionCheckInCommand(conference.Id, session.Id, registration.TicketCode),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.RegistrationId.Should().Be(registration.Id);
        result.Value!.SessionId.Should().Be(session.Id);
        result.Value.Name.Should().Be("Jan Kowalski");

        var checkIn = await dbContext.ConferenceCheckIns.SingleAsync(TestContext.Current.CancellationToken);
        checkIn.RegistrationId.Should().Be(registration.Id);
        checkIn.CheckedInAt.Should().BeOnOrAfter(now);
        result.Value.CheckInTime.Should().Be(checkIn.CheckedInAt);
    }

    [Fact]
    public async Task Handle_Should_Not_Create_Conference_Check_In_For_Session_When_Created()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedRunningConferenceAsync(dbContext);
        var user = await SeedUserAsync(dbContext, firstName: "Jan", lastName: "Kowalski");
        var registration = await SeedRegistrationAsync(dbContext, conference.Id, user.Id);
        var confCheckIn = await SeedCheckInAsync(dbContext, registration.Id);
        var handler = new SessionCheckInCommandHandler(dbContext, _currentUser);
        var now = DateTimeOffset.UtcNow;
        var session = conference.Sessions.First(s => s.StartTime < now && s.EndTime > now);

        // Act
        var result = await handler.Handle(
            new SessionCheckInCommand(conference.Id, session.Id, registration.TicketCode),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.RegistrationId.Should().Be(registration.Id);
        result.Value!.SessionId.Should().Be(session.Id);
        result.Value.Name.Should().Be("Jan Kowalski");

        var conferenceCheckIn = await dbContext.ConferenceCheckIns.SingleAsync(TestContext.Current.CancellationToken);
        conferenceCheckIn.Id.Should().Be(confCheckIn.Id);
        conferenceCheckIn.RegistrationId.Should().Be(registration.Id);
        var sessionCheckIn = await dbContext.SessionCheckIns.SingleAsync(TestContext.Current.CancellationToken);
        sessionCheckIn.CheckedInAt.Should().Be(result.Value.CheckInTime);
        conferenceCheckIn.CheckedInAt.Should().BeBefore(sessionCheckIn.CheckedInAt);
    }

    [Fact]
    public async Task Handle_Should_Return_Conflict_When_Session_Has_Not_Started()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedRunningConferenceAsync(dbContext);
        var user = await SeedUserAsync(dbContext);
        var registration = await SeedRegistrationAsync(dbContext, conference.Id, user.Id);
        var now = DateTimeOffset.UtcNow;
        var upcoming = await SeedExtraSessionAsync(dbContext, conference, now.AddMinutes(10), now.AddMinutes(50));
        var handler = new SessionCheckInCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(
            new SessionCheckInCommand(conference.Id, upcoming.Id, registration.TicketCode),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Conflict);
        result.Error.Code.Should().Be("Session.NotStarted");
        await AssertNothingCheckedInAsync(dbContext);
    }

    [Fact]
    public async Task Handle_Should_Return_Conflict_When_Ticket_Was_Already_Scanned_For_The_Session()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedRunningConferenceAsync(dbContext);
        var user = await SeedUserAsync(dbContext);
        var registration = await SeedRegistrationAsync(dbContext, conference.Id, user.Id);
        var handler = new SessionCheckInCommandHandler(dbContext, _currentUser);
        var now = DateTimeOffset.UtcNow;
        var session = conference.Sessions.First(s => s.StartTime < now && s.EndTime > now);
        var command = new SessionCheckInCommand(conference.Id, session.Id, registration.TicketCode);

        // Act
        var first = await handler.Handle(command, TestContext.Current.CancellationToken);
        var second = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        first.IsSuccess.Should().BeTrue();
        second.IsFailure.Should().BeTrue();
        second.Error!.Type.Should().Be(ErrorType.Conflict);
        second.Error.Code.Should().Be("Ticket.AlreadyScanned");
        (await dbContext.SessionCheckIns.CountAsync(TestContext.Current.CancellationToken)).Should().Be(1);
    }

    [Fact]
    public async Task Handle_Should_Allow_Same_Ticket_On_Different_Sessions_And_Create_Conference_Check_In_Once()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedRunningConferenceAsync(dbContext);
        var user = await SeedUserAsync(dbContext);
        var registration = await SeedRegistrationAsync(dbContext, conference.Id, user.Id);
        var handler = new SessionCheckInCommandHandler(dbContext, _currentUser);
        var now = DateTimeOffset.UtcNow;
        var firstSession = conference.Sessions.First(s => s.StartTime < now && s.EndTime > now);
        var secondSession = await SeedExtraSessionAsync(dbContext, conference, now.AddMinutes(-30), now.AddMinutes(30));

        // Act
        var first = await handler.Handle(
            new SessionCheckInCommand(conference.Id, firstSession.Id, registration.TicketCode),
            TestContext.Current.CancellationToken);
        var second = await handler.Handle(
            new SessionCheckInCommand(conference.Id, secondSession.Id, registration.TicketCode),
            TestContext.Current.CancellationToken);

        // Assert
        first.IsSuccess.Should().BeTrue();
        second.IsSuccess.Should().BeTrue();

        var sessionIds = await dbContext.SessionCheckIns
            .Select(c => c.SessionId)
            .ToListAsync(TestContext.Current.CancellationToken);
        sessionIds.Should().BeEquivalentTo(new[] { firstSession.Id, secondSession.Id });
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
        var handler = new SessionCheckInCommandHandler(dbContext, _currentUser);
        var now = DateTimeOffset.UtcNow;
        var session = conference.Sessions.First(s => s.StartTime < now && s.EndTime > now);

        // Act
        var result = await handler.Handle(
            new SessionCheckInCommand(conference.Id, session.Id, registration.TicketCode),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Forbidden);
        result.Error.Code.Should().Be("CheckIn.Forbidden");
        await AssertNothingCheckedInAsync(dbContext);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Ticket_Code_Does_Not_Exist()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedRunningConferenceAsync(dbContext);
        var handler = new SessionCheckInCommandHandler(dbContext, _currentUser);
        var now = DateTimeOffset.UtcNow;
        var session = conference.Sessions.First(s => s.StartTime < now && s.EndTime > now);

        // Act
        var result = await handler.Handle(
            new SessionCheckInCommand(conference.Id, session.Id, Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("Ticket.Invalid");
        await AssertNothingCheckedInAsync(dbContext);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Session_Does_Not_Exist()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedRunningConferenceAsync(dbContext);
        var user = await SeedUserAsync(dbContext);
        var registration = await SeedRegistrationAsync(dbContext, conference.Id, user.Id);
        var handler = new SessionCheckInCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(
            new SessionCheckInCommand(conference.Id, Guid.NewGuid(), registration.TicketCode),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        await AssertNothingCheckedInAsync(dbContext);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Session_Belongs_To_Another_Conference()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedRunningConferenceAsync(dbContext);
        var otherConference = await SeedRunningConferenceAsync(dbContext);
        var user = await SeedUserAsync(dbContext);
        var registration = await SeedRegistrationAsync(dbContext, conference.Id, user.Id);
        var handler = new SessionCheckInCommandHandler(dbContext, _currentUser);
        var otherSession = otherConference.Sessions.First();

        // Act
        var result = await handler.Handle(
            new SessionCheckInCommand(conference.Id, otherSession.Id, registration.TicketCode),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        await AssertNothingCheckedInAsync(dbContext);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Session_Is_Deleted()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedRunningConferenceAsync(dbContext);
        var user = await SeedUserAsync(dbContext);
        var registration = await SeedRegistrationAsync(dbContext, conference.Id, user.Id);
        var handler = new SessionCheckInCommandHandler(dbContext, _currentUser);
        var now = DateTimeOffset.UtcNow;
        var session = conference.Sessions.First(s => s.StartTime < now && s.EndTime > now);
        session.DeletedAt = now;
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await handler.Handle(
            new SessionCheckInCommand(conference.Id, session.Id, registration.TicketCode),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        await AssertNothingCheckedInAsync(dbContext);
    }

    private static async Task AssertNothingCheckedInAsync(AppDbContext dbContext)
    {
        (await dbContext.SessionCheckIns.AnyAsync(TestContext.Current.CancellationToken)).Should().BeFalse();
        (await dbContext.ConferenceCheckIns.AnyAsync(TestContext.Current.CancellationToken)).Should().BeFalse();
    }
}
