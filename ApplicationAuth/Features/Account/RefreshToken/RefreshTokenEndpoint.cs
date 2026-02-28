using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ApplicationAuth.SharedModels.ResponseModels;
using ApplicationAuth.Features.Account.Login;
using ApplicationAuth.Features.Account.Shared;
using ApplicationAuth.Common.Constants;
using ApplicationAuth.Domain.Entities.Identity;
using System.Collections.Generic;
using MediatR;

namespace ApplicationAuth.Features.Account.RefreshToken;

public static class RefreshTokenEndpoint
{
    public static void MapRefreshTokenEndpoints(this IEndpointRouteBuilder app)
    {
        // Public User Refresh
        app.MapPut("/api/v1/sessions", async (
            [FromBody] RefreshTokenRequest request,
            [FromServices] IMediator mediator) =>
        {
            request.AllowedRoles.Add(Role.User);
            var response = await mediator.Send(request);
            return Results.Json(new JsonResponse<TokenResponse>(response), statusCode: StatusCodes.Status201Created);
        })
        .RequireAuthorization(policy => policy.RequireRole(Role.User))
        .WithTags("Sessions")
        .WithSummary("Refresh user's access token")
        .WithDescription("Refresh user's access token");

        // Admin Refresh
        app.MapPut("/api/v1/admin-sessions", async (
            [FromBody] RefreshTokenRequest request,
            [FromServices] IMediator mediator) =>
        {
            request.AllowedRoles.AddRange(new[] { Role.SuperAdmin, Role.Admin });
            var response = await mediator.Send(request);
            return Results.Json(new JsonResponse<TokenResponse>(response), statusCode: StatusCodes.Status201Created);
        })
        .RequireAuthorization(policy => policy.RequireRole(Role.SuperAdmin, Role.Admin))
        .WithTags("Admin Sessions")
        .WithSummary("Refresh admin's access token")
        .WithDescription("Refresh admin's access token");
    }
}
