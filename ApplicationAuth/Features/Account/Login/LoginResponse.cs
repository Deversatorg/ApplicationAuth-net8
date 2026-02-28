using System.Text.Json.Serialization;
using ApplicationAuth.Features.Account.Shared;

namespace ApplicationAuth.Features.Account.Login;

public record LoginResponse(
    [property: JsonPropertyName("user")] UserResponse User,
    [property: JsonPropertyName("token")] TokenResponse Token
);
