using Api.Extensions;
using Api.Requests.Conferences;
using Application.Features.Conferences.Create;
using Application.Features.Conferences.Delete;
using Application.Features.Conferences.Get;
using Application.Features.Conferences.GetAll;
using Application.Features.Conferences.Update;
using Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public static class ConferenceEndpoints
{
    public static void MapConferenceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/conferences")
            .WithTags("Conferences");

        group.MapGet("/", GetConferences)
            .WithName("GetConferences")
            .WithSummary("Get paginated conferences.")
            .Produces<PagedResult<ConferenceListItemResponse>>(StatusCodes.Status200OK)
            .AllowAnonymous();

        group.MapGet("/{id:guid}", GetConferenceById)
            .WithName("GetConferenceById")
            .WithSummary("Get a conference by ID.")
            .Produces<ConferenceDetailResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .AllowAnonymous();

        group.MapPost("/", CreateConference)
            .WithName("CreateConference")
            .WithSummary("Create a conference. The current user becomes the owner.")
            .Produces<ConferenceDetailResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .RequireAuthorization("Organizer");

        group.MapPatch("/{id:guid}", UpdateConference)
            .WithName("UpdateConference")
            .WithSummary("Partially update a conference. Only the owner can do this.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization("Organizer");

        group.MapDelete("/{id:guid}", DeleteConference)
            .WithName("DeleteConference")
            .WithSummary("Delete (soft) a conference. Only the owner can do this.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization("Organizer");
    }

    private static async Task<IResult> GetConferenceById(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetConferenceQuery(id);
        var result = await sender.Send(query, cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }


    private static async Task<IResult> CreateConference(
        [FromBody] CreateConferenceCommand command,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess
        ? TypedResults.CreatedAtRoute(result.Value, "GetConferenceById", new { id = result.Value!.Id })
        : result.ToProblemDetails();
    }

    private static async Task<IResult> UpdateConference(
        [FromRoute] Guid id,
        [FromBody] UpdateConferenceRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateConferenceCommand(
            id, request.Name, request.Description, request.StartDate,
            request.EndDate, request.Location, request.ParticipantLimit);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblemDetails();
    }

    private static async Task<IResult> DeleteConference(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteConferenceCommand(id), cancellationToken);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblemDetails();
    }

    private static async Task<IResult> GetConferences(
        [AsParameters] GetAllConferencesQuery query,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }
}
