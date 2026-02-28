using ApplicationAuth.Common.Exceptions;
using ApplicationAuth.Common.Extensions;
using ApplicationAuth.Common.Constants;
using ApplicationAuth.DAL.Abstract;
using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.SharedModels.Enums;
using ApplicationAuth.SharedModels.RequestModels;
using ApplicationAuth.SharedModels.RequestModels.Base.CursorPagination;
using ApplicationAuth.SharedModels.ResponseModels.Base.CursorPagination;
using ApplicationAuth.SharedModels.ResponseModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using System.Threading;
using MediatR;

namespace ApplicationAuth.Features.AdminUsers.GetAll;

public class GetAllUsersHandler : 
    IRequestHandler<GetAllUsersOffsetQuery, PaginationResponseModel<UserTableRowResponse>>,
    IRequestHandler<GetAllUsersCursorQuery, CursorPaginationBaseResponseModel<UserTableRowResponse>>
{
    private readonly IDataContext _dataContext;

    public GetAllUsersHandler(IDataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<PaginationResponseModel<UserTableRowResponse>> Handle(GetAllUsersOffsetQuery request, CancellationToken cancellationToken)
    {
        if (!request.IsSuperAdmin && !request.IsAdmin)
            throw new CustomException(HttpStatusCode.Forbidden, "role", "You don't have permissions");

        var search = !string.IsNullOrEmpty(request.Model.Search) && request.Model.Search.Length > 1;

        var users = _dataContext.Set<ApplicationUser>().Where(x => !x.IsDeleted
                && !x.UserRoles.Any(w => w.Role.Name == Role.SuperAdmin)
                && (!search || (x.Email.Contains(request.Model.Search) || x.Profile.FirstName.Contains(request.Model.Search) || x.Profile.LastName.Contains(request.Model.Search)))
                && (request.GetAdmins ? x.UserRoles.Any(w => w.Role.Name == Role.Admin) : x.UserRoles.Any(w => w.Role.Name == Role.User))
                && (request.IsSuperAdmin || !x.UserRoles.Any(w => (w.Role.Name == Role.Admin))))
            .TagWith("GetAllUsers_GetUsers")
            .Include(w => w.UserRoles)
                .ThenInclude(w => w.Role)
            .Select(x => new
            {
                Email = x.Email,
                FirstName = x.Profile.FirstName,
                LastName = x.Profile.LastName,
                IsBlocked = !x.IsActive,
                RegisteredAt = x.RegistratedAt,
                Id = x.Id
            });

        if (search)
            users = users.Where(x => x.Email.Contains(request.Model.Search) || x.FirstName.Contains(request.Model.Search) || x.LastName.Contains(request.Model.Search));

        int count = await users.CountAsync(cancellationToken);

        if (request.Model.Order != null)
            users = users.OrderBy(request.Model.Order.Key.ToString(), request.Model.Order.Direction == SortingDirection.Asc);

        users = users.Skip(request.Model.Offset).Take(request.Model.Limit);

        var response = users.Select(x => new UserTableRowResponse(
            x.Id,
            x.FirstName,
            x.LastName,
            x.Email,
            x.RegisteredAt.ToISO(),
            x.IsBlocked
        )).ToList();

        return new(response, count);
    }

    public async Task<CursorPaginationBaseResponseModel<UserTableRowResponse>> Handle(GetAllUsersCursorQuery request, CancellationToken cancellationToken)
    {
        if (!request.IsSuperAdmin && !request.IsAdmin)
            throw new CustomException(HttpStatusCode.Forbidden, "role", "You don't have permissions");

        var search = !string.IsNullOrEmpty(request.Model.Search) && request.Model.Search.Length > 1;

        var users = _dataContext.Set<ApplicationUser>().Where(x => !x.IsDeleted
                && !x.UserRoles.Any(w => w.Role.Name == Role.SuperAdmin)
                && (!search || (x.Email.Contains(request.Model.Search) || x.Profile.FirstName.Contains(request.Model.Search) || x.Profile.LastName.Contains(request.Model.Search)))
                && (request.GetAdmins ? x.UserRoles.Any(w => w.Role.Name == Role.Admin) : x.UserRoles.Any(w => w.Role.Name == Role.User))
                && (request.IsSuperAdmin || !x.UserRoles.Any(w => (w.Role.Name == Role.Admin))))
            .TagWith("GetAllUsersCursor_GetUsers")
            .Select(x => new
            {
                Email = x.Email,
                FirstName = x.Profile.FirstName,
                LastName = x.Profile.LastName,
                IsBlocked = !x.IsActive,
                RegisteredAt = x.RegistratedAt,
                Id = x.Id
            });

        if (request.Model.Order != null)
            users = users.OrderBy(request.Model.Order.Key.ToString(), request.Model.Order.Direction == SortingDirection.Asc);

        var userList = await users.ToListAsync(cancellationToken);

        var offset = 0;

        if (request.Model.LastId.HasValue)
        {
            var item = userList.FirstOrDefault(u => u.Id == request.Model.LastId);

            if (item is null)
                throw new CustomException(HttpStatusCode.BadRequest, "lastId", "There is no user with specific id in selection");

            offset = userList.IndexOf(item) + 1;
        }

        users = users.Skip(offset).Take(request.Model.Limit + 1);

        var response = users.Select(x => new UserTableRowResponse(
            x.Id,
            x.FirstName,
            x.LastName,
            x.Email,
            x.RegisteredAt.ToISO(),
            x.IsBlocked
        ));

        int? nextCursorId = null;

        if (users.Count() > request.Model.Limit)
        {
            response = response.Take(request.Model.Limit);
            nextCursorId = response.AsEnumerable().LastOrDefault()?.Id;
        }

        return new(response.ToList(), nextCursorId);
    }
}
