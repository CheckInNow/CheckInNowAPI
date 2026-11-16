using Application.Abstractions.Messaging;

namespace Application.Features.Registrations.Get;

public sealed record GetRegistrationQuery(Guid ConferenceId, Guid RegistrationId) : IQuery<RegistrationDetailResponse>;

public sealed record RegistrationDetailResponse(Guid Id, Guid ConferenceId, Guid UserId, DateTimeOffset CreatedAt);
