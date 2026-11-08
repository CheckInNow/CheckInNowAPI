using Api.Extensions;
using Application.Features.Conferences.Get;
using Application.Features.Conferences.GetAll;
using Application.Features.Identity.Register;
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
            .AllowAnonymous();

        group.MapGet("/{id:guid}", GetConferenceById)
            .WithName("GetConferenceById")
            .WithSummary("Get a conference by ID.")
            .AllowAnonymous();
    }

    private static async Task<IResult> GetConferenceById(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetConferenceQuery(id);
        var result = await sender.Send(query, cancellationToken);
        return result.IsSuccess ? TypedResults.Ok() : result.ToProblemDetails();
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
