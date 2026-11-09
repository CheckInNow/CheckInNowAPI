using Domain.Entities;
using FluentAssertions;
using Infrastructure.BackgroundServices;
using Microsoft.EntityFrameworkCore;

namespace Application.UnitTests.Infrastructure.BackgroundServices;

public sealed class UnconfirmedUserCleanupServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 9, 12, 0, 0, TimeSpan.Zero);

    private static ApplicationUser User(string email, bool confirmed, TimeSpan age) => new()
    {
        Id = Guid.NewGuid(),
        FirstName = "Test",
        LastName = "User",
        Email = email,
        UserName = email,
        EmailConfirmed = confirmed,
        CreatedAt = Now - age
    };

    [Fact]
    public async Task DeleteExpiredUnconfirmedUsersAsync_Should_Delete_Only_Unconfirmed_Users_Older_Than_7_Days()
    {
        // Arrange
        await using var context = TestDbContextFactory.Create();
        context.Users.AddRange(
            User("old-unconfirmed@example.com", confirmed: false, TimeSpan.FromDays(8)),
            User("new-unconfirmed@example.com", confirmed: false, TimeSpan.FromDays(6)),
            User("old-confirmed@example.com", confirmed: true, TimeSpan.FromDays(30)));
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var deleted = await UnconfirmedUserCleanupService.DeleteExpiredUnconfirmedUsersAsync(
            context, Now, TestContext.Current.CancellationToken);

        // Assert
        deleted.Should().Be(1);
        var remaining = await context.Users.Select(u => u.Email).ToListAsync(TestContext.Current.CancellationToken);
        remaining.Should().BeEquivalentTo("new-unconfirmed@example.com", "old-confirmed@example.com");
    }

    [Fact]
    public async Task DeleteExpiredUnconfirmedUsersAsync_Should_Return_Zero_When_Nothing_To_Delete()
    {
        // Arrange
        await using var context = TestDbContextFactory.Create();
        context.Users.Add(User("fresh@example.com", confirmed: false, TimeSpan.FromHours(1)));
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var deleted = await UnconfirmedUserCleanupService.DeleteExpiredUnconfirmedUsersAsync(
            context, Now, TestContext.Current.CancellationToken);

        // Assert
        deleted.Should().Be(0);
        (await context.Users.CountAsync(TestContext.Current.CancellationToken)).Should().Be(1);
    }
}
