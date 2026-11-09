using FluentValidation;

namespace Application.Features.Conferences.Create;

public sealed class CreateConferenceValidator : AbstractValidator<CreateConferenceCommand>
{
    public CreateConferenceValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.StartDate)
            .NotEmpty();

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date must be on or after the start date.");

        RuleFor(x => x.Location)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.ParticipantLimit)
            .GreaterThan(0);
    }
}
