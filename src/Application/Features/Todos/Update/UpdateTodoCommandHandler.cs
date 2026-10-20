
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Common;

namespace Application.Features.Todos.Update;
public sealed class UpdateTodoCommandHandler(IAppDbContext dbContext) : ICommandHandler<UpdateTodoCommand>
{
    public async Task<Result> Handle(UpdateTodoCommand command, CancellationToken cancellationToken = default)
    {
        var todo = await dbContext.Todos.FindAsync([command.Id], cancellationToken);
        if (todo is null)
            return Result.Failure(Error.NotFound("Todo.NotFound", $"Todo with ID '{command.Id}' was not found."));

        todo.Title = command.Title;
        todo.Description = command.Description;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
