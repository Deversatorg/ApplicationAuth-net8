using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApplicationAuth.SharedModels.RequestModels
{
    public record OrderingRequestModel<KeyType, DirectionType>
    {
        [JsonPropertyName("key")]
        public KeyType Key { get; set; }

        [JsonPropertyName("direction")]
        public DirectionType Direction { get; set; }
    }
}
