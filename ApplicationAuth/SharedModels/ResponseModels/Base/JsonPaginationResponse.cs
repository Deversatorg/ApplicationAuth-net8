using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApplicationAuth.SharedModels.ResponseModels
{
    public record JsonPaginationResponse<T> : JsonResponse<T> where T : class
    {
        public JsonPaginationResponse(T newdata, int nextOffset, int totalCount)
            : base(newdata)
        {
            Pagination = new PaginationModel
                {
                    NextOffset = nextOffset,
                    TotalCount = totalCount
                };
        }

        [JsonRequired]
        [JsonPropertyName("pagination")]
        public PaginationModel Pagination { get; set; }
    }

    public record PaginationModel
    {
        /// <summary>
        /// request offset + length of returned array
        /// </summary>
        [JsonPropertyName("nextOffset")]
        [JsonRequired]
        public int NextOffset { get; set; }

        /// <summary>
        /// total count of items. This could be used for disabling endless scroll functionality
        /// </summary>
        [JsonPropertyName("totalCount")]
        [JsonRequired]
        public int TotalCount { get; set; }
    }
}
