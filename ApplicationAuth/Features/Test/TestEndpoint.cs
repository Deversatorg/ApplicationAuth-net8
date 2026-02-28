using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ApplicationAuth.Common.Exceptions;
using ApplicationAuth.Features.Account.Shared;
using ApplicationAuth.DAL.Abstract;
using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.Domain.Entities.Telegram;
using ApplicationAuth.SharedModels.ResponseModels;
using ApplicationAuth.SharedModels.ResponseModels.Session;

namespace ApplicationAuth.Features.Test;

public record ShortAuthorizationRequestModel(int? Id, string UserName);

public static class TestEndpoint
{
    public static void MapTestEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/test")
            .WithTags("Test")
            .AllowAnonymous();

        // POST api/v1/test/authorize
        group.MapPost("/authorize", async (
            [FromBody] ShortAuthorizationRequestModel model,
            [FromServices] IDataContext dataContext,
            [FromServices] IJWTService jwtService) =>
        {
            IQueryable<ApplicationUser> users = dataContext.Set<ApplicationUser>();

            if (model.Id.HasValue)
                users = users.Where(x => x.Id == model.Id);
            else if (!string.IsNullOrEmpty(model.UserName))
                users = users.Where(x => x.UserName == model.UserName);

            var user = await users.Include(x => x.Profile).FirstOrDefaultAsync();

            if (user == null)
            {
                throw new CustomException(System.Net.HttpStatusCode.NotFound, "", "User is not found");
            }

            var result = await jwtService.BuildLoginResponse(user);

            return Results.Ok(new JsonResponse<LoginResponseModel>(result));
        })
        .WithSummary("For Swagger UI")
        .WithDescription("Authorize without credentials for testing");

        // DELETE api/v1/test/DeleteAccount
        group.MapDelete("/DeleteAccount", async (
            [FromQuery] int userId,
            [FromServices] IDataContext dataContext) =>
        {
            var user = await dataContext.Set<ApplicationUser>()
                .Include(u => u.UserRoles)
                .Include(u => u.Profile)
                .Include(u => u.Tokens)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null)
            {
                dataContext.Set<ApplicationUser>().Remove(user);
                dataContext.SaveChanges();
            }

            return Results.Ok(new JsonResponse<MessageResponseModel>(new MessageResponseModel("User has been deleted")));
        })
        .WithSummary("Hard delete user from db")
        .WithDescription("Hard delete user from db");

        // DELETE api/v1/test/stickerRate
        group.MapDelete("/stickerRate", (
            [FromServices] IDataContext dataContext) =>
        {
            var stickers = dataContext.Set<TelegramSticker>().ToList();

            foreach (var sticker in stickers)
            {
                dataContext.Set<TelegramSticker>().Remove(sticker);
            }
            dataContext.SaveChanges();

            return Results.Ok(new JsonResponse<MessageResponseModel>(new MessageResponseModel("Stickers was deleted from db")));
        })
        .WithSummary("Hard delete telegramStickers from db")
        .WithDescription("Hard delete telegramStickers from db");
    }
}
