using Application.Abstractions.Data;
using Application.Abstractions.Identity;
using Application.Abstractions.Messaging;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Registrations.GetTicket;

public sealed class GetTicketQueryHandler(IAppDbContext context, ICurrentUser currentUser) : IQueryHandler<GetTicketQuery, RegistrationTicketResponse>
{
    public async Task<Result<RegistrationTicketResponse>> Handle(GetTicketQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId;

        var ticket = await context.Registrations
            .Where(r => r.Id == request.RegistrationId && r.UserId == userId
            && r.Conference.DeletedAt == null && r.ConferenceId == request.ConferenceId)
            .Select(r => new RegistrationTicketResponse(
                r.ConferenceId,
                r.Conference.Name,
                r.TicketCode
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (ticket is null)
        {
            return Result.Failure<RegistrationTicketResponse>(Error.NotFound("Registration.NotFound", "Registration not found."));
        }

        return Result.Success(ticket);
    }
}
