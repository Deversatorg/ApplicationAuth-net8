using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ApplicationAuth.SharedModels.ResponseModels;
using ApplicationAuth.Features.Account.Login;
using MediatR;

namespace ApplicationAuth.Features.Account.AdminLogin;

public static class AdminLoginEndpoint
{
    public static void MapAdminLoginEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/admin-sessions", async (
            [FromBody] AdminLoginRequest request,
            [FromServices] IMediator mediator) =>
        {
            var response = await mediator.Send(request);
            return Results.Json(new JsonResponse<LoginResponse>(response), statusCode: StatusCodes.Status201Created);
        })
        .AllowAnonymous()
        .WithTags("Admin Sessions")
        .WithSummary("Admin login")
        .WithDescription("Admin login");
    }
}
