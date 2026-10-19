
using Application.Abstractions.Messaging;
using Domain.Common;

namespace Application.Features.Todos.Complete;
public sealed record CompleteTodoCommand(Guid Id) : ICommand;
