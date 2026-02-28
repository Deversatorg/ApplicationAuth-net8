using ApplicationAuth.SharedModels;
using FluentValidation;
using MediatR;

namespace ApplicationAuth.Features.Account.PasswordRecovery;

public record ForgotPasswordRequest(string Email) : IRequest<bool>;

public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address field is empty")
            .EmailAddress().WithMessage("Email address is not in valid format");
    }
}

public record ResetPasswordRequest(string Email, string Code, string NewPassword) : IRequest<bool>;

public class ResetPasswordValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address field is empty")
            .EmailAddress().WithMessage("Email address is not in valid format");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Verification code is required")
            .Length(6).WithMessage("Code must be 6 characters long")
            .Matches("^[0-9]+$").WithMessage("Code must contain only digits");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .Length(6, 50).WithMessage("Password should be from 6 to 50 characters")
            .Matches(ModelRegularExpression.REG_ONE_LATER_DIGIT).WithMessage("Password should contain at least one letter and one digit")
            .Matches(ModelRegularExpression.REG_NOT_CONTAIN_SPACES_ONLY).WithMessage("Password can’t contain spaces only");
    }
}
