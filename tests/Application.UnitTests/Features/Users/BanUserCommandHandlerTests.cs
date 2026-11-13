using Application.Abstractions.Identity;
using Application.Features.Users.Ban;
using Application.Features.Users.Unban;
using Application.UnitTests.Features.Identity;
using Domain.Common;
using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace Application.UnitTests.Features.Users;

public sealed class BanUserCommandHandlerTests
{
    private static readonly Guid AdminId = Guid.NewGuid();

    private readonly UserManager<ApplicationUser> _userManager = IdentityTestHelpers.CreateUserManager();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly ApplicationUser _user = new()
    {
        Id = Guid.NewGuid(),
        Email = "jan@example.com",
        RefreshToken = "refresh-token",
        RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddDays(7)
    };
    private readonly BanUserCommandHandler _banHandler;
    private readonly UnbanUserCommandHandler _unbanHandler;

    public BanUserCommandHandlerTests()
    {
        _currentUser.UserId.Returns(AdminId);
        _userManager.FindByIdAsync(_user.Id.ToString()).Returns(_user);
        _userManager.UpdateAsync(Arg.Any<ApplicationUser>()).Returns(IdentityResult.Success);

        _banHandler = new BanUserCommandHandler(_userManager, _currentUser);
        _unbanHandler = new UnbanUserCommandHandler(_userManager, _currentUser);
    }

    [Fact]
    public async Task Ban_Should_Lock_User_Out_Permanently_And_Revoke_Refresh_Token()
    {
        // Act
        var result = await _banHandler.Handle(new BanUserCommand(_user.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _user.LockoutEnabled.Should().BeTrue();
        _user.LockoutEnd.Should().Be(DateTimeOffset.MaxValue);
        _user.RefreshToken.Should().BeNull();
        _user.RefreshTokenExpiryTime.Should().BeNull();
        await _userManager.Received(1).UpdateAsync(_user);
    }

    [Fact]
    public async Task Ban_Should_Return_Conflict_When_User_Is_Already_Banned()
    {
        // Arrange
        _user.LockoutEnabled = true;
        _user.LockoutEnd = DateTimeOffset.MaxValue;

        // Act
        var result = await _banHandler.Handle(new BanUserCommand(_user.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Conflict);
        await _userManager.DidNotReceive().UpdateAsync(Arg.Any<ApplicationUser>());
    }

    [Fact]
    public async Task Ban_Should_Return_Forbidden_When_Admin_Bans_Themselves()
    {
        // Arrange
        _currentUser.UserId.Returns(_user.Id);

        // Act
        var result = await _banHandler.Handle(new BanUserCommand(_user.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Forbidden);
        _user.LockoutEnd.Should().BeNull();
        await _userManager.DidNotReceive().UpdateAsync(Arg.Any<ApplicationUser>());
    }

    [Fact]
    public async Task Ban_Should_Return_NotFound_When_User_Does_Not_Exist()
    {
        // Arrange
        _userManager.FindByIdAsync(Arg.Any<string>()).ReturnsNull();

        // Act
        var result = await _banHandler.Handle(new BanUserCommand(Guid.NewGuid()), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Unban_Should_Clear_Lockout()
    {
        // Arrange
        _user.LockoutEnabled = true;
        _user.LockoutEnd = DateTimeOffset.MaxValue;

        // Act
        var result = await _unbanHandler.Handle(new UnbanUserCommand(_user.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _user.LockoutEnd.Should().BeNull();
        await _userManager.Received(1).UpdateAsync(_user);
    }

    [Fact]
    public async Task Unban_Should_Return_Conflict_When_User_Is_Not_Banned()
    {
        // Act
        var result = await _unbanHandler.Handle(new UnbanUserCommand(_user.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Conflict);
        await _userManager.DidNotReceive().UpdateAsync(Arg.Any<ApplicationUser>());
    }

    [Fact]
    public async Task Unban_Should_Return_Forbidden_When_Admin_Unbans_Themselves()
    {
        // Arrange
        _currentUser.UserId.Returns(_user.Id);

        // Act
        var result = await _unbanHandler.Handle(new UnbanUserCommand(_user.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public async Task Unban_Should_Return_NotFound_When_User_Does_Not_Exist()
    {
        // Arrange
        _userManager.FindByIdAsync(Arg.Any<string>()).ReturnsNull();

        // Act
        var result = await _unbanHandler.Handle(new UnbanUserCommand(Guid.NewGuid()), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }
}
