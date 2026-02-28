using System.Text.Json;
using System.Text.Json.Serialization;
using System;

namespace ApplicationAuth.SharedModels.ResponseModels
{
    public record UserDeviceResponseModel
    {
        [JsonRequired]
        [JsonPropertyName("userId")]
        public int UserId { get; set; }

        [JsonRequired]
        [JsonPropertyName("isVerified")]
        public bool IsVerified { get; set; }

        [JsonRequired]
        [JsonPropertyName("deviceToken")]
        public string DeviceToken { get; set; }

        [JsonRequired]
        [JsonPropertyName("addedAt")]
        public DateTime AddedAt { get; set; }
    }
}
