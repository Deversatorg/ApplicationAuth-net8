using System.Text.Json.Serialization;

namespace ApplicationAuth.Features.AdminUsers;

public record UserTableRowResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("firstName")] string FirstName,
    [property: JsonPropertyName("lastName")] string LastName,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("registeredAt")] string RegisteredAt,
    [property: JsonPropertyName("isBlocked")] bool IsBlocked
);
