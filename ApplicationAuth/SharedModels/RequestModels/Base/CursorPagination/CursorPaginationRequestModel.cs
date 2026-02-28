using System.Text.Json;
using System.Text.Json.Serialization;
using ApplicationAuth.SharedModels.Enums;

namespace ApplicationAuth.SharedModels.RequestModels.Base.CursorPagination
{
    public record CursorPaginationRequestModel<T> : CursorPaginationBaseRequestModel where T : struct
    {
        [JsonPropertyName("search")]
        public string Search { get; set; }

        [JsonPropertyName("order")]
        public OrderingRequestModel<T, SortingDirection> Order { get; set; }
    }
}
