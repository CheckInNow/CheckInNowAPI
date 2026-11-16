using FluentValidation;

namespace Application.Features.Registrations.GetAll;

public sealed class GetAllRegistrationsValidator : AbstractValidator<GetAllRegistrationsQuery>
{
    public GetAllRegistrationsValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}
