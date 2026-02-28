using System;

namespace ApplicationAuth.Features.Telegram.Models;

public record TelegramMessageRequestModel
{
    public string UserToken { get; set; }
    public string Text { get; set; }
}

public record TelegramMessageResponseModel
{
    public int Id { get; set; }
    public string Message { get; set; }
    public string SenderId { get; set; }
    public string SentAt { get; set; }
}

public record TelegramStickerResponseModel
{
    public int Id { get; set; }
    public string StickerId { get; set; }
    public string FileUniqueId { get; set; }
    public int CountOfUsage { get; set; }
    public string DateOfFirstUsage { get; set; }
}
