using Application.Abstractions.Data;
using Application.Abstractions.Identity;
using Application.Abstractions.Messaging;
using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.CheckIns.Session;

public sealed class SessionCheckInCommandHandler(IAppDbContext context, ICurrentUser currentUser) : ICommandHandler<SessionCheckInCommand, SessionCheckInResponse>
{
    public async Task<Result<SessionCheckInResponse>> Handle(SessionCheckInCommand request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        var rules = await context.Registrations
            .Where(r => r.TicketCode == request.TicketCode
                && r.ConferenceId == request.ConferenceId
                && r.Conference.DeletedAt == null
                && r.Conference.Sessions.Any(s => s.Id == request.SessionId && s.DeletedAt == null))
            .Select(r => new
            {
                RegistrationId = r.Id,
                FullName = r.User.FirstName + " " + r.User.LastName,
                IsAlreadyScanned = context.SessionCheckIns.Any(s => s.SessionId == request.SessionId && s.RegistrationId == r.Id),
                IsUserScanningAllowed = r.Conference.OrganizerId == currentUser.UserId,
                IsSessionStarted = r.Conference.Sessions.Any(s => s.StartTime < now && s.Id == request.SessionId),
                IsSessionEnded = r.Conference.Sessions.Any(s => s.EndTime < now && s.Id == request.SessionId),
                HasConferenceCheckIn = context.ConferenceCheckIns.Any(c => c.RegistrationId == r.Id),
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (rules is null)
        {
            return Result.Failure<SessionCheckInResponse>(
                Error.NotFound("Ticket.Invalid", "Invalid ticket."));
        }

        if (!rules.IsUserScanningAllowed)
        {
            return Result.Failure<SessionCheckInResponse>(
                Error.Forbidden("CheckIn.Forbidden", "You are not allowed to scan tickets."));
        }

        if (rules.IsAlreadyScanned)
        {
            return Result.Failure<SessionCheckInResponse>(
                Error.Conflict("Ticket.AlreadyScanned", "This ticket was already scanned."));
        }

        if (!rules.IsSessionStarted)
        {
            return Result.Failure<SessionCheckInResponse>(
                Error.Conflict("Session.NotStarted", "Session is not started yet."));
        }

        if (rules.IsSessionEnded)
        {
            return Result.Failure<SessionCheckInResponse>(
                Error.Conflict("Session.AlreadyEnded", "Session already ended."));
        }

        var checkIn = new SessionCheckIn
        {
            RegistrationId = rules.RegistrationId,
            SessionId = request.SessionId,
            CheckedInAt = now
        };
        context.SessionCheckIns.Add(checkIn);

        if (!rules.HasConferenceCheckIn)
        {
            context.ConferenceCheckIns.Add(new ConferenceCheckIn
            {
                RegistrationId = rules.RegistrationId,
                CheckedInAt = now
            });
        }

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            var scannedConcurrently = await context.SessionCheckIns
                .AnyAsync(c => c.SessionId == request.SessionId && c.RegistrationId == rules.RegistrationId, cancellationToken);

            if (!scannedConcurrently)
            {
                throw;
            }

            return Result.Failure<SessionCheckInResponse>(
                Error.Conflict("Ticket.AlreadyScanned", "This ticket was already scanned."));
        }
        return Result.Success(new SessionCheckInResponse(
            checkIn.Id,
            checkIn.RegistrationId,
            checkIn.SessionId,
            rules.FullName,
            checkIn.CheckedInAt
        ));

    }
}
