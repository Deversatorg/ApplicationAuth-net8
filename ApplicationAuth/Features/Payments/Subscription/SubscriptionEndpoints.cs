using ApplicationAuth.Features.Payments.Subscription;
using ApplicationAuth.SharedModels.ResponseModels;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace ApplicationAuth.Features.Payments.Subscription
{
    public static class SubscriptionEndpoints
    {
        public static void MapSubscriptionEndpoints(this IEndpointRouteBuilder app)
        {
            // GET /api/v1/payments/subscription
            app.MapGet("/api/v1/payments/subscription", async (
                [FromServices] IMediator mediator,
                HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out var userId))
                    return Results.Unauthorized();

                var result = await mediator.Send(new GetSubscriptionQuery(userId));
                if (result == null)
                    return Results.NotFound(new JsonResponse<string>("No subscription found"));

                return Results.Ok(new JsonResponse<SubscriptionResponse>(result));
            })
            .RequireAuthorization()
            .WithSummary("Get My Subscription")
            .WithDescription("Returns the current user's subscription status and plan details.");

            // POST /api/v1/payments/subscription/cancel
            app.MapPost("/api/v1/payments/subscription/cancel", async (
                [FromServices] IMediator mediator,
                HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out var userId))
                    return Results.Unauthorized();

                await mediator.Send(new CancelSubscriptionRequest(userId));
                return Results.Ok(new JsonResponse<string>("Subscription will be canceled at the end of the current billing period."));
            })
            .RequireAuthorization()
            .WithSummary("Cancel Subscription")
            .WithDescription("Cancels the current user's active subscription at the end of the billing period.");
        }
    }
}
