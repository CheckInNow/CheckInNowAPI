using Application.Abstractions.Identity;
using Application.Features.Conferences.Delete;
using Domain.Common;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using NSubstitute;

namespace Application.UnitTests.Features.Conferences;

public sealed class DeleteConferenceCommandHandlerTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();

    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    public DeleteConferenceCommandHandlerTests()
    {
        _currentUser.UserId.Returns(OwnerId);
    }

    [Fact]
    public async Task Handle_Should_Soft_Delete_Conference_When_Current_User_Is_Owner()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        var handler = new DeleteConferenceCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new DeleteConferenceCommand(conference.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        dbContext.Conferences.Should().HaveCount(1);
        dbContext.Conferences.Single().DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_Should_Return_Forbidden_When_Current_User_Is_Not_The_Owner()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        _currentUser.UserId.Returns(Guid.NewGuid());
        var handler = new DeleteConferenceCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new DeleteConferenceCommand(conference.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Forbidden);
        dbContext.Conferences.Single().DeletedAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Conference_Does_Not_Exist()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var handler = new DeleteConferenceCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new DeleteConferenceCommand(Guid.NewGuid()), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Conference_Is_Already_Deleted()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var conference = await SeedConferenceAsync(dbContext);
        conference.DeletedAt = DateTimeOffset.UtcNow.AddDays(-1);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var handler = new DeleteConferenceCommandHandler(dbContext, _currentUser);

        // Act
        var result = await handler.Handle(new DeleteConferenceCommand(conference.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
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
