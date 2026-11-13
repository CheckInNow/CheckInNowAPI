using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Users.Get;

public sealed class GetUserQueryHandler(IAppDbContext dbContext, TimeProvider timeProvider)
    : IQueryHandler<GetUserQuery, UserDetailResponse>
{
    public async Task<Result<UserDetailResponse>> Handle(GetUserQuery query, CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();

        var user = await dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == query.Id)
            .Select(u => new UserDetailResponse(
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email!,
                dbContext.UserRoles
                    .Where(ur => ur.UserId == u.Id)
                    .Join(dbContext.Roles, ur => ur.RoleId, r => r.Id, (_, r) => r.Name!)
                    .OrderBy(name => name)
                    .ToList(),
                u.EmailConfirmed,
                u.LockoutEnabled && u.LockoutEnd != null && u.LockoutEnd > now,
                u.LockoutEnd,
                u.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        return user is null
            ? Result.Failure<UserDetailResponse>(UserErrors.NotFound(query.Id))
            : Result.Success(user);
    }
}
