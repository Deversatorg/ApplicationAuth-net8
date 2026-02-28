using ApplicationAuth.Common.Exceptions;
using ApplicationAuth.Common.Constants;
using ApplicationAuth.DAL.Abstract;
using ApplicationAuth.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net;

using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ApplicationAuth.Features.AdminUsers.Delete;

public class DeleteUserHandler : IRequestHandler<DeleteUserRequest, UserResponse>
{
    private readonly IDataContext _dataContext;

    public DeleteUserHandler(IDataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<UserResponse> Handle(DeleteUserRequest request, CancellationToken cancellationToken)
    {
        var user = _dataContext.Set<ApplicationUser>()
            .Where(w => w.Id == request.Id && !w.UserRoles.Any(x => x.Role.Name == Role.SuperAdmin) && (!w.UserRoles.Any(x => x.Role.Name == Role.Admin) || request.IsSuperAdmin))
            .TagWith("DeleteUser_GetUser")
            .Include(w => w.Profile)
            .FirstOrDefault();

        if (user == null)
            throw new CustomException(HttpStatusCode.BadRequest, "userId", "User is not found");

        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;

        _dataContext.Set<ApplicationUser>().Update(user);
        await _dataContext.SaveChangesAsync(cancellationToken);

        return new UserResponse(
            user.Id,
            user.Email,
            user.PhoneNumber,
            user.Profile?.FirstName,
            user.Profile?.LastName,
            !user.IsActive
        );
    }
}
