using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApplicationAuth.SharedModels.ResponseModels
{
    public record JsonResponse<T>
    {
        public JsonResponse(T newdata)
        {
            Data = newdata;
        }

        [JsonRequired]
        [JsonPropertyName("_v")]
        public string Version { get; set; } = "1.0";

        //[JsonRequired]
        [JsonPropertyName("data")]
        public T Data { get; set; }
    }
}
