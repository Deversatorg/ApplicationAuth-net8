using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApplicationAuth.SharedModels.ResponseModels.Session
{
    public record SingleTokenResponseModel
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }
    }
}
