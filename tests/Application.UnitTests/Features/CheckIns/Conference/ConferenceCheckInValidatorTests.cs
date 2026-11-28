using Application.Features.CheckIns.Conference;
using FluentAssertions;

namespace UnitTests.Features.CheckIns.Conference;

public sealed class ConferenceCheckInValidatorTests
{
    private readonly ConferenceCheckInValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_For_Valid_Command()
    {
        _validator.Validate(new ConferenceCheckInCommand(Guid.NewGuid(), Guid.NewGuid())).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_Ticket_Code_Is_Empty()
    {
        _validator.Validate(new ConferenceCheckInCommand(Guid.NewGuid(), Guid.Empty)).IsValid.Should().BeFalse();
    }
}
