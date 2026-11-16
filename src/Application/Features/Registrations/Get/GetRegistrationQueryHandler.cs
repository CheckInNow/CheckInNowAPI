using Application.Abstractions.Data;
using Application.Abstractions.Identity;
using Application.Abstractions.Messaging;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Registrations.Get;

public sealed class GetRegistrationQueryHandler(IAppDbContext dbContext, ICurrentUser currentUser)
    : IQueryHandler<GetRegistrationQuery, RegistrationDetailResponse>
{
    public async Task<Result<RegistrationDetailResponse>> Handle(GetRegistrationQuery query, CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId;
        if (userId is null)
        {
            return Result.Failure<RegistrationDetailResponse>(
                Error.Forbidden("Auth.Forbidden", "Current user could not be determined."));
        }

        var registration = await dbContext.Registrations
            .Where(r => r.Id == query.RegistrationId
                && r.ConferenceId == query.ConferenceId
                && r.Conference.DeletedAt == null)
            .Select(r => new
            {
                r.Id,
                r.ConferenceId,
                r.UserId,
                r.CreatedAt,
                r.Conference.OrganizerId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (registration is null)
        {
            return Result.Failure<RegistrationDetailResponse>(
                Error.NotFound("Registration.NotFound", $"Registration with ID '{query.RegistrationId}' was not found."));
        }

        if (registration.UserId != userId.Value && registration.OrganizerId != userId.Value)
        {
            return Result.Failure<RegistrationDetailResponse>(
                Error.Forbidden("Registration.Forbidden", "Only the registered user or the conference owner can view this registration."));
        }

        return Result.Success(new RegistrationDetailResponse(
            registration.Id,
            registration.ConferenceId,
            registration.UserId,
            registration.CreatedAt));
    }
}
