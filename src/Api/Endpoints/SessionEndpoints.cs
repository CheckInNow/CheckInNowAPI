using Api.Extensions;
using Api.Requests.Sessions;
using Application.Features.Sessions.Create;
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
            .AllowAnonymous();

        group.MapGet("/{sessionId:guid}", GetSession)
            .WithName("GetSession")
            .WithSummary("Get a session of a conference.")
            .AllowAnonymous();

        group.MapPost("/", AddSession)
            .WithName("AddSession")
            .WithSummary("Add a session to a conference. Only the conference owner can do this.")
            .RequireAuthorization("Organizer");

        group.MapDelete("/", DeleteSession)
            .WithName("DeleteSession")
            .WithSummary("Delete a session from a conference. Only the conference owner can do this.")
            .RequireAuthorization("Organizer");

        group.MapPatch("/", UpdateSession)
            .WithName("UpdateSession")
            .WithSummary("Update a session in a conference. Only the conference owner can do this.")
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
        return result.IsSuccess ? TypedResults.Ok() : result.ToProblemDetails();
    }

    private static async Task<IResult> GetSessions(
        [FromRoute] Guid conferenceId,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private static async Task<IResult> GetSession(
    [FromRoute] Guid conferenceId,
    [FromServices] ISender sender,
    CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private static async Task<IResult> DeleteSession(
        [FromRoute] Guid conferenceId,
        [FromServices] ISender sender,
        CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }

    private static async Task UpdateSession(
        [FromRoute] Guid conferenceId,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

}
