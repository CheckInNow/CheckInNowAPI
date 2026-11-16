using Application.Abstractions.Messaging;
using Domain.Common;

namespace Application.Features.Registrations.GetAll;

public sealed record GetAllRegistrationsQuery(Guid ConferenceId, int Page = 1, int PageSize = 10)
    : IQuery<PagedResult<RegistrationListItemResponse>>;

public sealed record RegistrationListItemResponse(
    Guid Id,
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    DateTimeOffset CreatedAt);
