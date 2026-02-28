using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization;

namespace ApplicationAuth.SharedModels.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum UserTableColumn
    {
        Id,
        FirstName,
        LastName,
        Email,
        RegisteredAt,
        IsBlocked
    }
}
