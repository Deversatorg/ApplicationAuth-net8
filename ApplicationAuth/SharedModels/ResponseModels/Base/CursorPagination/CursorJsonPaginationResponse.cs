using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApplicationAuth.SharedModels.ResponseModels.Base.CursorPagination
{
    public record CursorJsonPaginationResponse<T> : JsonResponse<T> where T : class
    {
        public CursorJsonPaginationResponse(T newdata, int? lastId) 
            : base(newdata)
        {

            Pagination = new CursorPaginationModel()
            {
                LastId = lastId
            };

        }
        [JsonRequired]
        [JsonPropertyName("pagination")]
        public CursorPaginationModel Pagination { get; set; }
    }

    public record CursorPaginationModel
    {
        [JsonPropertyName("lastId")]
        public int? LastId { get; set; }
    }
}
