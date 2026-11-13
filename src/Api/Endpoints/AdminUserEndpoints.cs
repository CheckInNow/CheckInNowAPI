using Api.Extensions;
using Application.Features.Users.AddOrganizerRole;
using Application.Features.Users.Ban;
using Application.Features.Users.Get;
using Application.Features.Users.GetAll;
using Application.Features.Users.RemoveOrganizerRole;
using Application.Features.Users.Unban;
using Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public static class AdminUserEndpoints
{
    public static void MapAdminUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/users")
            .WithTags("Admin - Users")
            .RequireAuthorization("Admin")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/", GetUsers)
            .WithName("GetUsers")
            .WithSummary("Get paginated users, optionally filtered by email or name.")
            .Produces<PagedResult<UserListItemResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:guid}", GetUserById)
            .WithName("GetUserById")
            .WithSummary("Get user details with roles.")
            .Produces<UserDetailResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/roles/organizer", AddOrganizerRole)
            .WithName("AddOrganizerRole")
            .WithSummary("Add the Organizer role to a user (the User role is kept).")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}/roles/organizer", RemoveOrganizerRole)
            .WithName("RemoveOrganizerRole")
            .WithSummary("Remove the Organizer role from a user.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/ban", BanUser)
            .WithName("BanUser")
            .WithSummary("Ban a user (permanent account lockout). No data is deleted.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/unban", UnbanUser)
            .WithName("UnbanUser")
            .WithSummary("Unban a user (clear the account lockout).")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> GetUsers(
        [AsParameters] GetUsersQuery query,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> GetUserById(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetUserQuery(id), cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> AddOrganizerRole(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new AddOrganizerRoleCommand(id), cancellationToken);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblemDetails();
    }

    private static async Task<IResult> RemoveOrganizerRole(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RemoveOrganizerRoleCommand(id), cancellationToken);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblemDetails();
    }

    private static async Task<IResult> BanUser(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new BanUserCommand(id), cancellationToken);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblemDetails();
    }

    private static async Task<IResult> UnbanUser(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UnbanUserCommand(id), cancellationToken);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblemDetails();
    }
}
