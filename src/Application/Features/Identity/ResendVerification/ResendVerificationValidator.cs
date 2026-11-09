using FluentValidation;

namespace Application.Features.Identity.ResendVerification;

public sealed class ResendVerificationValidator : AbstractValidator<ResendVerificationCommand>
{
    public ResendVerificationValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
