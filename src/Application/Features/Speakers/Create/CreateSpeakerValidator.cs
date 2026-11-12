using FluentValidation;

namespace Application.Features.Speakers.Create;

public sealed class CreateSpeakerValidator : AbstractValidator<CreateSpeakerCommand>
{
    public CreateSpeakerValidator()
    {
        RuleFor(x => x.ConferenceId)
            .NotEmpty();

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Bio)
            .MaximumLength(1000);
    }
}
