using MediatR;

namespace ApplicationAuth.Features.Account.Logout;

public record LogoutRequest(int UserId) : IRequest;
