using Application.Features.Registrations.GetAll;
using FluentAssertions;

namespace Application.UnitTests.Features.Registrations;

public sealed class GetAllRegistrationsValidatorTests
{
    private readonly GetAllRegistrationsValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_For_Default_Query()
    {
        _validator.Validate(new GetAllRegistrationsQuery(Guid.NewGuid())).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_Should_Fail_When_Page_Is_Less_Than_One(int page)
    {
        _validator.Validate(new GetAllRegistrationsQuery(Guid.NewGuid(), Page: page)).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Validate_Should_Fail_When_Page_Size_Is_Out_Of_Range(int pageSize)
    {
        _validator.Validate(new GetAllRegistrationsQuery(Guid.NewGuid(), PageSize: pageSize)).IsValid.Should().BeFalse();
    }
}
