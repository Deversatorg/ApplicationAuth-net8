using System.Text.Json.Serialization;

namespace ApplicationAuth.Features.Account.Shared;

public record UserResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("userName")] string UserName,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("firstName")] string FirstName,
    [property: JsonPropertyName("lastName")] string LastName,
    [property: JsonPropertyName("isBlocked")] bool IsBlocked,
    [property: JsonPropertyName("role")] string Role
);
