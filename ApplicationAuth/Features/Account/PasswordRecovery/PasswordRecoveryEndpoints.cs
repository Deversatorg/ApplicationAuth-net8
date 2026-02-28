using ApplicationAuth.SharedModels.ResponseModels;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;

namespace ApplicationAuth.Features.Account.PasswordRecovery;

public static class PasswordRecoveryEndpoints
{
    public static void MapPasswordRecoveryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/sessions");

        group.MapPost("/forgot-password", async ([FromBody] ForgotPasswordRequest request, [FromServices] IMediator mediator) =>
        {
            await mediator.Send(request);
            return Results.Ok(new JsonResponse<object>("If the email is registered, a recovery code has been sent."));
        })
        .AllowAnonymous()
        .RequireRateLimiting("AuthPolicy")
        .WithTags("Account")
        .WithSummary("Forgot Password")
        .WithDescription("Sends a 6-digit recovery code to the specified email.");

        group.MapPost("/reset-password", async ([FromBody] ResetPasswordRequest request, [FromServices] IMediator mediator) =>
        {
            await mediator.Send(request);
            return Results.Ok(new JsonResponse<object>("Password reset successful."));
        })
        .AllowAnonymous()
        .RequireRateLimiting("AuthPolicy")
        .WithTags("Account")
        .WithSummary("Reset Password")
        .WithDescription("Resets a user password using the provided 6-digit recovery code.");
    }
}
