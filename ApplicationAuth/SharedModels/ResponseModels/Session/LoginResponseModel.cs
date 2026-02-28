using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApplicationAuth.SharedModels.ResponseModels.Session
{
    public record LoginResponseModel
    {
        [JsonPropertyName("user")]
        public UserRoleResponseModel User { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("token")]
        public TokenResponseModel Token { get; set; }
    }
}
