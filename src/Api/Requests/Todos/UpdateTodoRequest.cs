namespace Api.Requests.Todos;

public sealed record UpdateTodoRequest(string Title, string? Description);
