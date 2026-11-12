using Api.Extensions;
using Api.Requests.Speakers;
using Application.Features.Speakers.Create;
using Application.Features.Speakers.Get;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public static class SpeakerEndpoints
{
    public static void MapSpeakerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/conferences/{conferenceId:guid}/speakers")
            .WithTags("Speakers");

        group.MapGet("/", GetSpeakers)
            .WithName("GetSpeakers")
            .WithSummary("Get the speakers of a conference.")
            .Produces<List<SpeakerResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .AllowAnonymous();

        group.MapPost("/", CreateSpeaker)
            .WithName("CreateSpeaker")
            .WithSummary("Add a speaker to a conference. Only the conference owner can do this.")
            .Produces<CreateSpeakerResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization("Organizer");
    }

    private static async Task<IResult> GetSpeakers(
        [FromRoute] Guid conferenceId,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSpeakersQuery(conferenceId), cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> CreateSpeaker(
        [FromRoute] Guid conferenceId,
        [FromBody] CreateSpeakerRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateSpeakerCommand(conferenceId, request.FirstName, request.LastName, request.Bio);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? TypedResults.Created($"/api/conferences/{conferenceId}/speakers/{result.Value!.Id}", result.Value)
            : result.ToProblemDetails();
    }
}
