using Api.Extensions;
using Api.Requests.Rooms;
using Application.Features.Rooms.Create;
using Application.Features.Rooms.Get;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public static class RoomEndpoints
{
    public static void MapRoomEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/conferences/{conferenceId:guid}/rooms")
            .WithTags("Rooms");

        group.MapGet("/", GetRooms)
            .WithName("GetRooms")
            .WithSummary("Get the rooms of a conference.")
            .Produces<List<RoomResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .AllowAnonymous();

        group.MapPost("/", CreateRoom)
            .WithName("CreateRoom")
            .WithSummary("Add a room to a conference. Only the conference owner can do this.")
            .Produces<CreateRoomResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization("Organizer");
    }

    private static async Task<IResult> GetRooms(
        [FromRoute] Guid conferenceId,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetRoomsQuery(conferenceId), cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> CreateRoom(
        [FromRoute] Guid conferenceId,
        [FromBody] CreateRoomRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateRoomCommand(conferenceId, request.Name, request.Capacity);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? TypedResults.Created($"/api/conferences/{conferenceId}/rooms/{result.Value!.Id}", result.Value)
            : result.ToProblemDetails();
    }
}
