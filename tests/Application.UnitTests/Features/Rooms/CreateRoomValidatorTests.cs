using Application.Features.Rooms.Create;
using FluentAssertions;

namespace Application.UnitTests.Features.Rooms;

public sealed class CreateRoomValidatorTests
{
    private readonly CreateRoomValidator _validator = new();

    private static CreateRoomCommand ValidCommand() => new(Guid.NewGuid(), "Room A", 50);

    [Fact]
    public void Validate_Should_Pass_For_Valid_Command()
    {
        _validator.Validate(ValidCommand()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_Pass_When_Capacity_Is_Not_Provided()
    {
        _validator.Validate(ValidCommand() with { Capacity = null }).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_Should_Fail_When_Name_Is_Empty(string name)
    {
        _validator.Validate(ValidCommand() with { Name = name }).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Should_Fail_When_Name_Is_Longer_Than_200_Characters()
    {
        _validator.Validate(ValidCommand() with { Name = new string('a', 201) }).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Should_Pass_When_Name_Has_200_Characters()
    {
        _validator.Validate(ValidCommand() with { Name = new string('a', 200) }).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_Should_Fail_When_Capacity_Is_Not_Positive(int capacity)
    {
        _validator.Validate(ValidCommand() with { Capacity = capacity }).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Should_Fail_When_ConferenceId_Is_Empty()
    {
        _validator.Validate(ValidCommand() with { ConferenceId = Guid.Empty }).IsValid.Should().BeFalse();
    }
}
