using ApplicationAuth.Common.Exceptions;
using ApplicationAuth.DAL.Abstract;
using ApplicationAuth.Domain.Entities.Identity;

using Microsoft.EntityFrameworkCore;
using ApplicationAuth.Features.Account.Shared;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using System.Threading;
using MediatR;

namespace ApplicationAuth.Features.Account.Logout;

public class LogoutHandler : IRequestHandler<LogoutRequest>
{
    private readonly IDataContext _dataContext;
    private readonly IJWTService _jwtService;

    public LogoutHandler(IDataContext dataContext, IJWTService jwtService)
    {
        _dataContext = dataContext;
        _jwtService = jwtService;
    }

    public async Task Handle(LogoutRequest request, CancellationToken cancellationToken)
    {
        var user = _dataContext.Set<ApplicationUser>().Where(x => x.Id == request.UserId)
                .TagWith("Logout_GetUser")
                .Include(x => x.Tokens)
                .FirstOrDefault();

        if (user == null)
            throw new CustomException(HttpStatusCode.BadRequest, "user", "User is not found");

        await _jwtService.ClearUserTokens(user);
        
        await _dataContext.SaveChangesAsync();
    }
}
