
using Application.Abstractions.Messaging;
using Domain.Common;

namespace Application.Features.Todos.Get;
public sealed record GetTodoQuery(Guid Id) : IQuery<TodoDetailResponse>;

public sealed record TodoDetailResponse(Guid Id, string Title, string? Description, bool IsCompleted, DateTimeOffset? CompletedAt, DateTimeOffset CreatedAt);
