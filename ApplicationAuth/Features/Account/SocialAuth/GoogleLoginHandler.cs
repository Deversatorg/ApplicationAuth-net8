using ApplicationAuth.Common.Exceptions;
using ApplicationAuth.DAL.Abstract;
using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.Features.Account.Login;
using ApplicationAuth.Features.Account.Shared;
using ApplicationAuth.SharedModels.Enums;
using Google.Apis.Auth;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationAuth.Features.Account.SocialAuth;

public class GoogleLoginHandler : IRequestHandler<GoogleLoginRequest, LoginResponse>
{
    private readonly IDataContext _dataContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJWTService _jwtService;

    public GoogleLoginHandler(IDataContext dataContext, UserManager<ApplicationUser> userManager, IJWTService jwtService)
    {
        _dataContext = dataContext;
        _userManager = userManager;
        _jwtService = jwtService;
    }

    public async Task<LoginResponse> Handle(GoogleLoginRequest request, CancellationToken cancellationToken)
    {
        GoogleJsonWebSignature.Payload payload;
        try
        {
            // Validate the token cryptographically against Google's public keys
            payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken);
        }
        catch (InvalidJwtException)
        {
            throw new CustomException(HttpStatusCode.Unauthorized, "google", "Invalid Google ID Token.");
        }

        if (string.IsNullOrEmpty(payload.Email))
            throw new CustomException(HttpStatusCode.BadRequest, "google", "Email claim not found in Google Token.");

        var email = payload.Email.ToLower();

        var user = await _dataContext.Set<ApplicationUser>()
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .Include(x => x.Profile)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email, cancellationToken);

        if (user == null)
        {
            user = new ApplicationUser
            {
                Email = email,
                UserName = email,
                IsActive = true, // Auto-activate since Google verified them
                EmailConfirmed = true,
                RegistratedAt = DateTime.UtcNow
            };

            // Use a completely random unguessable password for social auth accounts
            var result = await _userManager.CreateAsync(user, Guid.NewGuid().ToString("N") + "Aa1!");
            
            if (!result.Succeeded)
                throw new CustomException(HttpStatusCode.InternalServerError, "google", "Failed to create user account.");

            await _userManager.AddToRoleAsync(user, Role.User);
        }
        else if (!user.IsActive)
        {
            // If they registered normally but never verified, logging in via Google proves ownership
            user.IsActive = true;
            user.EmailConfirmed = true;
            await _dataContext.SaveChangesAsync(cancellationToken);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var tokenResponseModel = await _jwtService.BuildLoginResponse(user, null);

        return new LoginResponse(
            User: new UserResponse(
                Id: user.Id,
                UserName: user.UserName,
                Email: user.Email,
                FirstName: user.Profile?.FirstName,
                LastName: user.Profile?.LastName,
                IsBlocked: !user.IsActive,
                Role: roles.FirstOrDefault() ?? "none"
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
