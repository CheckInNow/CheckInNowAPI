using FluentValidation;

namespace Application.Features.Sessions.Create;

public sealed class CreateSessionValidator : AbstractValidator<CreateSessionCommand>
{
    public CreateSessionValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.StartTime)
            .NotEmpty();

        RuleFor(x => x.EndTime)
            .NotEmpty()
            .GreaterThan(x => x.StartTime)
            .WithMessage("End time must be on or after the start time.");

        RuleFor(x => x.RoomId)
            .NotEmpty();

        RuleFor(x => x.SpeakerId)
            .NotEmpty();
    }
}
