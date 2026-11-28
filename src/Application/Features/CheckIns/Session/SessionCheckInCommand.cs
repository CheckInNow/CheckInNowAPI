using Application.Abstractions.Messaging;

namespace Application.Features.CheckIns.Session;

public sealed record SessionCheckInCommand(Guid ConferenceId, Guid SessionId, Guid TicketCode) : ICommand<SessionCheckInResponse>;

public sealed record SessionCheckInResponse(Guid Id, Guid RegistrationId, Guid SessionId, string Name, DateTimeOffset CheckInTime);
