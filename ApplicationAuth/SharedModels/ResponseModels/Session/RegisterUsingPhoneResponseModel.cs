using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApplicationAuth.SharedModels.ResponseModels.Session
{
    public record RegisterUsingPhoneResponseModel
    {
        [JsonRequired]
        [JsonPropertyName("phone")]
        public string Phone { get; set; }

        [JsonRequired]
        [JsonPropertyName("sMSSent")]
        public bool SMSSent { get; set; }
    }
}
