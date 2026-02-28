using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApplicationAuth.SharedModels.ResponseModels.Session
{
    public record RegisterResponseModel
    {
        [JsonRequired]
        [JsonPropertyName("email")]
        public string Email { get; set; }
    }
}
