using ApplicationAuth.Features.Payments.Checkout;
using ApplicationAuth.SharedModels.ResponseModels;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Linq;
using System.Security.Claims;

namespace ApplicationAuth.Features.Payments.Checkout
{
    public static class CreateCheckoutSessionEndpoint
    {
        public static void MapCreateCheckoutSessionEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/v1/payments/checkout", async (
                [FromBody] CreateCheckoutSessionBody body,
                [FromServices] IMediator mediator,
                HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out var userId))
                    return Results.Unauthorized();

                var request = new CreateCheckoutSessionRequest(userId, body.PlanId);
                var result = await mediator.Send(request);
                return Results.Json(new JsonResponse<CheckoutSessionResponse>(result), statusCode: StatusCodes.Status200OK);
            })
            .RequireAuthorization()
            .WithSummary("Create Checkout Session")
            .WithDescription("Creates a Stripe Checkout Session for the given plan. Returns a sessionUrl for redirect.");
        }
    }

    public record CreateCheckoutSessionBody(int PlanId);
}
