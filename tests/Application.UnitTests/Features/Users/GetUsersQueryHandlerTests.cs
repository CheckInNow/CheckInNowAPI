using Application.Features.Users.Get;
using Application.Features.Users.GetAll;
using Domain.Common;
using Domain.Constants;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;

namespace Application.UnitTests.Features.Users;

public sealed class GetUsersQueryHandlerTests
{
    [Fact]
    public async Task GetAll_Should_Return_Paged_Users_With_Roles_And_Lockout_State()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var (userRole, organizerRole) = await SeedRolesAsync(dbContext);
        var older = await SeedUserAsync(dbContext, "Anna", "Nowak", "anna@example.com", DateTimeOffset.UtcNow.AddDays(-2));
        var newer = await SeedUserAsync(dbContext, "Jan", "Kowalski", "jan@example.com", DateTimeOffset.UtcNow.AddDays(-1));
        newer.LockoutEnabled = true;
        newer.LockoutEnd = DateTimeOffset.MaxValue;
        dbContext.UserRoles.AddRange(
            new IdentityUserRole<Guid> { UserId = older.Id, RoleId = userRole.Id },
            new IdentityUserRole<Guid> { UserId = older.Id, RoleId = organizerRole.Id });
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var handler = new GetUsersQueryHandler(dbContext);

        // Act
        var result = await handler.Handle(new GetUsersQuery(Page: 1, PageSize: 1), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalCount.Should().Be(2);
        result.Value.Items.Should().ContainSingle();
        var item = result.Value.Items[0];
        item.Id.Should().Be(newer.Id);
        item.IsLockedOut.Should().BeTrue();
        item.Roles.Should().BeEmpty();

        var secondPage = await handler.Handle(new GetUsersQuery(Page: 2, PageSize: 1), TestContext.Current.CancellationToken);
        secondPage.Value!.Items.Single().Roles.Should().Equal(Roles.Organizer, Roles.User);
        secondPage.Value.Items.Single().IsLockedOut.Should().BeFalse();
    }

    [Theory]
    [InlineData("ANNA@")]
    [InlineData("nowak")]
    [InlineData("anna nowak")]
    public async Task GetAll_Should_Filter_By_Email_Or_Name(string search)
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var anna = await SeedUserAsync(dbContext, "Anna", "Nowak", "anna@example.com", DateTimeOffset.UtcNow);
        await SeedUserAsync(dbContext, "Jan", "Kowalski", "jan@example.com", DateTimeOffset.UtcNow);
        var handler = new GetUsersQueryHandler(dbContext);

        // Act
        var result = await handler.Handle(new GetUsersQuery(Search: search), TestContext.Current.CancellationToken);

        // Assert
        result.Value!.TotalCount.Should().Be(1);
        result.Value.Items.Single().Id.Should().Be(anna.Id);
    }

    [Fact]
    public async Task Get_Should_Return_User_Details_With_Roles()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var (userRole, _) = await SeedRolesAsync(dbContext);
        var user = await SeedUserAsync(dbContext, "Anna", "Nowak", "anna@example.com", DateTimeOffset.UtcNow);
        dbContext.UserRoles.Add(new IdentityUserRole<Guid> { UserId = user.Id, RoleId = userRole.Id });
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var handler = new GetUserQueryHandler(dbContext);

        // Act
        var result = await handler.Handle(new GetUserQuery(user.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Email.Should().Be("anna@example.com");
        result.Value.Roles.Should().Equal(Roles.User);
        result.Value.IsLockedOut.Should().BeFalse();
    }

    [Fact]
    public async Task Get_Should_Return_NotFound_When_User_Does_Not_Exist()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var handler = new GetUserQueryHandler(dbContext);

        // Act
        var result = await handler.Handle(new GetUserQuery(Guid.NewGuid()), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    private static async Task<(IdentityRole<Guid> User, IdentityRole<Guid> Organizer)> SeedRolesAsync(AppDbContext dbContext)
    {
        var user = new IdentityRole<Guid>(Roles.User) { Id = Guid.NewGuid() };
        var organizer = new IdentityRole<Guid>(Roles.Organizer) { Id = Guid.NewGuid() };
        dbContext.Roles.AddRange(user, organizer);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return (user, organizer);
    }

    private static async Task<ApplicationUser> SeedUserAsync(
        AppDbContext dbContext, string firstName, string lastName, string email, DateTimeOffset createdAt)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            UserName = email,
            CreatedAt = createdAt
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return user;
    }
}
