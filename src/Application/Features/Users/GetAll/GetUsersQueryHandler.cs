using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Users.GetAll;

public sealed class GetUsersQueryHandler(IAppDbContext dbContext)
    : IQueryHandler<GetUsersQuery, PagedResult<UserListItemResponse>>
{
    public async Task<Result<PagedResult<UserListItemResponse>>> Handle(GetUsersQuery query, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var users = dbContext.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            users = users.Where(u =>
                u.Email!.ToLower().Contains(term) ||
                u.FirstName.ToLower().Contains(term) ||
                u.LastName.ToLower().Contains(term) ||
                (u.FirstName + " " + u.LastName).ToLower().Contains(term));
        }

        var totalCount = await users.CountAsync(cancellationToken);

        var items = await users
            .OrderByDescending(u => u.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(u => new UserListItemResponse(
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
                u.LockoutEnabled && u.LockoutEnd != null && u.LockoutEnd > now))
            .ToListAsync(cancellationToken);

        return Result.Success(new PagedResult<UserListItemResponse>(items, totalCount, query.Page, query.PageSize));
    }
}
