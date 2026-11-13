using Application.Abstractions.Identity;
using Application.Features.Users.AddOrganizerRole;
using Application.Features.Users.RemoveOrganizerRole;
using Application.UnitTests.Features.Identity;
using Domain.Common;
using Domain.Constants;
using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace Application.UnitTests.Features.Users;

public sealed class OrganizerRoleCommandHandlerTests
{
    private static readonly Guid AdminId = Guid.NewGuid();

    private readonly UserManager<ApplicationUser> _userManager = IdentityTestHelpers.CreateUserManager();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly ApplicationUser _user = new() { Id = Guid.NewGuid(), Email = "jan@example.com" };
    private readonly AddOrganizerRoleCommandHandler _addHandler;
    private readonly RemoveOrganizerRoleCommandHandler _removeHandler;

    public OrganizerRoleCommandHandlerTests()
    {
        _currentUser.UserId.Returns(AdminId);
        _userManager.FindByIdAsync(_user.Id.ToString()).Returns(_user);
        _userManager.AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);
        _userManager.RemoveFromRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);

        _addHandler = new AddOrganizerRoleCommandHandler(_userManager, _currentUser);
        _removeHandler = new RemoveOrganizerRoleCommandHandler(_userManager, _currentUser);
    }

    [Fact]
    public async Task Add_Should_Add_Organizer_Role_Without_Removing_Other_Roles()
    {
        // Arrange
        _userManager.IsInRoleAsync(_user, Roles.Organizer).Returns(false);

        // Act
        var result = await _addHandler.Handle(new AddOrganizerRoleCommand(_user.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _userManager.Received(1).AddToRoleAsync(_user, Roles.Organizer);
        await _userManager.DidNotReceive().RemoveFromRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Add_Should_Return_Conflict_When_User_Is_Already_Organizer()
    {
        // Arrange
        _userManager.IsInRoleAsync(_user, Roles.Organizer).Returns(true);

        // Act
        var result = await _addHandler.Handle(new AddOrganizerRoleCommand(_user.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Conflict);
        await _userManager.DidNotReceive().AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Add_Should_Return_NotFound_When_User_Does_Not_Exist()
    {
        // Arrange
        _userManager.FindByIdAsync(Arg.Any<string>()).ReturnsNull();

        // Act
        var result = await _addHandler.Handle(new AddOrganizerRoleCommand(Guid.NewGuid()), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Add_Should_Return_Forbidden_When_Admin_Changes_Own_Roles()
    {
        // Arrange
        _currentUser.UserId.Returns(_user.Id);

        // Act
        var result = await _addHandler.Handle(new AddOrganizerRoleCommand(_user.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Forbidden);
        await _userManager.DidNotReceive().AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Remove_Should_Remove_Organizer_Role()
    {
        // Arrange
        _userManager.IsInRoleAsync(_user, Roles.Organizer).Returns(true);

        // Act
        var result = await _removeHandler.Handle(new RemoveOrganizerRoleCommand(_user.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _userManager.Received(1).RemoveFromRoleAsync(_user, Roles.Organizer);
    }

    [Fact]
    public async Task Remove_Should_Return_Conflict_When_User_Is_Not_Organizer()
    {
        // Arrange
        _userManager.IsInRoleAsync(_user, Roles.Organizer).Returns(false);

        // Act
        var result = await _removeHandler.Handle(new RemoveOrganizerRoleCommand(_user.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Conflict);
        await _userManager.DidNotReceive().RemoveFromRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Remove_Should_Return_NotFound_When_User_Does_Not_Exist()
    {
        // Arrange
        _userManager.FindByIdAsync(Arg.Any<string>()).ReturnsNull();

        // Act
        var result = await _removeHandler.Handle(new RemoveOrganizerRoleCommand(Guid.NewGuid()), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Remove_Should_Return_Forbidden_When_Admin_Changes_Own_Roles()
    {
        // Arrange
        _currentUser.UserId.Returns(_user.Id);

        // Act
        var result = await _removeHandler.Handle(new RemoveOrganizerRoleCommand(_user.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Forbidden);
        await _userManager.DidNotReceive().RemoveFromRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
    }
}
