using System.Collections.Generic;
using System.Text.Json.Serialization;
using MediatR;
using FluentValidation;
using ApplicationAuth.Features.Account.Login;
using ApplicationAuth.Features.Account.Shared;

namespace ApplicationAuth.Features.Account.RefreshToken;

public record RefreshTokenRequest(
    string RefreshToken
) : IRequest<TokenResponse>
{
    [JsonIgnore]
    public List<string> AllowedRoles { get; set; } = new();
}

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh Token is empty");
    }
}
