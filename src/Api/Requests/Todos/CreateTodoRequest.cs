namespace Api.Requests.Todos;

public sealed record CreateTodoRequest(string Title, string? Description);
