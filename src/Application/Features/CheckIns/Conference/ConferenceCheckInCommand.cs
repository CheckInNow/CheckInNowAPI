using Application.Abstractions.Messaging;

namespace Application.Features.CheckIns.Conference;

public sealed record ConferenceCheckInCommand(Guid ConferenceId, Guid TicketCode) : ICommand;

public sealed record ConferenceCheckInResponse(Guid Id, Guid RegistrationId, string Name, DateTimeOffset CheckInTime)
