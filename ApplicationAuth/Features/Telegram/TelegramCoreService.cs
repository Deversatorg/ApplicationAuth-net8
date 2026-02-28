using ApplicationAuth.Features.Telegram.Models;
using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.DAL.Abstract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ApplicationAuth.Features.Telegram
{
    public class TelegramCoreService : ITelegramCoreService
    {
        private readonly string _apiKey;
        private static TelegramBotClient client;
        private readonly IServiceProvider _serviceProvider;
        public TelegramCoreService(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            using var cts = new CancellationTokenSource();
            _apiKey = configuration["TelegramApiKey"];

            client = new TelegramBotClient(_apiKey);
            var receiverOptions = new global::Telegram.Bot.Polling.ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>() };
            client.StartReceiving(_handleUpdateAsync, _handleErrorAsync, receiverOptions, cts.Token);

            _serviceProvider = serviceProvider;
        }

        public async Task SendMessage(int chatId, string text)
        {
            try
            {
                await client.SendTextMessageAsync(chatId, text);
            }
            catch (Exception)
            {
                throw new ApplicationException();
            }
        }

        private Task _handleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        private async Task _handleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = _botOnMessageReceived(botClient, update.Message);

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await _handleErrorAsync(botClient, exception, cancellationToken);
            }
        }

        private async Task _botOnMessageReceived(ITelegramBotClient botClient, Message message)
        {
            if (message != null)
            {
                await RegisterTelegramUser(message.From.Id.ToString(), message.From.Username);
            }

            if (message != null && message.Type == MessageType.Sticker) //???????? ?????????? ??????? ? ???? ??? ??????????
            {
                using var scope = _serviceProvider.CreateScope();
                var telegramService = scope.ServiceProvider.GetRequiredService<ITelegramService>();
                await telegramService.SaveStickerRate(message.Sticker.FileUniqueId, message.Sticker.FileId);
                await client.SendStickerAsync(new ChatId(message.From.Id), new global::Telegram.Bot.Types.InputFiles.InputOnlineFile(message.Sticker.FileId));
            }
            if (message == null || message.Type != MessageType.Text || (message.Type == MessageType.Text && message.Text == "/start"))
                return;

            if (message.Type == MessageType.Text && message.Text == "/top10")
            {
                using var scope = _serviceProvider.CreateScope();
                var telegramService = scope.ServiceProvider.GetRequiredService<ITelegramService>();
                var stickers = await telegramService.GetTopStickers();
                foreach (var sticker in stickers)
                {
                    if (sticker.FileUniqueId == null || !sticker.FileUniqueId.Any()) continue;
                    await client.SendStickerAsync(new ChatId(message.From.Id), new global::Telegram.Bot.Types.InputFiles.InputOnlineFile(sticker.StickerId));
                }
                return;
            }

            var model = new TelegramMessageRequestModel()
            {
                UserToken = message.From.Id.ToString(),
                Text = $"{message.Text ?? "empty text"} - {DateTime.Now}"
            };

            var id = message.From.Id;
            var name = message.From.Username;

            await SendMessage((int)id, model.Text);

            using var msgScope = _serviceProvider.CreateScope();
            var ts = msgScope.ServiceProvider.GetRequiredService<ITelegramService>();
            await ts.SaveMessage(model);

        }

        private async Task<ApplicationUser> RegisterTelegramUser(string telegramId, string userName)
        {
            using var scope = _serviceProvider.CreateScope();
            var dataContext = scope.ServiceProvider.GetRequiredService<IDataContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            
            ApplicationUser user = dataContext.Set<ApplicationUser>().FirstOrDefault(x => x.TelegramId.ToLower() == telegramId.ToLower());

            if (user != null)
                return user;

            user = new ApplicationUser
            {
                TelegramId = telegramId,
                UserName = userName,
                IsActive = true,
                RegistratedAt = DateTime.UtcNow,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };

            var result = await userManager.CreateAsync(user, "Welcome1");

            if (!result.Succeeded) return null;

            result = await userManager.AddToRoleAsync(user, Role.User);
            
            return user;
        }

    }
}
