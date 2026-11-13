using Application.Features.Sessions.Update;
using FluentAssertions;
using static Application.UnitTests.Features.Sessions.SessionTestData;

namespace Application.UnitTests.Features.Sessions;

public sealed class UpdateSessionValidatorTests
{
    private readonly UpdateSessionValidator _validator = new();

    private static UpdateSessionCommand Command() =>
        new(Guid.NewGuid(), Guid.NewGuid(), null, null, null, null, null, null);

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
    public void Validate_Should_Fail_When_Title_Is_Empty()
    {
        _validator.Validate(Command() with { Title = "" }).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Should_Fail_When_Title_Is_Too_Long()
    {
        _validator.Validate(Command() with { Title = new string('a', 201) }).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Should_Fail_When_Description_Is_Too_Long()
    {
        _validator.Validate(Command() with { Description = new string('a', 1001) }).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Should_Fail_When_Room_Id_Is_Empty_Guid()
    {
        _validator.Validate(Command() with { RoomId = Guid.Empty }).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Should_Fail_When_Speaker_Id_Is_Empty_Guid()
    {
        _validator.Validate(Command() with { SpeakerId = Guid.Empty }).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Should_Fail_When_Both_Times_Provided_And_End_Is_Not_After_Start()
    {
        _validator.Validate(Command() with { StartTime = At(11), EndTime = At(10) }).IsValid.Should().BeFalse();
        _validator.Validate(Command() with { StartTime = At(11), EndTime = At(11) }).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Should_Pass_When_Only_One_Time_Is_Provided()
    {
        _validator.Validate(Command() with { StartTime = At(11) }).IsValid.Should().BeTrue();
    }
}
