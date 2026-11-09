using Domain.Entities;
using FluentAssertions;
using Infrastructure;
using Infrastructure.BackgroundServices;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Application.UnitTests.Infrastructure.Identity;

public sealed class EmailConfirmationTokenProviderTests
{
    private static ServiceProvider BuildProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "unit-test-secret-that-is-long-enough-123"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        var databaseName = Guid.NewGuid().ToString();
        services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(databaseName));
        services.AddInfrastructure(configuration);

        return services.BuildServiceProvider();
    }

    [Fact]
    public void EmailConfirmation_Token_Should_Expire_After_24_Hours()
    {
        using var provider = BuildProvider();

        provider.GetRequiredService<IOptions<EmailConfirmationTokenProviderOptions>>().Value.TokenLifespan
            .Should().Be(TimeSpan.FromHours(24));
    }

    [Fact]
    public void AddInfrastructure_Should_Use_Dedicated_Email_Confirmation_Token_Provider()
    {
        using var provider = BuildProvider();

        provider.GetRequiredService<IOptions<IdentityOptions>>().Value.Tokens.EmailConfirmationTokenProvider
            .Should().Be(EmailConfirmationTokenProviderOptions.ProviderName);
    }

    [Fact]
    public void AddInfrastructure_Should_Register_Unconfirmed_User_Cleanup_Service()
    {
        using var provider = BuildProvider();

        provider.GetServices<IHostedService>().Should().ContainSingle(s => s is UnconfirmedUserCleanupService);
    }

    [Fact]
    public async Task Generated_Token_Should_Confirm_Email()
    {
        // Arrange
        await using var provider = BuildProvider();
        await using var scope = provider.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@example.com",
            UserName = "jan@example.com"
        };
        (await userManager.CreateAsync(user)).Succeeded.Should().BeTrue();

        // Act
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var result = await userManager.ConfirmEmailAsync(user, token);

        // Assert
        result.Succeeded.Should().BeTrue();
        user.EmailConfirmed.Should().BeTrue();
    }
}
