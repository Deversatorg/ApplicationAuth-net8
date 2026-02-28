using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ApplicationAuth.SharedModels.ResponseModels;
using MediatR;

namespace ApplicationAuth.Features.Account.VerifyEmail;

public static class VerifyEmailEndpoint
{
    public static void MapVerifyEmailEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/sessions/verify-email", async ([FromBody] VerifyEmailRequest model, [FromServices] IMediator mediator) =>
        {
            var response = await mediator.Send(model);
            return Results.Ok(new JsonResponse<VerifyEmailResponse>(response));
        })
        .AllowAnonymous()
        .RequireRateLimiting("AuthPolicy")
        .WithTags("Account")
        .WithSummary("Verify Email using Registration Code")
        .WithDescription("Allows users to verify their account using the 6-digit code sent to their email.");
    }
}
