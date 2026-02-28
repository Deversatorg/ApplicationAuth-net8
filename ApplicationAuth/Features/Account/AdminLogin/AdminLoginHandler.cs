using ApplicationAuth.Common.Exceptions;
using ApplicationAuth.DAL.Abstract;
using ApplicationAuth.Domain.Entities.Identity;

using ApplicationAuth.Common.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using ApplicationAuth.Features.Account.Shared;
using ApplicationAuth.Features.Account.Login;

using MediatR;
using System.Threading;

namespace ApplicationAuth.Features.Account.AdminLogin;

public class AdminLoginHandler : IRequestHandler<AdminLoginRequest, LoginResponse>
{
    private readonly IDataContext _dataContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJWTService _jwtService;

    public AdminLoginHandler(IDataContext dataContext, UserManager<ApplicationUser> userManager, IJWTService jwtService)
    {
        _dataContext = dataContext;
        _userManager = userManager;
        _jwtService = jwtService;
    }

    public async Task<LoginResponse> Handle(AdminLoginRequest request, CancellationToken cancellationToken)
    {
        var user = _dataContext.Set<ApplicationUser>().Where(x => x.Email == request.Email)
            .TagWith("Login_GetAdmin")
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .Include(x => x.Profile)
            .FirstOrDefault();

        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password) || !user.UserRoles.Any(x => x.Role.Name == Role.Admin || x.Role.Name == Role.SuperAdmin))
            throw new CustomException(HttpStatusCode.BadRequest, "general", "Invalid credentials");

        var tokenResponseModel = await _jwtService.BuildLoginResponse(user, request.AccessTokenLifetime);

        return new LoginResponse(
            User: new UserResponse(
                Id: user.Id,
                UserName: user.UserName,
                Email: user.Email,
                FirstName: user.Profile?.FirstName,
                LastName: user.Profile?.LastName,
                IsBlocked: !user.IsActive,
                Role: user.UserRoles?.FirstOrDefault()?.Role?.Name ?? "none"
            ),
            Token: new TokenResponse(
                AccessToken: tokenResponseModel.Token.AccessToken,
                RefreshToken: tokenResponseModel.Token.RefreshToken,
                ExpireDate: tokenResponseModel.Token.ExpireDate,
                Type: tokenResponseModel.Token.Type
            )
        );
    }
}
