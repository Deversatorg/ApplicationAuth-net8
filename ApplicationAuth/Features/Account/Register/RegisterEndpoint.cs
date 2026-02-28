using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ApplicationAuth.SharedModels.ResponseModels;
using MediatR;

namespace ApplicationAuth.Features.Account.Register;

public static class RegisterEndpoint
{
    public static void MapRegisterEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/users", async (
            [FromBody] RegisterRequest request,
            [FromServices] IMediator mediator) =>
        {
            var response = await mediator.Send(request);
            return Results.Json(new JsonResponse<RegisterResponse>(response), statusCode: StatusCodes.Status201Created);
        })
        .AllowAnonymous()
        .RequireRateLimiting("AuthPolicy")
        .WithSummary("Register new user")
        .WithDescription("Register new user");
    }
}
