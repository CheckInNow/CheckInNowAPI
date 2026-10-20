using Api.Extensions;
using Application.Features.Identity.Login;
using Application.Features.Identity.RefreshToken;
using Application.Features.Identity.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;
public static class IdentityEndpoints
{
    public static void MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/identity")
            .WithTags("Identity");

        group.MapPost("/register", Register)
            .WithName("Register")
            .WithSummary("Register a new user");

        group.MapPost("/login", Login)
            .WithName("Login")
            .WithSummary("Login with email and password");

        group.MapPost("/refresh", Refresh)
            .WithName("RefreshToken")
            .WithSummary("Refresh an expired access token");
    }

    private static async Task<IResult> Register(
        [FromBody] RegisterCommand command,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? TypedResults.Ok() : result.ToProblemDetails();
    }

    private static async Task<IResult> Login(
        [FromBody] LoginCommand command,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> Refresh(
        [FromBody] RefreshTokenCommand command,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }
}
