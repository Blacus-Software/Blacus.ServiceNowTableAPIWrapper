using System.Text.Json.Serialization;

namespace Blacus.ServiceNowTableAPIWrapper
{

    public class ResponseErrorMessage
    {

        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        [JsonPropertyName("message")]
        public string? Message { get; set; }


        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        [JsonPropertyName("detail")]
        public string? Detail { get; set; }

    }

}