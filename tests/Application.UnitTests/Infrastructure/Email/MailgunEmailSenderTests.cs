using System.Net;
using System.Text;
using FluentAssertions;
using Infrastructure.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Application.UnitTests.Infrastructure.Email;

public sealed class MailgunEmailSenderTests
{
    private static readonly Dictionary<string, string?> ValidSettings = new()
    {
        ["MAILGUN_API_KEY"] = "test-api-key",
        ["MAILGUN_DOMAIN"] = "mg.checkinnow.test",
        ["MAILGUN_FROM"] = "CheckInNow <postmaster@mg.checkinnow.test>"
    };

    private static MailgunEmailSender CreateSender(
        FakeHttpMessageHandler handler,
        Dictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        return new MailgunEmailSender(new HttpClient(handler), configuration, NullLogger<MailgunEmailSender>.Instance);
    }

    [Fact]
    public async Task SendEmailAsync_Should_Post_Message_To_Mailgun_Api()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK);
        var sender = CreateSender(handler, ValidSettings);

        // Act
        await sender.SendEmailAsync("user@example.com", "Subject", "Plain text", "<p>Html</p>");

        // Assert
        handler.Request.Should().NotBeNull();
        handler.Request!.Method.Should().Be(HttpMethod.Post);
        handler.Request.RequestUri.Should().Be(new Uri("https://api.eu.mailgun.net/v3/mg.checkinnow.test/messages"));
    }

    [Fact]
    public async Task SendEmailAsync_Should_Use_Basic_Auth_With_Api_Key()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK);
        var sender = CreateSender(handler, ValidSettings);

        // Act
        await sender.SendEmailAsync("user@example.com", "Subject", "Plain text", "<p>Html</p>");

        // Assert
        var expected = Convert.ToBase64String(Encoding.UTF8.GetBytes("api:test-api-key"));
        handler.Request!.Headers.Authorization!.Scheme.Should().Be("Basic");
        handler.Request.Headers.Authorization.Parameter.Should().Be(expected);
    }

    [Fact]
    public async Task SendEmailAsync_Should_Send_All_Message_Fields_As_Form_Data()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK);
        var sender = CreateSender(handler, ValidSettings);

        // Act
        await sender.SendEmailAsync("user@example.com", "Subject", "Plain text", "<p>Html</p>");

        // Assert
        handler.Body.Should()
            .Contain("CheckInNow <postmaster@mg.checkinnow.test>")
            .And.Contain("user@example.com")
            .And.Contain("Subject")
            .And.Contain("Plain text")
            .And.Contain("<p>Html</p>");
    }

    [Fact]
    public async Task SendEmailAsync_Should_Use_Configured_Base_Url()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK);
        var settings = new Dictionary<string, string?>(ValidSettings)
        {
            ["MAILGUN_BASE_URL"] = "https://api.mailgun.net"
        };
        var sender = CreateSender(handler, settings);

        // Act
        await sender.SendEmailAsync("user@example.com", "Subject", "Plain text", "<p>Html</p>");

        // Assert
        handler.Request!.RequestUri!.Host.Should().Be("api.mailgun.net");
    }

    [Fact]
    public async Task SendEmailAsync_Should_Throw_When_Mailgun_Returns_Error()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(HttpStatusCode.Unauthorized);
        var sender = CreateSender(handler, ValidSettings);

        // Act
        var act = () => sender.SendEmailAsync("user@example.com", "Subject", "Plain text", "<p>Html</p>");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Mailgun error.");
    }

    [Theory]
    [InlineData("MAILGUN_API_KEY")]
    [InlineData("MAILGUN_DOMAIN")]
    [InlineData("MAILGUN_FROM")]
    public async Task SendEmailAsync_Should_Throw_And_Not_Send_When_Setting_Is_Missing(string missingKey)
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK);
        var settings = new Dictionary<string, string?>(ValidSettings);
        settings.Remove(missingKey);
        var sender = CreateSender(handler, settings);

        // Act
        var act = () => sender.SendEmailAsync("user@example.com", "Subject", "Plain text", "<p>Html</p>");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        handler.Request.Should().BeNull();
    }

    private sealed class FakeHttpMessageHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }
        public string Body { get; private set; } = string.Empty;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            if (request.Content is not null)
            {
                Body = await request.Content.ReadAsStringAsync(cancellationToken);
            }

            return new HttpResponseMessage(statusCode) { Content = new StringContent("{}") };
        }
    }
}
