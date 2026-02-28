using ApplicationAuth.Common.Constants;
using ApplicationAuth.Common.Extensions;
using ApplicationAuth.Common.Utilities.Interfaces;
using ApplicationAuth.DAL.Abstract;
using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.SharedModels.ResponseModels.Session;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ApplicationAuth.Features.Account.Shared
{
    public class JWTService : IJWTService
    {
        private const int _cookiesLifeTime = 1440; //24hours

        private readonly UserManager<ApplicationUser> _userManager = null;
        private readonly IHashUtility _hashService = null;
        private readonly IDataContext _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public JWTService(UserManager<ApplicationUser> userManager,
            IHashUtility hashService,
            IDataContext unitOfWork,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _hashService = hashService;
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ClaimsIdentity> GetIdentity(ApplicationUser user, bool isRefreshToken)
        {
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, user.UserName),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim("isRefresh", isRefreshToken.ToString())
                };

                foreach (var role in roles)
                    claims.Add(new Claim(ClaimsIdentity.DefaultRoleClaimType, role));

                return new(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            }
            return null;
        }

        public JwtSecurityToken CreateToken(DateTime now, ClaimsIdentity identity, DateTime lifetime)
        {
            return new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                notBefore: now,
                claims: identity.Claims,
                expires: lifetime,
                //signingCredentials: AuthOptions.GetSigningCredentials()
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
                );
        }

        public async Task<TokenResponseModel> CreateUserTokenAsync(ApplicationUser user, int? accessTokenLifetime = null, bool isRefresh = false)
        {
            var dateNow = DateTime.UtcNow;

            #region remove old tokens

            var tokens = _unitOfWork.Set<UserToken>().Where(x => x.UserId == user.Id)
                .TagWith(nameof(CreateUserTokenAsync) + "_GetUsersTokens")
                .ToList();

            tokens.ForEach(x => _unitOfWork.Set<UserToken>().Remove(x));

            #endregion

            if (!user.IsActive)
                return null;

            #region create token

            var accessIdentity = await GetIdentity(user, false);
            var refreshIdentity = await GetIdentity(user, true);

            if (accessIdentity == null || refreshIdentity == null)
                throw new Exception("User not found");

            var accessLifetime = accessTokenLifetime.HasValue && accessTokenLifetime.Value != 0 ? dateNow.Add(TimeSpan.FromSeconds(accessTokenLifetime.Value)) : dateNow.Add(TimeSpan.FromDays(AuthOptions.ACCESS_TOKEN_LIFETIME));
            var refreshLifetime = dateNow.Add(TimeSpan.FromDays(AuthOptions.REFRESH_TOKEN_LIFETIME));

            var accessJwtToken = new JwtSecurityTokenHandler().WriteToken(CreateToken(dateNow, accessIdentity, accessLifetime));
            var refreshJwtToken = new JwtSecurityTokenHandler().WriteToken(CreateToken(dateNow, refreshIdentity, refreshLifetime));

            user.Tokens.Add(new UserToken
            {
                AccessExpiresDate = accessLifetime,
                RefreshExpiresDate = refreshLifetime,
                IsActive = true,
                AccessTokenHash = _hashService.GetHash(accessJwtToken),
                RefreshTokenHash = _hashService.GetHash(refreshJwtToken),
                CreatedAt = DateTime.UtcNow
            });

            #endregion

            var response = new TokenResponseModel
            {
                AccessToken = accessJwtToken,
                ExpireDate = accessLifetime.ToISO(),
                RefreshToken = refreshJwtToken,
                Type = "Bearer"
            };

            _unitOfWork.Set<ApplicationUser>().Update(user);
            _unitOfWork.SaveChanges();

            return response;
        }

        public async Task<LoginResponseModel> BuildLoginResponse(ApplicationUser user, int? accessTokenLifetime = null)
        {
            user.LastVisitAt = DateTime.UtcNow;

            _unitOfWork.Set<ApplicationUser>().Update(user);
            _unitOfWork.SaveChanges();

            var tokenResponseModel = await CreateUserTokenAsync(user, accessTokenLifetime);

            var roles = await _userManager.GetRolesAsync(user);

            var result = new LoginResponseModel()
            {
                User = new UserRoleResponseModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    FirstName = user.Profile?.FirstName,
                    LastName = user.Profile?.LastName,
                    Role = (roles != null) ? roles.SingleOrDefault() : "none"
                },
                Token = tokenResponseModel,
            };

            _httpContextAccessor.HttpContext.Response.Cookies.Append(".AspNetCore.Application.Id", result.Token.AccessToken,
            new CookieOptions
            {
                MaxAge = TimeSpan.FromMinutes(_cookiesLifeTime)
            });

            _httpContextAccessor.HttpContext.Response.Cookies.Append(".AspNetCore.Application.Id.Refresh", result.Token.RefreshToken,
            new CookieOptions
            {
                MaxAge = TimeSpan.FromMinutes(_cookiesLifeTime)
            });


            return result;
        }

        public async Task ClearUserTokens(ApplicationUser user)
        {
            var tokens = user.Tokens.ToList();

            tokens.ForEach(x => _unitOfWork.Set<UserToken>().Remove(x));

            _unitOfWork.SaveChanges();

            _httpContextAccessor.HttpContext.Response.Cookies.Delete(".AspNetCore.Application.Id");
            _httpContextAccessor.HttpContext.Response.Cookies.Delete(".AspNetCore.Application.Id.Refresh");

        }
    }
}
