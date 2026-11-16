using Application.Abstractions.Data;
using Application.Abstractions.Identity;
using Application.Abstractions.Messaging;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Registrations.GetAll;

public sealed class GetAllRegistrationsQueryHandler(IAppDbContext dbContext, ICurrentUser currentUser)
    : IQueryHandler<GetAllRegistrationsQuery, PagedResult<RegistrationListItemResponse>>
{
    public async Task<Result<PagedResult<RegistrationListItemResponse>>> Handle(GetAllRegistrationsQuery query, CancellationToken cancellationToken = default)
    {
        var conference = await dbContext.Conferences
            .Where(c => c.Id == query.ConferenceId && c.DeletedAt == null)
            .Select(c => new { c.OrganizerId })
            .FirstOrDefaultAsync(cancellationToken);

        if (conference is null)
        {
            return Result.Failure<PagedResult<RegistrationListItemResponse>>(
                Error.NotFound("Conference.NotFound", $"Conference with ID '{query.ConferenceId}' was not found."));
        }

        if (currentUser.UserId != conference.OrganizerId)
        {
            return Result.Failure<PagedResult<RegistrationListItemResponse>>(
                Error.Forbidden("Registration.Forbidden", "Only the conference owner can view its registrations."));
        }

        var registrations = dbContext.Registrations
            .Where(r => r.ConferenceId == query.ConferenceId);

        var totalCount = await registrations.CountAsync(cancellationToken);

        var items = await registrations
            .OrderBy(r => r.CreatedAt)
            .ThenBy(r => r.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(r => new RegistrationListItemResponse(
                r.Id,
                r.UserId,
                r.User.FirstName,
                r.User.LastName,
                r.User.Email ?? string.Empty,
                r.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result.Success(new PagedResult<RegistrationListItemResponse>(items, totalCount, query.Page, query.PageSize));
    }
}
