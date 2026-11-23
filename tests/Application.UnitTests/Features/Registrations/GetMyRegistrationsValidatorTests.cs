using Application.Features.Registrations.GetMy;
using FluentAssertions;

namespace Application.UnitTests.Features.Registrations;

public sealed class GetMyRegistrationsValidatorTests
{
    private readonly GetMyRegistrationsValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_For_Default_Query()
    {
        _validator.Validate(new GetMyRegistrationsQuery()).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_Should_Fail_When_Page_Is_Less_Than_One(int page)
    {
        _validator.Validate(new GetMyRegistrationsQuery(Page: page)).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Validate_Should_Fail_When_Page_Size_Is_Out_Of_Range(int pageSize)
    {
        _validator.Validate(new GetMyRegistrationsQuery(PageSize: pageSize)).IsValid.Should().BeFalse();
    }
}
