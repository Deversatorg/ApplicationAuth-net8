using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.Common.Constants;
using ApplicationAuth.SharedModels.ResponseModels;
using MediatR;

namespace ApplicationAuth.Features.AdminUsers.Delete;

public static class DeleteUserEndpoint
{
    public static void MapDeleteUserEndpoints(this IEndpointRouteBuilder app)
    {
        // DELETE api/v1/admin-users/{id}
        app.MapDelete("/api/v1/admin-users/{id}", async (
            [FromRoute] int id,
            ClaimsPrincipal user,
            [FromServices] IMediator mediator) =>
        {
            bool isSuperAdmin = user.IsInRole(Role.SuperAdmin);

            var data = await mediator.Send(new DeleteUserRequest(id) { IsSuperAdmin = isSuperAdmin });

            return Results.Ok(new JsonResponse<UserResponse>(data));
        })
        .RequireAuthorization(policy => policy.RequireRole(Role.Admin, Role.SuperAdmin))
        .WithTags("Admin Users")
        .WithSummary("Delete user")
        .WithDescription("Soft Delete user");
    }
}
