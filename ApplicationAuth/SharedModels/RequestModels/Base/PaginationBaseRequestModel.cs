using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApplicationAuth.SharedModels.RequestModels
{
    public record PaginationBaseRequestModel
    {
        [JsonPropertyName("limit")]
        public int Limit { get; set; } = 10;

        [JsonPropertyName("offset")]
        public int Offset { get; set; } = 0;
    }
}
