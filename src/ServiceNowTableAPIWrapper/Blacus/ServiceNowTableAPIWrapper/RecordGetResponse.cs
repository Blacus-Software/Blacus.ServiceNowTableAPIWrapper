using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Blacus.ServiceNowTableAPIWrapper
{
    /// <summary>
    /// Represents an API response wrapper returning a single record.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    public class RecordGetResponse<T> 
    {

        /// <summary>
        /// Gets or sets the target deserialized record instance.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        [JsonPropertyName("result")]
        public T? Record { get; set; }

        /// <summary>
        /// Gets or sets the HTTP response status string.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        /// <summary>
        /// Gets or sets error details if the response indicated a failure.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        [JsonPropertyName("error")]
        public ResponseErrorMessage? Error { get; set; }

        /// <summary>
        /// Gets or sets unmapped extra JSON elements.
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object>? ExtensionData { get; set; }

        /// <summary>
        /// Gets or sets the raw source JSON text.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public string? SourceJSON { get; set; }
    }
}
