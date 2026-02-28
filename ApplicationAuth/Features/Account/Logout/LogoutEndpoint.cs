using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ApplicationAuth.SharedModels.ResponseModels;
using ApplicationAuth.Common.Extensions;
using ApplicationAuth.Common.Constants;
using ApplicationAuth.Domain.Entities.Identity;
using MediatR;

namespace ApplicationAuth.Features.Account.Logout;

public static class LogoutEndpoint
{
    public static void MapLogoutEndpoints(this IEndpointRouteBuilder app)
    {
        // Public User Logout
        app.MapDelete("/api/v1/sessions", async (
            ClaimsPrincipal user,
            [FromServices] IMediator mediator) =>
        {
            await mediator.Send(new LogoutRequest(user.GetUserId()));
            return Results.Ok(new JsonResponse<MessageResponseModel>(new("You have been logged out")));
        })
        .RequireAuthorization()
        .WithTags("Sessions")
        .WithSummary("User logout")
        .WithDescription("Clear user tokens");

        // Admin Logout
        app.MapDelete("/api/v1/admin-sessions", async (
            ClaimsPrincipal user,
            [FromServices] IMediator mediator) =>
        {
            await mediator.Send(new LogoutRequest(user.GetUserId()));
            return Results.Ok(new JsonResponse<MessageResponseModel>(new("You have been logged out")));
        })
        .RequireAuthorization(policy => policy.RequireRole(Role.Admin, Role.SuperAdmin))
        .WithTags("Admin Sessions")
        .WithSummary("Clear admin tokens")
        .WithDescription("Clear admin tokens");
    }
}
