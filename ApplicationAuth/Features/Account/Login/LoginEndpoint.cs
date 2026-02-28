using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ApplicationAuth.SharedModels.ResponseModels;
using MediatR;

namespace ApplicationAuth.Features.Account.Login;

public static class LoginEndpoint
{
    public static void MapLoginEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/sessions", async (
            [FromBody] LoginRequest request,
            [FromServices] IMediator mediator) =>
        {
            var response = await mediator.Send(request);
            return Results.Json(new JsonResponse<LoginResponse>(response), statusCode: StatusCodes.Status201Created);
        })
        .AllowAnonymous()
        .RequireRateLimiting("AuthPolicy")
        .WithSummary("Login User")
        .WithDescription("Login User. 'accessTokenLifetime' - access token life time (sec)");
    }
}
