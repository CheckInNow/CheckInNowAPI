using FluentAssertions;
using Infrastructure.Email;

namespace Application.UnitTests.Infrastructure.Email;

public sealed class MailpitEmailSenderTests
{
    [Fact]
    public void CreateMessage_Should_Set_Sender_Receiver_And_Subject()
    {
        // Act
        var message = MailpitEmailSender.CreateMessage("user@example.com", "Subject", "Plain text", "<p>Html</p>");

        // Assert
        message.From.Mailboxes.Should().ContainSingle(m => m.Address == "test@local.dev");
        message.To.Mailboxes.Should().ContainSingle(m => m.Address == "user@example.com");
        message.Subject.Should().Be("Subject");
    }

    [Fact]
    public void CreateMessage_Should_Contain_Text_And_Html_Bodies()
    {
        // Act
        var message = MailpitEmailSender.CreateMessage("user@example.com", "Subject", "Plain text", "<p>Html</p>");

        // Assert
        message.TextBody.Should().Be("Plain text");
        message.HtmlBody.Should().Be("<p>Html</p>");
    }
}
