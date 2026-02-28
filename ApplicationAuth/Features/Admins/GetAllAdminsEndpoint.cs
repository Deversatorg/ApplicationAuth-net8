using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Collections.Generic;
using ApplicationAuth.Common.Constants;
using ApplicationAuth.SharedModels.Enums;
using ApplicationAuth.SharedModels.RequestModels;
using ApplicationAuth.SharedModels.ResponseModels;
using ApplicationAuth.SharedModels.ResponseModels.Base;
using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.Features.AdminUsers.GetAll;
using ApplicationAuth.Features.AdminUsers;
using MediatR;

namespace ApplicationAuth.Features.Admins;

public static class GetAllAdminsEndpoint
{
    public static void MapGetAllAdminsEndpoints(this IEndpointRouteBuilder app)
    {
        // GET api/v1/superadmin/admins
        app.MapGet("/api/v1/superadmin/admins", async (
            [FromQuery] int? limit,
            [FromQuery] int? offset,
            [FromQuery] string search,
            [FromQuery] UserTableColumn? orderKey,
            [FromQuery] SortingDirection? orderDirection,
            ClaimsPrincipal user,
            [FromServices] IMediator mediator) =>
        {
            var model = new PaginationRequestModel<UserTableColumn>
            {
                Limit = limit ?? 10,
                Offset = offset ?? 0,
                Search = search,
                Order = orderKey.HasValue ? new OrderingRequestModel<UserTableColumn, SortingDirection> { Key = orderKey.Value, Direction = orderDirection ?? SortingDirection.Asc } : null
            };
            bool isSuperAdmin = user.IsInRole(Role.SuperAdmin);
            bool isAdmin = user.IsInRole(Role.Admin);

            var data = await mediator.Send(new GetAllUsersOffsetQuery(model, true, isSuperAdmin, isAdmin));

            return Results.Ok(new JsonPaginationResponse<List<UserTableRowResponse>>(
                data.Data, 
                model.Offset + model.Limit, 
                data.TotalCount));
        })
        .RequireAuthorization(policy => policy.RequireRole(Role.SuperAdmin))
        .WithTags("Admins")
        .WithSummary("Retrieve administrators in pagination")
        .WithDescription("Retrieve administrators in pagination");
    }
}
