using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApplicationAuth.SharedModels.ResponseModels;
using MediatR;

namespace ApplicationAuth.Features.Telegram;

public static class TelegramEndpoint
{
    public static void MapTelegramEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/telegram")
            .WithTags("Telegram")
            .AllowAnonymous();

        // POST api/v1/telegram/test
        group.MapPost("/test", async ([FromBody] string text, [FromServices] IMediator mediator) =>
        {
            var response = await mediator.Send(new TestInsertMinimalCommand(text));
            return Results.Created($"/api/v1/telegram/test", new JsonResponse<TelegramMessageResponse>(response));
        })
        .WithSummary("Test telegram Insert")
        .WithDescription("Test telegram Insert");

        // POST api/v1/telegram
        group.MapPost("/", async ([FromBody] TelegramMessageRequest model, [FromServices] IMediator mediator) =>
        {
            var response = await mediator.Send(new SaveMessageMinimalCommand(model));
            return Results.Created($"/api/v1/telegram", new JsonResponse<TelegramMessageResponse>(response));
        })
        .WithSummary("Telegram message save")
        .WithDescription("Telegram message save");

        // GET api/v1/telegram/{userId}/messages
        group.MapGet("/{id}/messages", async ([FromRoute] string id, [FromServices] IMediator mediator) =>
        {
            var response = await mediator.Send(new GetMessagesMinimalQuery(id));
            return Results.Ok(new JsonResponse<IEnumerable<TelegramMessageResponse>>(response));
        })
        .WithSummary("Get all messages by userToken")
        .WithDescription("Get all messages by userToken");
    }
}
