using Application.Features.Speakers.Create;
using FluentAssertions;

namespace Application.UnitTests.Features.Speakers;

public sealed class CreateSpeakerValidatorTests
{
    private readonly CreateSpeakerValidator _validator = new();

    private static CreateSpeakerCommand ValidCommand() => new(Guid.NewGuid(), "Jan", "Kowalski", "Backend developer.");

    [Fact]
    public void Validate_Should_Pass_For_Valid_Command()
    {
        _validator.Validate(ValidCommand()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_Pass_When_Bio_Is_Not_Provided()
    {
        _validator.Validate(ValidCommand() with { Bio = null }).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_Should_Fail_When_First_Name_Is_Empty(string firstName)
    {
        _validator.Validate(ValidCommand() with { FirstName = firstName }).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_Should_Fail_When_Last_Name_Is_Empty(string lastName)
    {
        _validator.Validate(ValidCommand() with { LastName = lastName }).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Should_Fail_When_Name_Is_Longer_Than_100_Characters()
    {
        _validator.Validate(ValidCommand() with { FirstName = new string('a', 101) }).IsValid.Should().BeFalse();
        _validator.Validate(ValidCommand() with { LastName = new string('a', 101) }).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Should_Fail_When_Bio_Is_Longer_Than_1000_Characters()
    {
        _validator.Validate(ValidCommand() with { Bio = new string('a', 1001) }).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Should_Pass_When_Bio_Has_1000_Characters()
    {
        _validator.Validate(ValidCommand() with { Bio = new string('a', 1000) }).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_ConferenceId_Is_Empty()
    {
        _validator.Validate(ValidCommand() with { ConferenceId = Guid.Empty }).IsValid.Should().BeFalse();
    }
}
