using Application.Abstractions.Identity;
using Application.Features.Identity.RefreshToken;
using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Application.UnitTests.Features.Identity;

public sealed class RefreshTokenCommandHandlerTests
{
    private static readonly RefreshTokenCommand Command = new("expired-access", "refresh");
    private static readonly TokenResponse Token = new("new-access", "new-refresh", DateTime.UtcNow.AddHours(1));

    private readonly UserManager<ApplicationUser> _userManager = IdentityTestHelpers.CreateUserManager();
    private readonly ITokenService _tokens = Substitute.For<ITokenService>();
    private readonly ApplicationUser _user = new() { Email = "jan@example.com", UserName = "jan@example.com", FirstName = "Jan", LastName = "Kowalski" };
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _handler = new RefreshTokenCommandHandler(_userManager, _tokens);

        _tokens.GetUserIdFromExpiredToken(Command.AccessToken).Returns(_user.Id.ToString());
        _userManager.FindByIdAsync(_user.Id.ToString()).Returns(_user);
        _userManager.IsLockedOutAsync(_user).Returns(false);
        _tokens.RefreshTokenAsync(Command.AccessToken, Command.RefreshToken, Arg.Any<CancellationToken>()).Returns(Token);
    }

    [Fact]
    public async Task Handle_Should_Return_New_Token_When_User_Is_Not_Locked_Out()
    {
        // Act
        var result = await _handler.Handle(Command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Token);
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
        await _tokens.DidNotReceive().RefreshTokenAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Refresh_Token_Is_Invalid()
    {
        // Arrange
        _tokens.RefreshTokenAsync(Command.AccessToken, Command.RefreshToken, Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("Invalid or expired refresh token."));

        // Act
        var result = await _handler.Handle(Command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Auth.InvalidRefreshToken");
    }
}
