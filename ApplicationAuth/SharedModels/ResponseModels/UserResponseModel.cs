using System.Text.Json;
using System.Text.Json.Serialization;
using ApplicationAuth.SharedModels.ResponseModels.Session;

namespace ApplicationAuth.SharedModels.ResponseModels
{
    public record UserResponseModel : UserBaseResponseModel
    {
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        [JsonPropertyName("isBlocked")]
        public bool IsBlocked { get; set; }

    }
}
