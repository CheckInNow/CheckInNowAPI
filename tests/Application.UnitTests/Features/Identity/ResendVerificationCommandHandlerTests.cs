using Application.Abstractions.Email;
using Application.Abstractions.Shared;
using Application.Features.Identity.EmailConfirmation;
using Application.Features.Identity.ResendVerification;
using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace Application.UnitTests.Features.Identity;

public sealed class ResendVerificationCommandHandlerTests
{
    private readonly UserManager<ApplicationUser> _userManager = IdentityTestHelpers.CreateUserManager();
    private readonly IFrontendLinkBuilder _linkBuilder = Substitute.For<IFrontendLinkBuilder>();
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private readonly ResendVerificationCommandHandler _handler;
    private readonly ApplicationUser _user = new() { Id = Guid.NewGuid(), FirstName = "Jan", Email = "jan@example.com" };

    public ResendVerificationCommandHandlerTests()
    {
        var confirmationSender = new EmailConfirmationSender(_userManager, _linkBuilder, _emailSender);
        _handler = new ResendVerificationCommandHandler(_userManager, confirmationSender);

        _userManager.FindByEmailAsync(_user.Email!).Returns(_user);
        _userManager.GenerateEmailConfirmationTokenAsync(_user).Returns("token");
        _linkBuilder.BuildFrontendLink(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<string>()).Returns("http://link");
    }

    [Fact]
    public async Task Handle_Should_Send_Email_When_User_Is_Not_Confirmed()
    {
        // Act
        var result = await _handler.Handle(new ResendVerificationCommand(_user.Email!), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _emailSender.Received(1).SendEmailAsync(_user.Email!, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_Should_Succeed_Without_Sending_When_User_Does_Not_Exist()
    {
        // Arrange
        _userManager.FindByEmailAsync(Arg.Any<string>()).ReturnsNull();

        // Act
        var result = await _handler.Handle(new ResendVerificationCommand("nobody@example.com"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _emailSender.DidNotReceive().SendEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_Should_Succeed_Without_Sending_When_Email_Already_Confirmed()
    {
        // Arrange
        _user.EmailConfirmed = true;

        // Act
        var result = await _handler.Handle(new ResendVerificationCommand(_user.Email!), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _emailSender.DidNotReceive().SendEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }
}
