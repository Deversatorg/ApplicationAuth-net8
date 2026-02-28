using System.Text.Json.Serialization;
using FluentValidation;

namespace ApplicationAuth.Features.Telegram;

public record TelegramMessageRequest(
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("userToken")] string UserToken
);

public class TelegramMessageRequestValidator : AbstractValidator<TelegramMessageRequest>
{
    public TelegramMessageRequestValidator()
    {
        RuleFor(x => x.UserToken)
            .NotEmpty().WithMessage("userToken field is empty");
    }
}

public record TelegramMessageResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("senderId")] string SenderId,
    [property: JsonPropertyName("sentAt")] string SentAt
);

public record TelegramStickerResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("stickerId")] string StickerId,
    [property: JsonPropertyName("fileUniqueId")] string FileUniqueId,
    [property: JsonPropertyName("countOfUsage")] int CountOfUsage,
    [property: JsonPropertyName("dateOfFirstUsage")] string DateOfFirstUsage
);
