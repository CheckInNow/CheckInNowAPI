
using Application.Abstractions.Identity;
using Application.Abstractions.OAuth;
using Application.Features.Identity.Login.GoogleLogin;
using Domain.Constants;
using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace Application.UnitTests.Features.Identity;

public sealed class GoogleLoginCommandHandlerTests
{
    private const string IdToken = "google-id-token";
    private const string Subject = "google-subject-123";
    private const string Email = "jan@example.com";

    private static readonly TokenResponse Token = new("access", "refresh", DateTime.UtcNow.AddHours(1));

    private readonly IGoogleTokenVerifier _google = Substitute.For<IGoogleTokenVerifier>();
    private readonly UserManager<ApplicationUser> _userManager = CreateUserManager();
    private readonly ITokenService _tokens = Substitute.For<ITokenService>();
    private readonly GoogleLoginCommandHandler _handler;

    public GoogleLoginCommandHandlerTests()
    {
        _handler = new GoogleLoginCommandHandler(_google, _userManager, _tokens);

        _userManager.FindByLoginAsync(Arg.Any<string>(), Arg.Any<string>()).ReturnsNull();
        _userManager.FindByEmailAsync(Arg.Any<string>()).ReturnsNull();
        _userManager.CreateAsync(Arg.Any<ApplicationUser>()).Returns(IdentityResult.Success);
        _userManager.AddLoginAsync(Arg.Any<ApplicationUser>(), Arg.Any<UserLoginInfo>()).Returns(IdentityResult.Success);
        _userManager.AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);
        _userManager.IsLockedOutAsync(Arg.Any<ApplicationUser>()).Returns(false);
        _tokens.GenerateTokenAsync(Arg.Any<ApplicationUser>(), Arg.Any<CancellationToken>()).Returns(Token);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Token_Is_Invalid()
    {
        // Arrange
        _google.ValidateAsync(IdToken, Arg.Any<CancellationToken>()).ReturnsNull();

        // Act
        var result = await _handler.Handle(new GoogleLoginCommand(IdToken), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("OAuth.InvalidToken");
        await _tokens.DidNotReceive().GenerateTokenAsync(Arg.Any<ApplicationUser>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Google_Email_Is_Not_Verified()
    {
        // Arrange
        SetupGoogleTokenAsValid(emailVerified: false);

        // Act
        var result = await _handler.Handle(new GoogleLoginCommand(IdToken), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("OAuth.GoogleEmailNotVerified");
        await _userManager.DidNotReceive().CreateAsync(Arg.Any<ApplicationUser>());
    }

    [Fact]
    public async Task Handle_Should_Create_Confirmed_User_And_Return_Token_When_User_Is_New()
    {
        // Arrange
        SetupGoogleTokenAsValid(emailVerified: true);

        // Act
        var result = await _handler.Handle(new GoogleLoginCommand(IdToken), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Token);
        await _userManager.Received(1).CreateAsync(
            Arg.Is<ApplicationUser>(u => u.Email == Email && u.UserName == Email && u.EmailConfirmed));
        await _userManager.Received(1).AddLoginAsync(
            Arg.Any<ApplicationUser>(),
            Arg.Is<UserLoginInfo>(l => l.LoginProvider == "Google" && l.ProviderKey == Subject));
    }

    [Fact]
    public async Task Handle_Should_Link_Google_Login_When_Existing_User_Has_Confirmed_Email()
    {
        // Arrange
        SetupGoogleTokenAsValid(emailVerified: true);
        var existing = new ApplicationUser { Email = Email, UserName = Email, EmailConfirmed = true, FirstName = "Jan", LastName = "Kowalski" };
        _userManager.FindByEmailAsync(Email).Returns(existing);

        // Act
        var result = await _handler.Handle(new GoogleLoginCommand(IdToken), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _userManager.Received(1).AddLoginAsync(
            existing,
            Arg.Is<UserLoginInfo>(l => l.LoginProvider == "Google" && l.ProviderKey == Subject));
        await _userManager.DidNotReceive().CreateAsync(Arg.Any<ApplicationUser>());
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Existing_User_Has_Unconfirmed_Email()
    {
        // Arrange
        SetupGoogleTokenAsValid(emailVerified: true);
        var existing = new ApplicationUser { Email = Email, UserName = Email, EmailConfirmed = false, FirstName = "Jan", LastName = "Kowalski" };
        _userManager.FindByEmailAsync(Email).Returns(existing);

        // Act
        var result = await _handler.Handle(new GoogleLoginCommand(IdToken), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Auth.EmailNotConfirmed");
        await _userManager.DidNotReceive().AddLoginAsync(Arg.Any<ApplicationUser>(), Arg.Any<UserLoginInfo>());
        await _tokens.DidNotReceive().GenerateTokenAsync(Arg.Any<ApplicationUser>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Return_Token_Without_Linking_When_Google_Login_Already_Exists()
    {
        // Arrange
        SetupGoogleTokenAsValid(emailVerified: true);
        var linked = new ApplicationUser { Email = Email, UserName = Email, EmailConfirmed = true, FirstName = "Jan", LastName = "Kowalski" };
        _userManager.FindByLoginAsync("Google", Subject).Returns(linked);

        // Act
        var result = await _handler.Handle(new GoogleLoginCommand(IdToken), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Token);
        await _userManager.DidNotReceive().FindByEmailAsync(Arg.Any<string>());
        await _userManager.DidNotReceive().AddLoginAsync(Arg.Any<ApplicationUser>(), Arg.Any<UserLoginInfo>());
        await _userManager.DidNotReceive().CreateAsync(Arg.Any<ApplicationUser>());
    }

    [Fact]
    public async Task Handle_Should_Fail_When_User_Is_Locked_Out()
    {
        // Arrange
        SetupGoogleTokenAsValid(emailVerified: true);
        var linked = new ApplicationUser { Email = Email, UserName = Email, EmailConfirmed = true, FirstName = "Jan", LastName = "Kowalski" };
        _userManager.FindByLoginAsync("Google", Subject).Returns(linked);
        _userManager.IsLockedOutAsync(linked).Returns(true);

        // Act
        var result = await _handler.Handle(new GoogleLoginCommand(IdToken), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Auth.LockedOut");
        await _tokens.DidNotReceive().GenerateTokenAsync(Arg.Any<ApplicationUser>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Fail_When_User_Creation_Fails()
    {
        // Arrange
        SetupGoogleTokenAsValid(emailVerified: true);
        _userManager.CreateAsync(Arg.Any<ApplicationUser>())
            .Returns(IdentityResult.Failed(new IdentityError { Code = "Error", Description = "Error" }));

        // Act
        var result = await _handler.Handle(new GoogleLoginCommand(IdToken), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Auth.CreateFailed");
        await _userManager.DidNotReceive().AddLoginAsync(Arg.Any<ApplicationUser>(), Arg.Any<UserLoginInfo>());
    }

    [Fact]
    public async Task Handle_Should_Delete_New_User_When_Linking_Google_Login_Fails()
    {
        // Arrange
        SetupGoogleTokenAsValid(emailVerified: true);
        _userManager.AddLoginAsync(Arg.Any<ApplicationUser>(), Arg.Any<UserLoginInfo>())
            .Returns(IdentityResult.Failed(new IdentityError { Code = "Error", Description = "Error" }));

        // Act
        var result = await _handler.Handle(new GoogleLoginCommand(IdToken), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Auth.AddLoginFailed");
        await _userManager.Received(1).DeleteAsync(Arg.Any<ApplicationUser>());
        await _tokens.DidNotReceive().GenerateTokenAsync(Arg.Any<ApplicationUser>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Assign_User_Role_When_User_Is_New()
    {
        // Arrange
        SetupGoogleTokenAsValid();

        // Act
        var result = await _handler.Handle(new GoogleLoginCommand(IdToken), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _userManager.Received(1).AddToRoleAsync(Arg.Any<ApplicationUser>(), Roles.User);
        await _userManager.DidNotReceive().AddToRoleAsync(Arg.Any<ApplicationUser>(), Roles.Organizer);
        await _userManager.DidNotReceive().AddToRoleAsync(Arg.Any<ApplicationUser>(), Roles.Admin);
    }

    [Fact]
    public async Task Handle_Should_Delete_New_User_When_Role_Assignment_Fails()
    {
        // Arrange
        SetupGoogleTokenAsValid();
        _userManager.AddToRoleAsync(Arg.Any<ApplicationUser>(), Roles.User)
            .Returns(IdentityResult.Failed(new IdentityError { Code = "Error", Description = "Error" }));

        // Act
        var result = await _handler.Handle(new GoogleLoginCommand(IdToken), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Auth.RoleAssignmentFailed");
        await _userManager.Received(1).DeleteAsync(Arg.Any<ApplicationUser>());
        await _userManager.DidNotReceive().AddLoginAsync(Arg.Any<ApplicationUser>(), Arg.Any<UserLoginInfo>());
        await _tokens.DidNotReceive().GenerateTokenAsync(Arg.Any<ApplicationUser>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Not_Assign_Role_When_Linking_Existing_User()
    {
        // Arrange
        SetupGoogleTokenAsValid();
        var existing = new ApplicationUser { Email = Email, UserName = Email, EmailConfirmed = true, FirstName = "Jan", LastName = "Kowalski" };
        _userManager.FindByEmailAsync(Email).Returns(existing);

        // Act
        var result = await _handler.Handle(new GoogleLoginCommand(IdToken), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _userManager.DidNotReceive().AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_Should_Not_Assign_Role_When_Google_Login_Already_Exists()
    {
        // Arrange
        SetupGoogleTokenAsValid();
        var linked = new ApplicationUser { Email = Email, UserName = Email, EmailConfirmed = true, FirstName = "Jan", LastName = "Kowalski" };
        _userManager.FindByLoginAsync("Google", Subject).Returns(linked);

        // Act
        var result = await _handler.Handle(new GoogleLoginCommand(IdToken), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _userManager.DidNotReceive().AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
    }

    private void SetupGoogleTokenAsValid(bool emailVerified = true)
    {
        _google.ValidateAsync(IdToken, Arg.Any<CancellationToken>())
        .Returns(new GoogleUserInfo(Subject, Email, emailVerified, "Jan", "Kowalski"));
    }


    private static UserManager<ApplicationUser> CreateUserManager()
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        return Substitute.For<UserManager<ApplicationUser>>(
            store, null, null, null, null, null, null, null, null);
    }
}
