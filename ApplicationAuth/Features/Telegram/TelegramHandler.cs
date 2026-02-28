using ApplicationAuth.Common.Exceptions;
using ApplicationAuth.Common.Extensions;
using ApplicationAuth.DAL.Abstract;
using ApplicationAuth.Domain.Entities.Telegram;
using ApplicationAuth.Features.Telegram.Models;

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using System.Threading;
using MediatR;

namespace ApplicationAuth.Features.Telegram;

// Note: Inheriting from ITelegramService so TelegramCoreService can still inject it seamlessly.
public class TelegramHandler : ITelegramService,
    IRequestHandler<TestInsertMinimalCommand, TelegramMessageResponse>,
    IRequestHandler<SaveMessageMinimalCommand, TelegramMessageResponse>,
    IRequestHandler<GetMessagesMinimalQuery, IEnumerable<TelegramMessageResponse>>
{
    private readonly IDataContext _dataContext;

    public TelegramHandler(IDataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public Task<TelegramMessageResponseModel> TestInsert(string text)
    {
        // Old wrapper for TestInsert
        var message = new TelegramMessage
        {
            Message = text,
            UserToken = "0",
            DateTime = DateTime.UtcNow
        };

        _dataContext.Set<TelegramMessage>().Add(message);
        _dataContext.SaveChanges();

        return Task.FromResult(new TelegramMessageResponseModel
        {
            Id = message.Id,
            Message = message.Message,
            SenderId = message.UserToken,
            SentAt = message.DateTime.ToISO()
        });
    }

    public Task<TelegramMessageResponseModel> SaveMessage(TelegramMessageRequestModel model)
    {
        var message = new TelegramMessage
        {
            Message = model.Text,
            UserToken = model.UserToken,
            DateTime = DateTime.UtcNow
        };

        _dataContext.Set<TelegramMessage>().Add(message);
        _dataContext.SaveChanges();

        return Task.FromResult(new TelegramMessageResponseModel
        {
            Id = message.Id,
            Message = message.Message,
            SenderId = message.UserToken,
            SentAt = message.DateTime.ToISO()
        });
    }

    public async Task<TelegramMessageResponse> Handle(SaveMessageMinimalCommand request, CancellationToken cancellationToken)
    {
        var message = new TelegramMessage
        {
            Message = request.Model.Text,
            UserToken = request.Model.UserToken,
            DateTime = DateTime.UtcNow
        };

        _dataContext.Set<TelegramMessage>().Add(message);
        await _dataContext.SaveChangesAsync(cancellationToken);

        return new TelegramMessageResponse(
            message.Id,
            message.Message,
            message.UserToken,
            message.DateTime.ToISO()
        );
    }

    public async Task<TelegramMessageResponse> Handle(TestInsertMinimalCommand request, CancellationToken cancellationToken)
    {
        return await Handle(new SaveMessageMinimalCommand(new TelegramMessageRequest(request.Text, "0")), cancellationToken);
    }

    public Task<IEnumerable<TelegramMessageResponseModel>> GetMessagesByUserToken(string userToken)
    {
        if (string.IsNullOrEmpty(userToken))
            throw new CustomException(HttpStatusCode.BadRequest, "userToken", "Invalid userToken value");

        var messages = _dataContext.Set<TelegramMessage>().Where(x => x.UserToken == userToken).ToList();

        return Task.FromResult(messages.Select(x => new TelegramMessageResponseModel
        {
            Id = x.Id,
            Message = x.Message,
            SenderId = x.UserToken,
            SentAt = x.DateTime.ToISO()
        }));
    }

    public async Task<IEnumerable<TelegramMessageResponse>> Handle(GetMessagesMinimalQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.UserToken))
            throw new CustomException(HttpStatusCode.BadRequest, "userToken", "Invalid userToken value");

        var messages = await _dataContext.Set<TelegramMessage>().Where(x => x.UserToken == request.UserToken).ToListAsync(cancellationToken);

        return messages.Select(x => new TelegramMessageResponse(
            x.Id,
            x.Message,
            x.UserToken,
            x.DateTime.ToISO()
        ));
    }

    private async Task<TelegramSticker> SaveStickerInternal(string fileUniqueId, string stickerId)
    {
        var sticker = new TelegramSticker
        {
            FileUniqueId = fileUniqueId,
            StickerId = stickerId,
            DateTime = DateTime.UtcNow,
            CountOfUsage = 1
        };

        _dataContext.Set<TelegramSticker>().Add(sticker);
        _dataContext.SaveChanges();
        return sticker;
    }

    public Task<IEnumerable<TelegramStickerResponseModel>> GetTopStickers()
    {
        var stickers = _dataContext.Set<TelegramSticker>()
            .OrderByDescending(x => x.CountOfUsage)
            .Take(10)
            .ToList();

        return Task.FromResult(stickers.Select(x => new TelegramStickerResponseModel
        {
            Id = x.Id,
            StickerId = x.StickerId,
            FileUniqueId = x.FileUniqueId,
            CountOfUsage = x.CountOfUsage,
            DateOfFirstUsage = x.DateTime.ToISO()
        }));
    }

    public async Task<TelegramStickerResponseModel> SaveStickerRate(string fileUniqueId, string stickerId)
    {
        if (string.IsNullOrEmpty(fileUniqueId))
            throw new CustomException(HttpStatusCode.BadRequest, "stickerId", "Invalid stickerId");

        var sticker = _dataContext.Set<TelegramSticker>().FirstOrDefault(x => x.FileUniqueId == fileUniqueId);

        if (sticker == null)
        {
            sticker = await SaveStickerInternal(fileUniqueId, stickerId);
        }
        else
        {
            sticker.CountOfUsage++;
            _dataContext.Set<TelegramSticker>().Update(sticker);
            _dataContext.SaveChanges();
        }

        return new TelegramStickerResponseModel
        {
            Id = sticker.Id,
            StickerId = sticker.StickerId,
            FileUniqueId = sticker.FileUniqueId,
            CountOfUsage = sticker.CountOfUsage,
            DateOfFirstUsage = sticker.DateTime.ToISO()
        };
    }
}
