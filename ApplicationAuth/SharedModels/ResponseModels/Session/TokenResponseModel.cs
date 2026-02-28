using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApplicationAuth.SharedModels.ResponseModels.Session
{
    public record TokenResponseModel
    {
        [JsonRequired]
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; }

        [JsonRequired]
        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; }

        [JsonRequired]
        [JsonPropertyName("expireDate")]
        public string ExpireDate { get; set; }

        [JsonRequired]
        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
}
