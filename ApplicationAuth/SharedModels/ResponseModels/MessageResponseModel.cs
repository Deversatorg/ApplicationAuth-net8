using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApplicationAuth.SharedModels.ResponseModels
{
    public record MessageResponseModel
    {
        public MessageResponseModel(string message)
        {
            Message = message;
        }

        [JsonRequired]
        public string Message { get; set; }
    }
}
