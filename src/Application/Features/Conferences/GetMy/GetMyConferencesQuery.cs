using Application.Abstractions.Messaging;
using Application.Features.Conferences.GetAll;
using Domain.Common;

namespace Application.Features.Conferences.GetMy;

public sealed record GetMyConferencesQuery(int Page = 1, int PageSize = 10) : IQuery<PagedResult<ConferenceListItemResponse>>;
