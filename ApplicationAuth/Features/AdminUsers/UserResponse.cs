using System.Text.Json.Serialization;

namespace ApplicationAuth.Features.AdminUsers;

public record UserResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("phoneNumber")] string PhoneNumber,
    [property: JsonPropertyName("firstName")] string FirstName,
    [property: JsonPropertyName("lastName")] string LastName,
    [property: JsonPropertyName("isBlocked")] bool IsBlocked
);
