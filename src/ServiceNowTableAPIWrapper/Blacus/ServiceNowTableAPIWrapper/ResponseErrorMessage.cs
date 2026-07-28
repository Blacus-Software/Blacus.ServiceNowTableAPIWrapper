using System.Text.Json.Serialization;

namespace Blacus.ServiceNowTableAPIWrapper
{
    /// <summary>
    /// Represents error details returned by the ServiceNow REST API.
    /// </summary>
    public class ResponseErrorMessage
    {
        /// <summary>
        /// Gets or sets the main error summary message.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        /// <summary>
        /// Gets or sets detailed error breakdown or stack information.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        [JsonPropertyName("detail")]
        public string? Detail { get; set; }
    }
}
