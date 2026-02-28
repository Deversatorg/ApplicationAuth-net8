using ApplicationAuth.Common.Constants;
using ApplicationAuth.SharedModels;
using FluentValidation;

using MediatR;

namespace ApplicationAuth.Features.Account.Register;

public record RegisterRequest(
    string Email,
    string Password,
    string ConfirmPassword
) : IRequest<RegisterResponse>;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address field is empty")
            .EmailAddress().WithMessage("Email address is not in valid format")
            .Length(3, 129).WithMessage("Email address is not in valid format")
            .Matches(ModelRegularExpression.REG_EMAIL).WithMessage("Email address is not in valid format")
            .Matches(ModelRegularExpression.REG_EMAIL_DOMAINS).WithMessage("Email address is not in valid format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password field is empty")
            .Length(6, 50).WithMessage("Password should be from 6 to 50 characters")
            .Matches(ModelRegularExpression.REG_ONE_LATER_DIGIT).WithMessage("Password should contain at least one letter and one digit")
            .Matches(ModelRegularExpression.REG_NOT_CONTAIN_SPACES_ONLY).WithMessage("Password can’t contain spaces only");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Confirm Password isn’t the same as Password");
    }
}
