using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ApplicationAuth.Features.Account.Login;
using ApplicationAuth.SharedModels.ResponseModels;
using MediatR;
using System.Threading.Tasks;

namespace ApplicationAuth.Features.Account.SocialAuth;

public static class SocialAuthEndpoints
{
    public static void MapSocialAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/sessions");

        group.MapPost("/google", async ([FromBody] GoogleLoginRequest model, [FromServices] IMediator mediator) =>
        {
            var response = await mediator.Send(model);
            return Results.Ok(new JsonResponse<LoginResponse>(response));
        })
        .AllowAnonymous()
        .WithTags("Account")
        .WithSummary("Login via Google")
        .WithDescription("Authenticates a user via a Google id_token and returns JWT access tokens.");

        group.MapPost("/apple", async ([FromBody] AppleLoginRequest model, [FromServices] IMediator mediator) =>
        {
            var response = await mediator.Send(model);
            return Results.Ok(new JsonResponse<LoginResponse>(response));
        })
        .AllowAnonymous()
        .WithTags("Account")
        .WithSummary("Login via Apple")
        .WithDescription("Authenticates a user via an Apple identity_token and returns JWT access tokens.");
    }
}
