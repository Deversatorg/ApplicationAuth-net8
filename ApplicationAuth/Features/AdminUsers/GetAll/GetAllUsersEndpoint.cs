using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Collections.Generic;
using ApplicationAuth.Common.Constants;
using ApplicationAuth.SharedModels.Enums;
using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.SharedModels.RequestModels;
using ApplicationAuth.SharedModels.RequestModels.Base.CursorPagination;
using ApplicationAuth.SharedModels.ResponseModels;
using ApplicationAuth.SharedModels.ResponseModels.Base;
using ApplicationAuth.SharedModels.ResponseModels.Base.CursorPagination;
using MediatR;

namespace ApplicationAuth.Features.AdminUsers.GetAll;

public static class GetAllUsersEndpoint
{
    public static void MapGetAllUsersEndpoints(this IEndpointRouteBuilder app)
    {
        // GET api/v1/admin-users
        app.MapGet("/api/v1/admin-users", async (
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

            var data = await mediator.Send(new GetAllUsersOffsetQuery(model, false, isSuperAdmin, isAdmin));

            return Results.Ok(new JsonPaginationResponse<List<UserTableRowResponse>>(
                data.Data, 
                model.Offset + model.Limit, 
                data.TotalCount));
        })
        .RequireAuthorization(policy => policy.RequireRole(Role.Admin, Role.SuperAdmin))
        .WithTags("Admin Users")
        .WithSummary("Retrieve users in pagination")
        .WithDescription("Retrieve users in pagination");

        // GET api/v1/admin-users/cursor
        app.MapGet("/api/v1/admin-users/cursor", async (
            [FromQuery] int? limit,
            [FromQuery] int? lastId,
            [FromQuery] string search,
            [FromQuery] UserTableColumn? orderKey,
            [FromQuery] SortingDirection? orderDirection,
            ClaimsPrincipal user,
            [FromServices] IMediator mediator) =>
        {
            var model = new CursorPaginationRequestModel<UserTableColumn>
            {
                Limit = limit ?? 10,
                LastId = lastId,
                Search = search,
                Order = orderKey.HasValue ? new OrderingRequestModel<UserTableColumn, SortingDirection> { Key = orderKey.Value, Direction = orderDirection ?? SortingDirection.Asc } : null
            };
            bool isSuperAdmin = user.IsInRole(Role.SuperAdmin);
            bool isAdmin = user.IsInRole(Role.Admin);

            var data = await mediator.Send(new GetAllUsersCursorQuery(model, false, isSuperAdmin, isAdmin));

            return Results.Ok(new CursorJsonPaginationResponse<List<UserTableRowResponse>>(
                data.Data, 
                data.LastId));
        })
        .RequireAuthorization(policy => policy.RequireRole(Role.Admin, Role.SuperAdmin))
        .WithTags("Admin Users")
        .WithSummary("Retrieve users in cursor pagination")
        .WithDescription("Retrieve users in cursor pagination");
    }
}
