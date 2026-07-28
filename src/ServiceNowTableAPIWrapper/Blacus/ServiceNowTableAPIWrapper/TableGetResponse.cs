using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Blacus.ServiceNowTableAPIWrapper
{
    /// <summary>
    /// Represents an API response wrapper returning a list/page of table records.
    /// </summary>
    /// <typeparam name="T">The record model type implementing <see cref="IServiceNowTableModel"/>.</typeparam>
    public class TableGetResponse<T> where T : IServiceNowTableModel
    {
        /// <summary>
        /// Gets or sets the collection of deserialized records returned by ServiceNow.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        [JsonPropertyName("result")]
        public IEnumerable<T>? Records { get; set; }

        /// <summary>
        /// Gets or sets the response status string.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        /// <summary>
        /// Gets or sets error details if the query failed.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("error")]
        public ResponseErrorMessage? Error { get; set; }

        /// <summary>
        /// Gets or sets unmapped extra JSON elements.
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object>? ExtensionData { get; set; }

        /// <summary>
        /// Gets or sets the raw original JSON text of the response.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public string? OriginalJSON { get; set; }

        /// <summary>
        /// Gets or sets the pagination URL link for retrieving the next page of records.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public string? NextLink { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether a subsequent results page is available.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public bool HasNext => NextLink != null;

        /// <summary>
        /// Gets or sets the client instance associated with this response for automated pagination.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public ServiceNowTableClient? Service { get; set; }

        /// <summary>
        /// Asynchronously fetches the next page of records using <see cref="NextLink"/>.
        /// </summary>
        /// <returns>A <see cref="TableGetResponse{T}"/> containing the next page of records.</returns>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="Service"/> or <see cref="NextLink"/> is null.</exception>
        public async Task<TableGetResponse<T>> GetNextPageAsync() =>
            Service != null && NextLink != null
                ? await Service.GetTableByURLAsync<T>(new Uri(NextLink))
                : throw new InvalidOperationException("Service client or NextLink is null.");
    }
}
