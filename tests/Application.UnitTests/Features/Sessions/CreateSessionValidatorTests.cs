using Application.Features.Sessions.Create;
using FluentAssertions;
using static Application.UnitTests.Features.Sessions.SessionTestData;

namespace Application.UnitTests.Features.Sessions;

public sealed class CreateSessionValidatorTests
{
    private readonly CreateSessionValidator _validator = new();

    private static CreateSessionCommand Command() =>
        new(Guid.NewGuid(), "Title", "Description", At(10), At(11), Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public void Validate_Should_Pass_For_Valid_Command()
    {
        _validator.Validate(Command()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_Pass_When_Description_Is_Null()
    {
        _validator.Validate(Command() with { Description = null }).IsValid.Should().BeTrue();
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
    public void Validate_Should_Fail_When_End_Time_Is_Not_After_Start_Time()
    {
        _validator.Validate(Command() with { StartTime = At(11), EndTime = At(10) }).IsValid.Should().BeFalse();
        _validator.Validate(Command() with { StartTime = At(11), EndTime = At(11) }).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Should_Fail_When_Room_Id_Is_Empty()
    {
        _validator.Validate(Command() with { RoomId = Guid.Empty }).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Should_Fail_When_Speaker_Id_Is_Empty()
    {
        _validator.Validate(Command() with { SpeakerId = Guid.Empty }).IsValid.Should().BeFalse();
    }
}
