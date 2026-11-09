using FluentValidation;

namespace Application.Features.Conferences.Update;

public sealed class UpdateConferenceValidator : AbstractValidator<UpdateConferenceCommand>
{
    public UpdateConferenceValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200)
            .When(x => x.Name is not null);

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.Location)
            .NotEmpty()
            .MaximumLength(200)
            .When(x => x.Location is not null);

        RuleFor(x => x.ParticipantLimit)
            .GreaterThan(0)
            .When(x => x.ParticipantLimit.HasValue);

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate!.Value)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("End date must be on or after the start date.");
    }
}
