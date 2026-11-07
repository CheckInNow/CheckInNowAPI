using FluentAssertions;
using Infrastructure.Shared;
using Microsoft.Extensions.Configuration;

namespace Application.UnitTests.Infrastructure.Shared;

public sealed class FrontendLinkBuilderTests
{
    private static FrontendLinkBuilder CreateBuilder(string? frontendUrl)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Frontend:Url"] = frontendUrl })
            .Build();

        return new FrontendLinkBuilder(configuration);
    }

    [Fact]
    public void BuildFrontendLink_Should_Return_Link_With_UserId_And_Token()
    {
        // Arrange
        var builder = CreateBuilder("https://checkinnow.test");
        var userId = Guid.NewGuid();

        // Act
        var link = builder.BuildFrontendLink("confirm-email", userId, "abc123");

        // Assert
        link.Should().Be($"https://checkinnow.test/confirm-email?userId={userId}&token=abc123");
    }

    [Fact]
    public void BuildFrontendLink_Should_Trim_Trailing_Slash_From_Frontend_Url()
    {
        // Arrange
        var builder = CreateBuilder("https://checkinnow.test/");
        var userId = Guid.NewGuid();

        // Act
        var link = builder.BuildFrontendLink("reset-password", userId, "token");

        // Assert
        link.Should().StartWith("https://checkinnow.test/reset-password?");
    }

    [Fact]
    public void BuildFrontendLink_Should_Throw_When_Frontend_Url_Is_Not_Configured()
    {
        // Arrange
        var builder = CreateBuilder(null);

        // Act
        var act = () => builder.BuildFrontendLink("confirm-email", Guid.NewGuid(), "token");

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }
}
