using System.Text.Json.Serialization;
using ApplicationAuth.Common.Constants;
using ApplicationAuth.SharedModels;
using FluentValidation;

using MediatR;

namespace ApplicationAuth.Features.Account.Login;

public record LoginRequest(
    string Email,
    string Password,
    [property: JsonPropertyName("accessTokenLifetime")] int? AccessTokenLifetime
) : IRequest<LoginResponse>;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address field is empty")
            .EmailAddress().WithMessage("Email address is not in valid format")
            .Length(3, 129).WithMessage("Email address is not in valid format")
            .Matches(ModelRegularExpression.REG_EMAIL).WithMessage("Email address is not in valid format")
            .Matches(ModelRegularExpression.REG_EMAIL_DOMAINS).WithMessage("Email address is not in valid format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .Length(6, 50).WithMessage("Password should be from 6 to 50 characters")
            .Matches(ModelRegularExpression.REG_ONE_LATER_DIGIT).WithMessage("Password should contain at least one letter and one digit")
            .Matches(ModelRegularExpression.REG_NOT_CONTAIN_SPACES_ONLY).WithMessage("Password can’t contain spaces only");
    }
}
