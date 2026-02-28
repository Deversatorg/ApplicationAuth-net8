using MediatR;
using ApplicationAuth.Features.Account.Login;

namespace ApplicationAuth.Features.Account.SocialAuth;

public record GoogleLoginRequest(string IdToken) : IRequest<LoginResponse>;

public record AppleLoginRequest(string IdentityToken, string GivenName, string FamilyName) : IRequest<LoginResponse>;
