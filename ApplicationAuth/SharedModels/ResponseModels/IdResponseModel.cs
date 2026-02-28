using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApplicationAuth.SharedModels.ResponseModels
{
    public record IdResponseModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
    }
}
