using Api.Extensions;
using Application.Features.Registrations.Create;
using Application.Features.Registrations.Delete;
using Application.Features.Registrations.Get;
using Application.Features.Registrations.GetAll;
using Application.Features.Registrations.GetTicket;
using Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public static class RegistrationEndpoints
{
    public static void MapRegistrationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/conferences/{conferenceId:guid}/registrations")
            .WithTags("Registrations");

        group.MapGet("/", GetRegistrations)
            .WithName("GetRegistrations")
            .WithSummary("Get paginated registrations of a conference. Only the conference owner can do this.")
            .Produces<PagedResult<RegistrationListItemResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization("Organizer");

        group.MapGet("/{registrationId:guid}", GetRegistration)
            .WithName("GetRegistration")
            .WithSummary("Get a registration. Only the registered user or the conference owner can do this.")
            .Produces<RegistrationDetailResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        group.MapPost("/", CreateRegistration)
            .WithName("CreateRegistration")
            .WithSummary("Register the current user for a conference. Requires a confirmed email.")
            .Produces<CreateRegistrationResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization();

        group.MapDelete("/{registrationId:guid}", DeleteRegistration)
            .WithName("DeleteRegistration")
            .WithSummary("Cancel (hard delete) a registration before the conference starts. Only the registered user can do this.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization();

        group.MapGet("/{registrationId:guid}/ticket", GetRegistrationTicket)
            .WithName("GetRegistrationTicket")
            .WithSummary("Get the ticket for a registration. Only the registered user can do this.")
            .Produces<RegistrationTicketResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();
    }

    private static async Task<IResult> GetRegistrations(
        [AsParameters] GetAllRegistrationsQuery query,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> GetRegistration(
        [FromRoute] Guid conferenceId,
        [FromRoute] Guid registrationId,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetRegistrationQuery(conferenceId, registrationId), cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> CreateRegistration(
        [FromRoute] Guid conferenceId,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateRegistrationCommand(conferenceId), cancellationToken);
        return result.IsSuccess
            ? TypedResults.CreatedAtRoute(result.Value, "GetRegistration", new { conferenceId, registrationId = result.Value!.Id })
            : result.ToProblemDetails();
    }

    private static async Task<IResult> DeleteRegistration(
        [FromRoute] Guid conferenceId,
        [FromRoute] Guid registrationId,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteRegistrationCommand(conferenceId, registrationId), cancellationToken);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblemDetails();
    }

    private static async Task<IResult> GetRegistrationTicket(
        [FromRoute] Guid conferenceId,
        [FromRoute] Guid registrationId,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTicketQuery(conferenceId, registrationId), cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }
}
