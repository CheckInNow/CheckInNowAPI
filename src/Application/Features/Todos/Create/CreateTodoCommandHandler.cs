
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Common;
using Domain.Entities;

namespace Application.Features.Todos.Create;
public sealed class CreateTodoCommandHandler(IAppDbContext dbContext) : ICommandHandler<CreateTodoCommand, CreateTodoResponse>
{
    public async Task<Result<CreateTodoResponse>> Handle(CreateTodoCommand command, CancellationToken cancellationToken = default)
    {
        var todo = new TodoItem
        {
            Title = command.Title,
            Description = command.Description
        };

        dbContext.Todos.Add(todo);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new CreateTodoResponse(todo.Id, todo.Title, todo.Description);
        return Result.Success(response);
    }
}
