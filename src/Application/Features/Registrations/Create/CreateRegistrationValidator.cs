using FluentValidation;

namespace Application.Features.Registrations.Create;

public sealed class CreateRegistrationValidator : AbstractValidator<CreateRegistrationCommand>
{
    public CreateRegistrationValidator()
    {
        RuleFor(x => x.ConferenceId)
            .NotEmpty().WithMessage("ConferenceId is required.");
    }
}
