
using Application.Abstractions.Messaging;
using Domain.Common;

namespace Application.Features.Todos.Update;
public sealed record UpdateTodoCommand(Guid Id, string Title, string? Description) : ICommand;
