using Application.Features.Conferences.Update;
using FluentAssertions;

namespace Application.UnitTests.Features.Conferences;

public sealed class UpdateConferenceValidatorTests
{
    private readonly UpdateConferenceValidator _validator = new();

    private static UpdateConferenceCommand Command() => new(Guid.NewGuid(), null, null, null, null, null, null);

    [Fact]
    public void Validate_Should_Pass_When_No_Fields_Are_Provided()
    {
        _validator.Validate(Command()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_Pass_When_Description_Is_Empty_String()
    {
        _validator.Validate(Command() with { Description = "" }).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_Name_Is_Empty()
    {
        _validator.Validate(Command() with { Name = "" }).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Should_Fail_When_Participant_Limit_Is_Not_Positive()
    {
        _validator.Validate(Command() with { ParticipantLimit = 0 }).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Should_Fail_When_Both_Dates_Provided_And_Start_Is_After_End()
    {
        var command = Command() with
        {
            StartDate = new DateTimeOffset(2027, 5, 12, 9, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2027, 5, 11, 9, 0, 0, TimeSpan.Zero)
        };

        _validator.Validate(command).IsValid.Should().BeFalse();
    }
}
