using Application.Abstractions.Identity;
using Application.Features.Identity.Login;
using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace Application.UnitTests.Features.Identity;

public sealed class LoginCommandHandlerTests
{
    private static readonly LoginCommand Command = new("jan@example.com", "Password1!");
    private static readonly TokenResponse Token = new("access", "refresh", DateTime.UtcNow.AddHours(1));

    private readonly UserManager<ApplicationUser> _userManager = IdentityTestHelpers.CreateUserManager();
    private readonly ITokenService _tokens = Substitute.For<ITokenService>();
    private readonly ApplicationUser _user = new() { Email = Command.Email, UserName = Command.Email, FirstName = "Jan", LastName = "Kowalski" };
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _handler = new LoginCommandHandler(_userManager, _tokens);

        _userManager.FindByEmailAsync(Command.Email).Returns(_user);
        _userManager.IsLockedOutAsync(_user).Returns(false);
        _userManager.CheckPasswordAsync(_user, Command.Password).Returns(true);
        _userManager.AccessFailedAsync(_user).Returns(IdentityResult.Success);
        _userManager.ResetAccessFailedCountAsync(_user).Returns(IdentityResult.Success);
        _tokens.GenerateTokenAsync(Arg.Any<ApplicationUser>(), Arg.Any<CancellationToken>()).Returns(Token);
    }

    [Fact]
    public async Task Handle_Should_Return_Token_And_Reset_Failed_Count_When_Password_Is_Correct()
    {
        // Act
        var result = await _handler.Handle(Command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Token);
        await _userManager.Received(1).ResetAccessFailedCountAsync(_user);
        await _userManager.DidNotReceive().AccessFailedAsync(Arg.Any<ApplicationUser>());
    }

    [Fact]
    public async Task Handle_Should_Increment_Failed_Count_When_Password_Is_Wrong()
    {
        // Arrange
        _userManager.CheckPasswordAsync(_user, Command.Password).Returns(false);

        // Act
        var result = await _handler.Handle(Command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Auth.InvalidCredentials");
        await _userManager.Received(1).AccessFailedAsync(_user);
        await _userManager.DidNotReceive().ResetAccessFailedCountAsync(Arg.Any<ApplicationUser>());
        await _tokens.DidNotReceive().GenerateTokenAsync(Arg.Any<ApplicationUser>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Fail_When_User_Is_Locked_Out()
    {
        // Arrange
        _userManager.IsLockedOutAsync(_user).Returns(true);

        // Act
        var result = await _handler.Handle(Command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Auth.LockedOut");
        await _userManager.DidNotReceive().CheckPasswordAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
        await _userManager.DidNotReceive().AccessFailedAsync(Arg.Any<ApplicationUser>());
        await _tokens.DidNotReceive().GenerateTokenAsync(Arg.Any<ApplicationUser>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Fail_When_User_Does_Not_Exist()
    {
        // Arrange
        _userManager.FindByEmailAsync(Command.Email).ReturnsNull();

        // Act
        var result = await _handler.Handle(Command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Auth.InvalidCredentials");
        await _userManager.DidNotReceive().AccessFailedAsync(Arg.Any<ApplicationUser>());
        await _tokens.DidNotReceive().GenerateTokenAsync(Arg.Any<ApplicationUser>(), Arg.Any<CancellationToken>());
    }
}
