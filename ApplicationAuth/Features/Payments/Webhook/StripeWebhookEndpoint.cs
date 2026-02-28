using ApplicationAuth.Features.Payments.Webhook;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Stripe;
using System.Threading;

namespace ApplicationAuth.Features.Payments.Webhook
{
    public static class StripeWebhookEndpoint
    {
        public static void MapStripeWebhookEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/v1/payments/webhook", async (
                HttpContext httpContext,
                [FromServices] StripeWebhookHandler handler) =>
            {
                var signature = httpContext.Request.Headers["Stripe-Signature"].ToString();
                await handler.HandleAsync(httpContext.Request.Body, signature, httpContext.RequestAborted);
                return Results.Ok();
            })
            .AllowAnonymous()    // Stripe calls this without Bearer token
            .WithSummary("Stripe Webhook")
            .WithDescription("Receives Stripe webhook events. Validates Stripe-Signature header.");
        }
    }
}
