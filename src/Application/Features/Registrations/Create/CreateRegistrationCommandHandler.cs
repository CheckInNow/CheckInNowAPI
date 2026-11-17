using Application.Abstractions.Data;
using Application.Abstractions.Identity;
using Application.Abstractions.Messaging;
using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Registrations.Create;

public sealed class CreateRegistrationCommandHandler(IAppDbContext context, ICurrentUser currentUser)
    : ICommandHandler<CreateRegistrationCommand, CreateRegistrationResponse>
{
    public async Task<Result<CreateRegistrationResponse>> Handle(CreateRegistrationCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId;
        if (userId is null)
        {
            return Result.Failure<CreateRegistrationResponse>(
                Error.Forbidden("Auth.Forbidden", "Current user could not be determined."));
        }

        var now = DateTimeOffset.UtcNow;

        var rules = await context.Conferences
            .Where(c => c.Id == request.ConferenceId && c.DeletedAt == null)
            .Select(c => new
            {
                IsStarted = c.StartDate <= now,
                IsFull = c.Registrations.Count() >= c.ParticipantLimit,
                IsAlreadyRegistered = c.Registrations.Any(r => r.UserId == userId.Value)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (rules is null)
        {
            return Result.Failure<CreateRegistrationResponse>(
                Error.NotFound("Conference.NotFound", $"Conference with ID '{request.ConferenceId}' was not found."));
        }

        var isEmailConfirmed = await context.Users
            .Where(u => u.Id == userId.Value)
            .Select(u => u.EmailConfirmed)
            .FirstOrDefaultAsync(cancellationToken);

        if (!isEmailConfirmed)
        {
            return Result.Failure<CreateRegistrationResponse>(
                Error.Forbidden("Registration.EmailNotConfirmed", "Confirm your email address to register for a conference."));
        }

        if (rules.IsStarted)
        {
            return Result.Failure<CreateRegistrationResponse>(
                Error.Conflict("Registration.ConferenceStarted", "Registration is closed because the conference has already started."));
        }

        if (rules.IsAlreadyRegistered)
        {
            return Result.Failure<CreateRegistrationResponse>(
                Error.Conflict("Registration.AlreadyRegistered", "You are already registered for this conference."));
        }

        if (rules.IsFull)
        {
            return Result.Failure<CreateRegistrationResponse>(
                Error.Conflict("Registration.ParticipantLimitReached", "This conference has reached its participant limit."));
        }

        var registration = new Registration
        {
            ConferenceId = request.ConferenceId,
            UserId = userId.Value,
            TicketCode = Guid.NewGuid()
        };

        context.Registrations.Add(registration);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateRegistrationResponse(
            registration.Id,
            registration.ConferenceId,
            registration.UserId,
            registration.CreatedAt));
    }
}
