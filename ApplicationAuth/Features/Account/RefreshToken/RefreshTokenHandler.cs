using ApplicationAuth.Common.Exceptions;
using ApplicationAuth.DAL.Abstract;
using ApplicationAuth.Domain.Entities.Identity;

using ApplicationAuth.Common.Utilities.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ApplicationAuth.Features.Account.Login;
using ApplicationAuth.Features.Account.Shared;

using System.Threading;
using MediatR;

namespace ApplicationAuth.Features.Account.RefreshToken;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenRequest, TokenResponse>
{
    private readonly IDataContext _dataContext;
    private readonly IJWTService _jwtService;
    private readonly IHashUtility _hashUtility;

    public RefreshTokenHandler(IDataContext dataContext, IJWTService jwtService, IHashUtility hashUtility)
    {
        _dataContext = dataContext;
        _jwtService = jwtService;
        _hashUtility = hashUtility;
    }

    public async Task<TokenResponse> Handle(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var hash = _hashUtility.GetHash(request.RefreshToken);

        var token = _dataContext.Set<UserToken>().Where(w => w.RefreshTokenHash == hash && w.IsActive && w.RefreshExpiresDate > DateTime.UtcNow)
            .TagWith("RefreshToken_GetRefreshToken")
            .Include(x => x.User)
                .ThenInclude(x => x.UserRoles)
                    .ThenInclude(x => x.Role)
            .FirstOrDefault();

        if (token == null)
            throw new CustomException(HttpStatusCode.BadRequest, "refreshToken", "Refresh token is invalid");

        if (!token.User.UserRoles.Any(x => request.AllowedRoles.Contains(x.Role.Name)))
            throw new CustomException(HttpStatusCode.Forbidden, "role", "Access denied");

        var result = await _jwtService.CreateUserTokenAsync(token.User, isRefresh: true);
        
        await _dataContext.SaveChangesAsync();

        return new TokenResponse(
            AccessToken: result.AccessToken,
            RefreshToken: result.RefreshToken,
            ExpireDate: result.ExpireDate,
            Type: result.Type
        );
    }
}
