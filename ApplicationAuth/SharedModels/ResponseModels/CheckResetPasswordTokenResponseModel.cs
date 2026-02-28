using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApplicationAuth.SharedModels.ResponseModels
{
    public record CheckResetPasswordTokenResponseModel
    {
        [JsonRequired]
        [JsonPropertyName("isValid")]
        public bool IsValid { get; set; }
    }
}
