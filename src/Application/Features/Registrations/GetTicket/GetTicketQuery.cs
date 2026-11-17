using Application.Abstractions.Messaging;

namespace Application.Features.Registrations.GetTicket;

public sealed record GetTicketQuery(Guid ConferenceId, Guid RegistrationId) : IQuery<RegistrationTicketResponse>;

public sealed record RegistrationTicketResponse(
    Guid ConferenceId,
    string ConferenceName,
    Guid TicketCode
);
