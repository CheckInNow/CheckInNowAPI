using Application.Abstractions.Identity;
using Application.Features.Registrations.GetMy;
using Domain.Common;
using FluentAssertions;
using NSubstitute;
using static Application.UnitTests.Features.Registrations.RegistrationTestData;

namespace Application.UnitTests.Features.Registrations;

public sealed class GetMyRegistrationsQueryHandlerTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    [Fact]
    public async Task Handle_Should_Return_Only_Registrations_Of_The_Current_User()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var first = await SeedConferenceAsync(dbContext, startsInDays: 10);
        var second = await SeedConferenceAsync(dbContext, startsInDays: 20);
        var notRegistered = await SeedConferenceAsync(dbContext, startsInDays: 30);
        var user = await SeedUserAsync(dbContext);
        var otherUser = await SeedUserAsync(dbContext);
        var firstRegistration = await SeedRegistrationAsync(dbContext, first.Id, user.Id);
        var secondRegistration = await SeedRegistrationAsync(dbContext, second.Id, user.Id);
        await SeedRegistrationAsync(dbContext, notRegistered.Id, otherUser.Id);
        _currentUser.UserId.Returns(user.Id);
        var handler = new GetMyRegistrationsQueryHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new GetMyRegistrationsQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalCount.Should().Be(2);
        result.Value.Items.Select(r => r.Id).Should().BeEquivalentTo([firstRegistration.Id, secondRegistration.Id]);
    }

    [Fact]
    public async Task Handle_Should_Not_Return_Registrations_Of_Another_User()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var first = await SeedConferenceAsync(dbContext);
        var second = await SeedConferenceAsync(dbContext);
        var user = await SeedUserAsync(dbContext);
        var otherUser = await SeedUserAsync(dbContext);
        await SeedRegistrationAsync(dbContext, first.Id, user.Id);
        await SeedRegistrationAsync(dbContext, second.Id, user.Id);
        _currentUser.UserId.Returns(otherUser.Id);
        var handler = new GetMyRegistrationsQueryHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new GetMyRegistrationsQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Should_Map_Conference_Details()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var user = await SeedUserAsync(dbContext);
        var registration = await SeedRegistrationAsync(dbContext, conference.Id, user.Id);
        _currentUser.UserId.Returns(user.Id);
        var handler = new GetMyRegistrationsQueryHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new GetMyRegistrationsQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().ContainSingle().Which.Should().Be(new MyRegistrationListItemResponse(
            registration.Id,
            conference.Id,
            conference.Name,
            conference.StartDate,
            conference.EndDate,
            conference.Location,
            registration.CreatedAt));
    }

    [Fact]
    public async Task Handle_Should_Not_Return_Registrations_Of_Deleted_Conferences()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var active = await SeedConferenceAsync(dbContext);
        var deleted = await SeedConferenceAsync(dbContext);
        var user = await SeedUserAsync(dbContext);
        var activeRegistration = await SeedRegistrationAsync(dbContext, active.Id, user.Id);
        await SeedRegistrationAsync(dbContext, deleted.Id, user.Id);
        deleted.DeletedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        _currentUser.UserId.Returns(user.Id);
        var handler = new GetMyRegistrationsQueryHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new GetMyRegistrationsQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalCount.Should().Be(1);
        result.Value.Items.Should().ContainSingle().Which.Id.Should().Be(activeRegistration.Id);
    }

    [Fact]
    public async Task Handle_Should_Order_By_Conference_Start_Date_Descending()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var past = await SeedConferenceAsync(dbContext, startsInDays: -10);
        var later = await SeedConferenceAsync(dbContext, startsInDays: 40);
        var sooner = await SeedConferenceAsync(dbContext, startsInDays: 5);
        var user = await SeedUserAsync(dbContext);
        await SeedRegistrationAsync(dbContext, past.Id, user.Id);
        await SeedRegistrationAsync(dbContext, later.Id, user.Id);
        await SeedRegistrationAsync(dbContext, sooner.Id, user.Id);
        _currentUser.UserId.Returns(user.Id);
        var handler = new GetMyRegistrationsQueryHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new GetMyRegistrationsQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Select(r => r.ConferenceId).Should().Equal(later.Id, sooner.Id, past.Id);
    }

    [Fact]
    public async Task Handle_Should_Order_By_Id_When_Start_Dates_Are_Equal()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var first = await SeedConferenceAsync(dbContext);
        var second = await SeedConferenceAsync(dbContext);
        second.StartDate = first.StartDate;
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var user = await SeedUserAsync(dbContext);
        var firstRegistration = await SeedRegistrationAsync(dbContext, first.Id, user.Id);
        var secondRegistration = await SeedRegistrationAsync(dbContext, second.Id, user.Id);
        _currentUser.UserId.Returns(user.Id);
        var handler = new GetMyRegistrationsQueryHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new GetMyRegistrationsQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Select(r => r.Id).Should()
            .Equal(new[] { firstRegistration.Id, secondRegistration.Id }.Order());
    }

    [Fact]
    public async Task Handle_Should_Return_Requested_Page()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var user = await SeedUserAsync(dbContext);
        var conferences = new List<Guid>();
        for (var i = 0; i < 3; i++)
        {
            var conference = await SeedConferenceAsync(dbContext, startsInDays: 10 * (i + 1));
            await SeedRegistrationAsync(dbContext, conference.Id, user.Id);
            conferences.Add(conference.Id);
        }

        _currentUser.UserId.Returns(user.Id);
        var handler = new GetMyRegistrationsQueryHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new GetMyRegistrationsQuery(Page: 2, PageSize: 2), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Select(r => r.ConferenceId).Should().Equal(conferences[0]);
        result.Value.TotalCount.Should().Be(3);
        result.Value.HasPreviousPage.Should().BeTrue();
        result.Value.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_Return_Forbidden_When_Current_User_Is_Unknown()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        _currentUser.UserId.Returns((Guid?)null);
        var handler = new GetMyRegistrationsQueryHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new GetMyRegistrationsQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Forbidden);
    }
}
