using FluentValidation;

namespace Application.Features.Sessions.Update;

public sealed class UpdateSessionValidator : AbstractValidator<UpdateSessionCommand>
{
    public UpdateSessionValidator()
    {
        RuleFor(x => x.ConferenceId)
            .NotEmpty();

        RuleFor(x => x.SessionId)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200)
            .When(x => x.Title is not null);

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.RoomId)
            .NotEmpty()
            .When(x => x.RoomId.HasValue);

        RuleFor(x => x.SpeakerId)
            .NotEmpty()
            .When(x => x.SpeakerId.HasValue);

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime!.Value)
            .When(x => x.StartTime.HasValue && x.EndTime.HasValue)
            .WithMessage("End time must be after the start time.");
    }
}
