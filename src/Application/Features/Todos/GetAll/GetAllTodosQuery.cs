
using Application.Abstractions.Messaging;
using Application.Features.Todos.Get;
using Domain.Common;

namespace Application.Features.Todos.GetAll;
public sealed record GetAllTodosQuery(int Page = 1, int PageSize = 10) : IQuery<PagedResult<TodoDetailResponse>>;
