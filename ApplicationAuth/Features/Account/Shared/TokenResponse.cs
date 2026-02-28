using System.Text.Json.Serialization;

namespace ApplicationAuth.Features.Account.Shared;

public record TokenResponse(
    [property: JsonPropertyName("accessToken")] string AccessToken,
    [property: JsonPropertyName("refreshToken")] string RefreshToken,
    [property: JsonPropertyName("expireDate")] string ExpireDate,
    [property: JsonPropertyName("type")] string Type
);
