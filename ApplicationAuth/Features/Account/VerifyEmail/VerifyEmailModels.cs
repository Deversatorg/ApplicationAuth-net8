using MediatR;

namespace ApplicationAuth.Features.Account.VerifyEmail;

public record VerifyEmailRequest(string Email, string Code) : IRequest<VerifyEmailResponse>;

public record VerifyEmailResponse(bool Success, string Message);
