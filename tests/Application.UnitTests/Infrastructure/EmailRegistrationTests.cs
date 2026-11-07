using Application.Abstractions.Email;
using Application.Abstractions.OAuth;
using Application.Abstractions.Shared;
using FluentAssertions;
using Infrastructure;
using Infrastructure.Email;
using Infrastructure.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Application.UnitTests.Infrastructure;

public sealed class EmailRegistrationTests
{
    private static ServiceProvider BuildProvider(string? emailProvider)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["EMAIL_PROVIDER"] = emailProvider,
                ["Jwt:Secret"] = "unit-test-secret-that-is-long-enough-123"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddInfrastructure(configuration);
        
        services.RemoveAll<IGoogleTokenVerifier>();

        return services.BuildServiceProvider();
    }

    [Theory]
    [InlineData("mailgun")]
    [InlineData("MAILGUN")]
    public void AddInfrastructure_Should_Register_Mailgun_When_Provider_Is_Mailgun(string provider)
    {
        using var serviceProvider = BuildProvider(provider);
        using var scope = serviceProvider.CreateScope();

        scope.ServiceProvider.GetRequiredService<IEmailSender>().Should().BeOfType<MailgunEmailSender>();
    }

    [Theory]
    [InlineData("mailpit")]
    [InlineData("MAILPIT")]
    [InlineData(null)]
    public void AddInfrastructure_Should_Register_Mailpit_Otherwise(string? provider)
    {
        using var serviceProvider = BuildProvider(provider);
        using var scope = serviceProvider.CreateScope();

        scope.ServiceProvider.GetRequiredService<IEmailSender>().Should().BeOfType<MailpitEmailSender>();
    }

    [Fact]
    public void AddInfrastructure_Should_Register_FrontendLinkBuilder()
    {
        using var provider = BuildProvider("mailpit");
        using var scope = provider.CreateScope();

        scope.ServiceProvider.GetRequiredService<IFrontendLinkBuilder>().Should().BeOfType<FrontendLinkBuilder>();
    }
}
