
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Common;

namespace Application.Features.Todos.Complete;
public sealed class CompleteTodoCommandHandler(IAppDbContext dbContext) : ICommandHandler<CompleteTodoCommand>
{
    public async Task<Result> Handle(CompleteTodoCommand command, CancellationToken cancellationToken = default)
    {
        var todo = await dbContext.Todos.FindAsync([command.Id], cancellationToken);
        if (todo is null)
            return Result.Failure(Error.NotFound("Todo.NotFound", $"Todo with ID '{command.Id}' was not found."));

        todo.MarkAsCompleted();
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
