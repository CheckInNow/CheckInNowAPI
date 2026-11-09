using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Features.Conferences.Get;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Conferences.GetAll;

public sealed class GetAllConferencesQueryHandler(IAppDbContext dbContext) : IQueryHandler<GetAllConferencesQuery, PagedResult<ConferenceListItemResponse>>
{
    public async Task<Result<PagedResult<ConferenceListItemResponse>>> Handle(GetAllConferencesQuery query, CancellationToken cancellationToken = default)
    {
        var totalCount = await dbContext.Conferences
            .Where(c => c.DeletedAt == null)
            .CountAsync(cancellationToken);

        var items = await dbContext.Conferences
            .Where(c => c.DeletedAt == null)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(c => new ConferenceListItemResponse(c.Id, c.Name, c.StartDate,
                c.EndDate, c.Location, c.ParticipantLimit))
            .ToListAsync(cancellationToken);

        var pagedResult = new PagedResult<ConferenceListItemResponse>(items, totalCount, query.Page, query.PageSize);
        return Result.Success(pagedResult);
    }
}
