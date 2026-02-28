using System.Text.Json;
using System.Text.Json.Serialization;
using System;
using System.ComponentModel.DataAnnotations;

namespace ApplicationAuth.SharedModels.RequestModels.Base.CursorPagination
{
    public record CursorPaginationBaseRequestModel
    {
        [JsonPropertyName("limit")]
        public int Limit { get; set; } = 10;

        [JsonPropertyName("lastId")]
        [Range(1, int.MaxValue, ErrorMessage = "{0} is invalid")]
        public int? LastId { get; set; }
    }
}
