using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Common;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Application.Abstractions.Identity;

namespace Application.Features.CheckIns.Conference;

public sealed class ConferenceCheckInCommandHandler(IAppDbContext context, ICurrentUser currentUser) : ICommandHandler<ConferenceCheckInCommand, ConferenceCheckInResponse>
{
    public async Task<Result<ConferenceCheckInResponse>> Handle(ConferenceCheckInCommand request, CancellationToken cancellationToken)
    {
        var rules = await context.Registrations
            .Where(r => r.TicketCode == request.TicketCode
                && r.ConferenceId == request.ConferenceId
                && r.Conference.DeletedAt == null)
            .Select(r => new
            {
                RegistrationId = r.Id,
                FullName = r.User.FirstName + " " + r.User.LastName,
                IsAlreadyScanned = context.ConferenceCheckIns.Any(c => c.RegistrationId == r.Id),
                IsUserScanningAllowed = r.Conference.OrganizerId == currentUser.UserId,
                IsConferenceStarted = r.Conference.StartDate < DateTimeOffset.UtcNow,
                IsConferenceEnded = r.Conference.EndDate < DateTimeOffset.UtcNow
            }).FirstOrDefaultAsync(cancellationToken);

        if (rules is null)
        {
            return Result.Failure<ConferenceCheckInResponse>(
                Error.NotFound("Ticket.Invalid", "Invalid ticket."));
        }

        if (!rules.IsUserScanningAllowed)
        {
            return Result.Failure<ConferenceCheckInResponse>(
                Error.Forbidden("CheckIn.Forbidden", "You are not allowed to scan tickets."));
        }

        if (rules.IsAlreadyScanned)
        {
            return Result.Failure<ConferenceCheckInResponse>(
                Error.Conflict("Ticket.AlreadyScanned", "This ticket was already scanned."));
        }

        if (!rules.IsConferenceStarted)
        {
            return Result.Failure<ConferenceCheckInResponse>(
                Error.Conflict("Conference.NotStarted", "Conference is not started yet."));
        }

        if (rules.IsConferenceEnded)
        {
            return Result.Failure<ConferenceCheckInResponse>(
                Error.Conflict("Conference.AlreadyEnded", "Conference already ended."));
        }

        var checkIn = new ConferenceCheckIn
        {
            RegistrationId = rules.RegistrationId,
            CheckedInAt = DateTimeOffset.UtcNow,
        };

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            var scannedConcurrently = await context.ConferenceCheckIns
                .AnyAsync(c => c.RegistrationId == rules.RegistrationId, cancellationToken);

            if (!scannedConcurrently)
            {
                throw;
            }

            return Result.Failure<ConferenceCheckInResponse>(
                Error.Conflict("Ticket.AlreadyScanned", "This ticket was already scanned."));
        }

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success(new ConferenceCheckInResponse(
            checkIn.Id,
            checkIn.RegistrationId,
            rules.FullName,
            checkIn.CheckedInAt
        ));
    }
}
