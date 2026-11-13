using Application.Abstractions.Messaging;
using Domain.Common;

namespace Application.Features.Users.GetAll;

public sealed record GetUsersQuery(int Page = 1, int PageSize = 10, string? Search = null)
    : IQuery<PagedResult<UserListItemResponse>>;

public sealed record UserListItemResponse(
    Guid Id, string FirstName, string LastName, string Email,
    List<string> Roles, bool EmailConfirmed, bool IsLockedOut);
