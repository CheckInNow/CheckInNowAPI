using Application.Abstractions.Data;
using Application.Abstractions.Identity;
using Application.Abstractions.Messaging;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Registrations.GetMy;

public sealed class GetMyRegistrationsQueryHandler(IAppDbContext dbContext, ICurrentUser currentUser)
    : IQueryHandler<GetMyRegistrationsQuery, PagedResult<MyRegistrationListItemResponse>>
{
    public async Task<Result<PagedResult<MyRegistrationListItemResponse>>> Handle(GetMyRegistrationsQuery query, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
        {
            return Result.Failure<PagedResult<MyRegistrationListItemResponse>>(
                Error.Forbidden("Auth.Forbidden", "Current user could not be determined."));
        }

        var userId = currentUser.UserId.Value;
        var registrations = dbContext.Registrations
            .Where(r => r.UserId == userId && r.Conference.DeletedAt == null);

        var totalCount = await registrations.CountAsync(cancellationToken);

        var items = await registrations
            .OrderByDescending(r => r.Conference.StartDate)
            .ThenBy(r => r.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(r => new MyRegistrationListItemResponse(
                r.Id,
                r.ConferenceId,
                r.Conference.Name,
                r.Conference.StartDate,
                r.Conference.EndDate,
                r.Conference.Location,
                r.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result.Success(new PagedResult<MyRegistrationListItemResponse>(items, totalCount, query.Page, query.PageSize));
    }
}
