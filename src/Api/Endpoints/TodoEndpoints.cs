using Api.Extensions;
using Api.Requests.Todos;
using Application.Features.Todos.Complete;
using Application.Features.Todos.Create;
using Application.Features.Todos.Delete;
using Application.Features.Todos.Get;
using Application.Features.Todos.GetAll;
using Application.Features.Todos.Update;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;
public static class TodoEndpoints
{
    public static void MapTodoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/todos")
            .WithTags("Todos")
            .RequireAuthorization();

        group.MapGet("/", GetAll)
            .WithName("GetAllTodos")
            .WithSummary("Get all todos with pagination");

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetTodoById")
            .WithSummary("Get a todo by ID");

        group.MapPost("/", Create)
            .WithName("CreateTodo")
            .WithSummary("Create a new todo");

        group.MapPut("/{id:guid}", Update)
            .WithName("UpdateTodo")
            .WithSummary("Update an existing todo");

        group.MapPatch("/{id:guid}/complete", Complete)
            .WithName("CompleteTodo")
            .WithSummary("Mark a todo as completed");

        group.MapDelete("/{id:guid}", Delete)
            .WithName("DeleteTodo")
            .WithSummary("Delete a todo");
    }

    private static async Task<IResult> GetAll(
        int? page,
        int? pageSize,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetAllTodosQuery(page ?? 1, pageSize ?? 10);
        var result = await sender.Send(query, cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> GetById(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTodoQuery(id), cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> Create(
        [FromBody] CreateTodoRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateTodoCommand(request.Title, request.Description);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? TypedResults.CreatedAtRoute(result.Value, "GetTodoById", new { id = result.Value!.Id })
            : result.ToProblemDetails();
    }

    private static async Task<IResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateTodoRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTodoCommand(id, request.Title, request.Description);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblemDetails();
    }

    private static async Task<IResult> Complete(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CompleteTodoCommand(id), cancellationToken);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblemDetails();
    }

    private static async Task<IResult> Delete(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteTodoCommand(id), cancellationToken);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblemDetails();
    }
}
