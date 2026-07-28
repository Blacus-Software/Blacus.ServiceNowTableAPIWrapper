using System.Text.Json;

namespace Blacus.ServiceNowTableAPIWrapper
{
    /// <summary>
    /// Represents a strongly-typed model mapping to a ServiceNow table record.
    /// </summary>
    public interface IServiceNowTableModel
    {
        /// <summary>
        /// Gets or sets the ServiceNow system identifier (<c>sys_id</c>).
        /// </summary>
        string? SysId { get; set; }

        /// <summary>
        /// Gets the JSON serializer options configured for this model.
        /// </summary>
        /// <returns>A <see cref="JsonSerializerOptions"/> instance.</returns>
        JsonSerializerOptions GetJsonSerializerOptions() =>
            JsonSerializerOptionsBuilder.Singleton;
    }
}
