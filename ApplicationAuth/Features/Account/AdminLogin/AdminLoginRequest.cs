using System.Text.Json.Serialization;
using ApplicationAuth.Common.Constants;
using ApplicationAuth.SharedModels;
using FluentValidation;

using MediatR;
using ApplicationAuth.Features.Account.Login;

namespace ApplicationAuth.Features.Account.AdminLogin;

public record AdminLoginRequest(
    string Email,
    string Password,
    [property: JsonPropertyName("accessTokenLifetime")] int? AccessTokenLifetime
) : IRequest<LoginResponse>;

public class AdminLoginRequestValidator : AbstractValidator<AdminLoginRequest>
{
    public AdminLoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address field is empty")
            .EmailAddress().WithMessage("Email address is not in valid format")
            .Length(3, 129).WithMessage("Email address is not in valid format")
            .Matches(ModelRegularExpression.REG_EMAIL).WithMessage("Email address is not in valid format")
            .Matches(ModelRegularExpression.REG_EMAIL_DOMAINS).WithMessage("Email address is not in valid format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}
