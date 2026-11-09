using Application.Features.Identity.ConfirmEmail;
using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace Application.UnitTests.Features.Identity;

public sealed class ConfirmEmailCommandHandlerTests
{
    private const string RawToken = "ab+c/d==";

    private readonly UserManager<ApplicationUser> _userManager = IdentityTestHelpers.CreateUserManager();
    private readonly ConfirmEmailCommandHandler _handler;
    private readonly ApplicationUser _user = new() { Id = Guid.NewGuid(), Email = "jan@example.com" };

    public ConfirmEmailCommandHandlerTests()
    {
        _handler = new ConfirmEmailCommandHandler(_userManager);

        _userManager.FindByIdAsync(_user.Id.ToString()).Returns(_user);
        _userManager.ConfirmEmailAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);
    }

    [Fact]
    public async Task Handle_Should_Decode_Token_And_Confirm_Email()
    {
        // Arrange
        var command = new ConfirmEmailCommand(_user.Id, IdentityTestHelpers.Encode(RawToken));

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _userManager.Received(1).ConfirmEmailAsync(_user, RawToken);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_User_Not_Found()
    {
        // Arrange
        _userManager.FindByIdAsync(Arg.Any<string>()).ReturnsNull();

        // Act
        var result = await _handler.Handle(
            new ConfirmEmailCommand(Guid.NewGuid(), IdentityTestHelpers.Encode(RawToken)),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Auth.InvalidConfirmationToken");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Token_Is_Not_Valid_Base64Url()
    {
        // Act
        var result = await _handler.Handle(
            new ConfirmEmailCommand(_user.Id, "not*valid*base64"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Auth.InvalidConfirmationToken");
        await _userManager.DidNotReceive().ConfirmEmailAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Token_Is_Invalid_Or_Expired()
    {
        // Arrange
        _userManager.ConfirmEmailAsync(_user, Arg.Any<string>())
            .Returns(IdentityResult.Failed(new IdentityError { Code = "InvalidToken", Description = "Invalid token." }));

        // Act
        var result = await _handler.Handle(
            new ConfirmEmailCommand(_user.Id, IdentityTestHelpers.Encode(RawToken)),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Auth.InvalidConfirmationToken");
    }

    [Fact]
    public async Task Handle_Should_Succeed_Without_Confirming_Again_When_Already_Confirmed()
    {
        // Arrange
        _user.EmailConfirmed = true;

        // Act
        var result = await _handler.Handle(
            new ConfirmEmailCommand(_user.Id, IdentityTestHelpers.Encode(RawToken)),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _userManager.DidNotReceive().ConfirmEmailAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
    }
}
