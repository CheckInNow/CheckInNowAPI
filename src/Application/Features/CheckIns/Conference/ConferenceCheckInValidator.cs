using FluentValidation;

namespace Application.Features.CheckIns.Conference;

public sealed class ConferenceCheckInValidator : AbstractValidator<ConferenceCheckInCommand>
{
    public ConferenceCheckInValidator()
    {
        RuleFor(c => c.TicketCode)
            .NotEmpty();
    }
}
