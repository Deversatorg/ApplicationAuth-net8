using ApplicationAuth.Features.Payments.Plans;
using ApplicationAuth.SharedModels.ResponseModels;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ApplicationAuth.Features.Payments.Plans
{
    public static class GetPlansEndpoint
    {
        public static void MapGetPlansEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/v1/plans", async ([FromServices] IMediator mediator) =>
            {
                var plans = await mediator.Send(new GetPlansQuery());
                return Results.Ok(new JsonResponse<object>(plans));
            })
            .AllowAnonymous()
            .WithSummary("Get Subscription Plans")
            .WithDescription("Returns all available subscription plans.");
        }
    }
}
