using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundServices;

public sealed class UnconfirmedUserCleanupService(
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider,
    ILogger<UnconfirmedUserCleanupService> logger) : BackgroundService
{
    public static readonly TimeSpan UnconfirmedAccountLifetime = TimeSpan.FromDays(7);
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var deleted = await DeleteExpiredUnconfirmedUsersAsync(context, timeProvider.GetUtcNow(), stoppingToken);
                logger.LogInformation("Deleted {Count} unconfirmed user accounts.", deleted);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during unconfirmed user cleanup");
            }

            await Task.Delay(Interval, timeProvider, stoppingToken);
        }
    }

    public static async Task<int> DeleteExpiredUnconfirmedUsersAsync(
        AppDbContext context,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        var threshold = now - UnconfirmedAccountLifetime;

        var expiredUsers = await context.Users
            .Where(u => !u.EmailConfirmed && u.CreatedAt < threshold)
            .ToListAsync(cancellationToken);

        if (expiredUsers.Count == 0)
        {
            return 0;
        }

        context.Users.RemoveRange(expiredUsers);
        await context.SaveChangesAsync(cancellationToken);
        return expiredUsers.Count;
    }
}
