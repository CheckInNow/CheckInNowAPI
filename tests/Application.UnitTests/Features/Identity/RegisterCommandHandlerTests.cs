using Application.Features.Identity.Register;
using Domain.Constants;
using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace Application.UnitTests.Features.Identity;

public sealed class RegisterCommandHandlerTests
{
    private static readonly RegisterCommand Command =
        new("Jan", "Kowalski", "jan@example.com", "Password1!", "Password1!");

    private readonly UserManager<ApplicationUser> _userManager = CreateUserManager();
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _handler = new RegisterCommandHandler(_userManager);

        _userManager.FindByEmailAsync(Arg.Any<string>()).ReturnsNull();
        _userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);
        _userManager.AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);
    }

    [Fact]
    public async Task Handle_Should_Assign_User_Role_When_Registration_Succeeds()
    {
        // Act
        var result = await _handler.Handle(Command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _userManager.Received(1).AddToRoleAsync(
            Arg.Is<ApplicationUser>(u => u.Email == Command.Email),
            Roles.User);
        await _userManager.DidNotReceive().AddToRoleAsync(Arg.Any<ApplicationUser>(), Roles.Organizer);
        await _userManager.DidNotReceive().AddToRoleAsync(Arg.Any<ApplicationUser>(), Roles.Admin);
    }

    [Fact]
    public async Task Handle_Should_Delete_User_When_Role_Assignment_Fails()
    {
        // Arrange
        _userManager.AddToRoleAsync(Arg.Any<ApplicationUser>(), Roles.User)
            .Returns(IdentityResult.Failed(new IdentityError { Code = "Error", Description = "Error" }));

        // Act
        var result = await _handler.Handle(Command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Auth.RoleAssignmentFailed");
        await _userManager.Received(1).DeleteAsync(Arg.Any<ApplicationUser>());
    }

    [Fact]
    public async Task Handle_Should_Not_Assign_Role_When_Email_Is_Taken()
    {
        // Arrange
        _userManager.FindByEmailAsync(Command.Email).Returns(new ApplicationUser { Email = Command.Email });

        // Act
        var result = await _handler.Handle(Command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Auth.EmailTaken");
        await _userManager.DidNotReceive().AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_Should_Not_Assign_Role_When_Creation_Fails()
    {
        // Arrange
        _userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Failed(new IdentityError { Code = "Error", Description = "Error" }));

        // Act
        var result = await _handler.Handle(Command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Auth.RegistrationFailed");
        await _userManager.DidNotReceive().AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
    }

    private static UserManager<ApplicationUser> CreateUserManager()
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        return Substitute.For<UserManager<ApplicationUser>>(
            store, null, null, null, null, null, null, null, null);
    }
}
