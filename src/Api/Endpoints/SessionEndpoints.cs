using Api.Extensions;
using Api.Requests.Sessions;
using Application.Features.Sessions.Create;
using Application.Features.Sessions.Delete;
using Application.Features.Sessions.Get;
using Application.Features.Sessions.GetAll;
using Application.Features.Sessions.Update;
using Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public static class SessionEndpoints
{
    public static void MapSessionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/conferences/{conferenceId:guid}/sessions")
            .WithTags("Sessions");

        group.MapGet("/", GetSessions)
            .WithName("GetSessions")
            .WithSummary("Get paginated sessions of a conference.")
            .Produces<PagedResult<SessionListItemResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .AllowAnonymous();

        group.MapGet("/{sessionId:guid}", GetSession)
            .WithName("GetSession")
            .WithSummary("Get a session of a conference.")
            .Produces<SessionDetailResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .AllowAnonymous();

        group.MapPost("/", AddSession)
            .WithName("AddSession")
            .WithSummary("Add a session to a conference. Only the conference owner can do this.")
            .Produces<CreateSessionResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization("Organizer");

        group.MapPatch("/{sessionId:guid}", UpdateSession)
            .WithName("UpdateSession")
            .WithSummary("Partially update a session. Only the conference owner can do this.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization("Organizer");

        group.MapDelete("/{sessionId:guid}", DeleteSession)
            .WithName("DeleteSession")
            .WithSummary("Delete (soft) a session. Only the conference owner can do this.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization("Organizer");
    }

    private static async Task<IResult> AddSession(
        [FromRoute] Guid conferenceId,
        [FromBody] CreateSessionRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateSessionCommand(
            conferenceId,
            request.Title,
            request.Description,
            request.StartTime,
            request.EndTime,
            request.RoomId,
            request.SpeakerId);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? TypedResults.CreatedAtRoute(result.Value, "GetSession", new { conferenceId, sessionId = result.Value!.Id })
            : result.ToProblemDetails();
    }

    private static async Task<IResult> GetSessions(
        [AsParameters] GetAllSessionsQuery query,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> GetSession(
        [FromRoute] Guid conferenceId,
        [FromRoute] Guid sessionId,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSessionQuery(conferenceId, sessionId), cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> UpdateSession(
        [FromRoute] Guid conferenceId,
        [FromRoute] Guid sessionId,
        [FromBody] UpdateSessionRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSessionCommand(
            conferenceId,
            sessionId,
            request.Title,
            request.Description,
            request.StartTime,
            request.EndTime,
            request.RoomId,
            request.SpeakerId);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblemDetails();
    }

    private static async Task<IResult> DeleteSession(
        [FromRoute] Guid conferenceId,
        [FromRoute] Guid sessionId,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteSessionCommand(conferenceId, sessionId), cancellationToken);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblemDetails();
    }
}
