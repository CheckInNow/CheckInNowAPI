using FluentValidation;

namespace Application.Features.CheckIns.Session;

public sealed class SessionCheckInValidator : AbstractValidator<SessionCheckInCommand>
{
    public SessionCheckInValidator()
    {
        RuleFor(c => c.TicketCode)
            .NotEmpty();
    }
}
