using System.Text.Json.Serialization;

namespace ApplicationAuth.Features.Account.Register;

public record RegisterResponse(
    [property: JsonPropertyName("email")] string Email
);
