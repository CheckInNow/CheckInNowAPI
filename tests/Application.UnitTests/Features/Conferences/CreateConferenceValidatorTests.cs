using Application.Features.Conferences.Create;
using FluentAssertions;

namespace Application.UnitTests.Features.Conferences;

public sealed class CreateConferenceValidatorTests
{
    private readonly CreateConferenceValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_For_Valid_Command()
    {
        _validator.Validate(ValidCommand()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_Pass_When_Start_And_End_Date_Are_Equal()
    {
        var date = new DateTimeOffset(2027, 5, 10, 9, 0, 0, TimeSpan.Zero);

        _validator.Validate(ValidCommand() with { StartDate = date, EndDate = date }).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_Should_Fail_When_Name_Is_Empty(string name)
    {
        _validator.Validate(ValidCommand() with { Name = name }).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Should_Fail_When_Start_Date_Is_After_End_Date()
    {
        var command = ValidCommand() with
        {
            StartDate = new DateTimeOffset(2027, 5, 12, 9, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2027, 5, 11, 9, 0, 0, TimeSpan.Zero)
        };

        _validator.Validate(command).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Validate_Should_Fail_When_Participant_Limit_Is_Not_Positive(int limit)
    {
        _validator.Validate(ValidCommand() with { ParticipantLimit = limit }).IsValid.Should().BeFalse();
    }

    private static CreateConferenceCommand ValidCommand() => new(
        "CheckIn Conf",
        "Description",
        new DateTimeOffset(2027, 5, 10, 9, 0, 0, TimeSpan.Zero),
        new DateTimeOffset(2027, 5, 11, 17, 0, 0, TimeSpan.Zero),
        "Legnica",
        100);
}
