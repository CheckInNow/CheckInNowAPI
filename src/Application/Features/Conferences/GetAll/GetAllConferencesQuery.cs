
using Application.Abstractions.Messaging;
using Application.Features.Todos.Get;
using Domain.Common;

namespace Application.Features.Conferences.GetAll;

public sealed record GetAllConferencesQuery(int Page = 1, int PageSize = 10) : IQuery<PagedResult<ConferenceListItemResponse>>;

public sealed record ConferenceListItemResponse(
    Guid Id, string Name, DateTimeOffset StartDate, DateTimeOffset EndDate,
    string Location, int ParticipantLimit);
