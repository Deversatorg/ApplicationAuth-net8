using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApplicationAuth.SharedModels.ResponseModels.Session
{
    public record UserRoleResponseModel : UserResponseModel
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }
    }
}
