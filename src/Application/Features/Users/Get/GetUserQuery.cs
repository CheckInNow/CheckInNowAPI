using Application.Abstractions.Messaging;

namespace Application.Features.Users.Get;

public sealed record GetUserQuery(Guid Id) : IQuery<UserDetailResponse>;

public sealed record UserDetailResponse(
    Guid Id, string FirstName, string LastName, string Email,
    List<string> Roles, bool EmailConfirmed, bool IsLockedOut,
    DateTimeOffset? LockoutEnd, DateTimeOffset CreatedAt);
