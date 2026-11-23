using FluentValidation;

namespace Application.Features.Registrations.GetMy;

public sealed class GetMyRegistrationsValidator : AbstractValidator<GetMyRegistrationsQuery>
{
    public GetMyRegistrationsValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}
