using ApplicationAuth.Common.Exceptions;
using System.Linq;
using System.Threading.Tasks;
using ApplicationAuth.DAL.Abstract;
using ApplicationAuth.Domain.Entities.Identity;

using ApplicationAuth.Common.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;
using ApplicationAuth.Features.Account.Shared;

using MediatR;
using System.Threading;

namespace ApplicationAuth.Features.Account.Login;

public class LoginHandler : IRequestHandler<LoginRequest, LoginResponse>
{
    private readonly IDataContext _dataContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJWTService _jwtService;

    public LoginHandler(IDataContext dataContext, UserManager<ApplicationUser> userManager, IJWTService jwtService)
    {
        _dataContext = dataContext;
        _userManager = userManager;
        _jwtService = jwtService;
    }

    public async Task<LoginResponse> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = _dataContext.Set<ApplicationUser>().Where(x => x.Email == request.Email)
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .Include(x => x.Profile)
            .FirstOrDefault();

        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password) || !user.UserRoles.Any(x => x.Role.Name == Role.User))
            throw new CustomException(HttpStatusCode.BadRequest, "credentials", "Invalid credentials");

        if (!string.IsNullOrEmpty(request.Email) && !user.EmailConfirmed)
            throw new CustomException(HttpStatusCode.BadRequest, "email", "Email is not confirmed");

        if (user.IsDeleted)
            throw new CustomException(HttpStatusCode.BadRequest, "general", "Your account was deleted by admin, to know more please contact administration.");

        if (!user.IsActive)
            throw new CustomException(HttpStatusCode.Forbidden, "general", "Your account was blocked, to know more please contact administration.");

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
