
using Application.Abstractions.Messaging;
using Domain.Common;

namespace Application.Features.Todos.Delete;
public sealed record DeleteTodoCommand(Guid Id) : ICommand;
