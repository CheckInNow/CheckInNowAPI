using Application.Abstractions.Email;
using Application.Abstractions.Shared;
using Application.Features.Identity.EmailConfirmation;
using Application.Features.Identity.Register;
using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;

namespace Application.UnitTests.Features.Identity;

public sealed class UserRegisteredHandlerTests
{
    // Contains characters ('+', '/', '=') that are not URL-safe.
    private const string RawToken = "ab+c/d==";

    private readonly UserManager<ApplicationUser> _userManager = IdentityTestHelpers.CreateUserManager();
    private readonly IFrontendLinkBuilder _linkBuilder = Substitute.For<IFrontendLinkBuilder>();
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private readonly UserRegisteredHandler _handler;

    private readonly ApplicationUser _user = new()
    {
        Id = Guid.NewGuid(),
        FirstName = "Jan",
        LastName = "Kowalski",
        Email = "jan@example.com"
    };

    public UserRegisteredHandlerTests()
    {
        var confirmationSender = new EmailConfirmationSender(_userManager, _linkBuilder, _emailSender);
        _handler = new UserRegisteredHandler(_userManager, confirmationSender, NullLogger<UserRegisteredHandler>.Instance);

        _userManager.FindByIdAsync(_user.Id.ToString()).Returns(_user);
        _userManager.GenerateEmailConfirmationTokenAsync(_user).Returns(RawToken);
        _linkBuilder.BuildFrontendLink(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<string>())
            .Returns(ci => $"http://localhost:5173/{ci.ArgAt<string>(0)}?userId={ci.ArgAt<Guid>(1)}&token={ci.ArgAt<string>(2)}");
    }

    [Fact]
    public async Task Handle_Should_Send_Confirmation_Email_With_Link()
    {
        // Act
        await _handler.Handle(new UserRegistered(_user.Id), TestContext.Current.CancellationToken);

        // Assert
        await _emailSender.Received(1).SendEmailAsync(
            _user.Email!,
            Arg.Any<string>(),
            Arg.Is<string>(text => text.Contains("/confirm-email?userId=")),
            Arg.Is<string>(html => html.Contains("/confirm-email?userId=")));
    }

    [Fact]
    public async Task Handle_Should_Build_Link_With_Url_Encoded_Token()
    {
        // Act
        await _handler.Handle(new UserRegistered(_user.Id), TestContext.Current.CancellationToken);

        // Assert
        _linkBuilder.Received(1).BuildFrontendLink(
            EmailConfirmationSender.ConfirmEmailAction,
            _user.Id,
            IdentityTestHelpers.Encode(RawToken));
        _linkBuilder.DidNotReceive().BuildFrontendLink(Arg.Any<string>(), Arg.Any<Guid>(), RawToken);
    }

    [Fact]
    public async Task Handle_Should_Not_Send_Email_When_User_Not_Found()
    {
        // Arrange
        _userManager.FindByIdAsync(Arg.Any<string>()).ReturnsNull();

        // Act
        await _handler.Handle(new UserRegistered(Guid.NewGuid()), TestContext.Current.CancellationToken);

        // Assert
        await _emailSender.DidNotReceive().SendEmailAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_Should_Not_Throw_When_Email_Sending_Fails()
    {
        // Arrange
        _emailSender.SendEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .ThrowsAsync(new InvalidOperationException("SMTP down"));

        // Act
        var act = () => _handler.Handle(new UserRegistered(_user.Id), TestContext.Current.CancellationToken);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
