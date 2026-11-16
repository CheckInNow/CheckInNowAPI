using Application.Abstractions.Identity;
using Application.Features.Registrations.GetAll;
using Domain.Common;
using FluentAssertions;
using NSubstitute;
using static Application.UnitTests.Features.Registrations.RegistrationTestData;

namespace Application.UnitTests.Features.Registrations;

public sealed class GetAllRegistrationsQueryHandlerTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    public GetAllRegistrationsQueryHandlerTests()
    {
        _currentUser.UserId.Returns(OrganizerId);
    }

    [Fact]
    public async Task Handle_Should_Return_Only_Registrations_Of_The_Conference_Ordered_By_Creation_Time()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var other = await SeedConferenceAsync(dbContext);
        var anna = await SeedUserAsync(dbContext, firstName: "Anna", lastName: "Nowak");
        var jan = await SeedUserAsync(dbContext, firstName: "Jan", lastName: "Kowalski");
        var now = DateTimeOffset.UtcNow;
        await SeedRegistrationAsync(dbContext, conference.Id, jan.Id, now);
        await SeedRegistrationAsync(dbContext, conference.Id, anna.Id, now.AddMinutes(-5));
        await SeedRegistrationAsync(dbContext, other.Id, anna.Id);
        var handler = new GetAllRegistrationsQueryHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new GetAllRegistrationsQuery(conference.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalCount.Should().Be(2);
        result.Value.Items.Select(r => r.FirstName).Should().Equal("Anna", "Jan");
        result.Value.Items[0].LastName.Should().Be("Nowak");
        result.Value.Items[0].Email.Should().Be(anna.Email);
        result.Value.Items[0].UserId.Should().Be(anna.Id);
    }

    [Fact]
    public async Task Handle_Should_Return_Requested_Page()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var now = DateTimeOffset.UtcNow;
        for (var i = 0; i < 3; i++)
        {
            var user = await SeedUserAsync(dbContext, firstName: $"User{i}");
            await SeedRegistrationAsync(dbContext, conference.Id, user.Id, now.AddMinutes(i));
        }

        var handler = new GetAllRegistrationsQueryHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new GetAllRegistrationsQuery(conference.Id, Page: 2, PageSize: 2), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Select(r => r.FirstName).Should().Equal("User2");
        result.Value.TotalCount.Should().Be(3);
        result.Value.HasPreviousPage.Should().BeTrue();
        result.Value.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_Page_When_Conference_Has_No_Registrations()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var handler = new GetAllRegistrationsQueryHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new GetAllRegistrationsQuery(conference.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Should_Return_Forbidden_When_Current_User_Is_Not_The_Owner()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        _currentUser.UserId.Returns(Guid.NewGuid());
        var handler = new GetAllRegistrationsQueryHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new GetAllRegistrationsQuery(conference.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Conference_Does_Not_Exist()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var handler = new GetAllRegistrationsQueryHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new GetAllRegistrationsQuery(Guid.NewGuid()), TestContext.Current.CancellationToken);

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
        var handler = new GetAllRegistrationsQueryHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new GetAllRegistrationsQuery(conference.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }
}
