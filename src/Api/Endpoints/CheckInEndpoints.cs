using Api.Extensions;
using Api.Requests.CheckIns;
using Application.Features.CheckIns.Conference;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public static class CheckInEndpoints
{
    public static void MapCheckInEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/conferences/{conferenceId:guid}/check-ins")
            .WithTags("CheckIns");

        group.MapPost("/", CheckInConference)
            .WithName("CheckInConference")
            .WithSummary("Check in a participant by ticket code. Only the conference owner can do this, only while the conference is running.")
            .Produces<ConferenceCheckInResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization("Organizer");
    }

    private static async Task<IResult> CheckInConference(
        [FromRoute] Guid conferenceId,
        [FromBody] ConferenceCheckInRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ConferenceCheckInCommand(conferenceId, request.TicketCode), cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }
}
