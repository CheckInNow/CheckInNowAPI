using Application.Abstractions.Data;
using Application.Abstractions.Identity;
using Application.Abstractions.Messaging;
using Application.Features.Conferences.GetAll;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Conferences.GetMy;

public sealed class GetMyConferencesQueryHandler(IAppDbContext dbContext, ICurrentUser currentUser)
    : IQueryHandler<GetMyConferencesQuery, PagedResult<ConferenceListItemResponse>>
{
    public async Task<Result<PagedResult<ConferenceListItemResponse>>> Handle(GetMyConferencesQuery query, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
        {
            return Result.Failure<PagedResult<ConferenceListItemResponse>>(Error.Forbidden("Auth.Forbidden", "Current user could not be determined."));
        }

        var organizerId = currentUser.UserId.Value;
        var conferences = dbContext.Conferences
            .Where(c => c.OrganizerId == organizerId && c.DeletedAt == null);

        var totalCount = await conferences.CountAsync(cancellationToken);

        var items = await conferences
            .OrderBy(c => c.StartDate)
            .ThenBy(c => c.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(c => new ConferenceListItemResponse(c.Id, c.Name, c.StartDate,
                c.EndDate, c.Location, c.ParticipantLimit))
            .ToListAsync(cancellationToken);

        var pagedResult = new PagedResult<ConferenceListItemResponse>(items, totalCount, query.Page, query.PageSize);
        return Result.Success(pagedResult);
    }
}
