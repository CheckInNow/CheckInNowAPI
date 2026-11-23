using Application.Abstractions.Messaging;
using Domain.Common;

namespace Application.Features.Registrations.GetMy;

public sealed record GetMyRegistrationsQuery(int Page = 1, int PageSize = 10)
    : IQuery<PagedResult<MyRegistrationListItemResponse>>;

public sealed record MyRegistrationListItemResponse(
    Guid Id,
    Guid ConferenceId,
    string ConferenceName,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string Location,
    DateTimeOffset CreatedAt);
