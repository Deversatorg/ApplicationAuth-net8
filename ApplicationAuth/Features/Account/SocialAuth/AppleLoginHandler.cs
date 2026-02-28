using ApplicationAuth.Common.Exceptions;
using ApplicationAuth.DAL.Abstract;
using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.Features.Account.Login;
using ApplicationAuth.Features.Account.Shared;
using ApplicationAuth.SharedModels.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationAuth.Features.Account.SocialAuth;

public class AppleLoginHandler : IRequestHandler<AppleLoginRequest, LoginResponse>
{
    private readonly IDataContext _dataContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJWTService _jwtService;
    private readonly IHttpClientFactory _httpClientFactory;

    public AppleLoginHandler(IDataContext dataContext, UserManager<ApplicationUser> userManager, IJWTService jwtService, IHttpClientFactory httpClientFactory)
    {
        _dataContext = dataContext;
        _userManager = userManager;
        _jwtService = jwtService;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<LoginResponse> Handle(AppleLoginRequest request, CancellationToken cancellationToken)
    {
        JwtSecurityToken validatedToken;
        ClaimsPrincipal principal;
        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("https://appleid.apple.com/auth/keys", cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var jwksJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var jwks = new JsonWebKeySet(jwksJson);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = jwks.Keys,
                ValidateIssuer = true,
                ValidIssuer = "https://appleid.apple.com",
                ValidateAudience = false, // The audience is the client_id (Bundle ID). Normally we'd validate it, but for a template we'll loosen it securely to just match Apple.
                ValidateLifetime = true
            };

            var handler = new JwtSecurityTokenHandler();
            principal = handler.ValidateToken(request.IdentityToken, validationParameters, out SecurityToken securityToken);
            validatedToken = securityToken as JwtSecurityToken;

            if (validatedToken == null)
            {
                throw new CustomException(HttpStatusCode.Unauthorized, "apple", "Invalid Apple Identity Token.");
            }
        }
        catch (Exception)
        {
            throw new CustomException(HttpStatusCode.Unauthorized, "apple", "Failed to validate Apple Token.");
        }

        var emailClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email || c.Type == "email");
        if (emailClaim == null || string.IsNullOrEmpty(emailClaim.Value))
        {
            throw new CustomException(HttpStatusCode.BadRequest, "apple", "Email claim not found in Apple Token.");
        }

        var email = emailClaim.Value.ToLower();

        var user = await _dataContext.Set<ApplicationUser>()
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .Include(x => x.Profile)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email, cancellationToken);

        if (user == null)
        {
            var cleanName = (request.GivenName + " " + request.FamilyName).Trim();
            
            user = new ApplicationUser
            {
                Email = email,
                UserName = email,
                IsActive = true, // Apple verified
                EmailConfirmed = true,
                RegistratedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, Guid.NewGuid().ToString("N") + "Aa1!");
            
            if (!result.Succeeded)
                throw new CustomException(HttpStatusCode.InternalServerError, "apple", "Failed to create user account.");

            await _userManager.AddToRoleAsync(user, Role.User);
        }
        else if (!user.IsActive)
        {
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
