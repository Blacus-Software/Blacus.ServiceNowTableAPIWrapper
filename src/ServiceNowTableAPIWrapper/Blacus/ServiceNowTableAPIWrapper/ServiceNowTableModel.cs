using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Blacus.ServiceNowTableAPIWrapper
{
    /// <summary>
    /// Base class implementation of <see cref="IServiceNowTableModel"/>.
    /// </summary>
    [Obsolete("This class is obsolete. Implement IServiceNowTableModel directly on model classes instead.", false)]
    public class ServiceNowTableModel : IServiceNowTableModel
    {
        /// <summary>
        /// Gets or sets the ServiceNow record system identifier (<c>sys_id</c>).
        /// </summary>
        [Key]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("sys_id")]
        [JsonPropertyOrder(-2)]
        public string? SysId { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the record was last updated (<c>sys_updated_on</c>).
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("sys_updated_on")]
        [JsonPropertyOrder(-1)]
        public virtual DateTime? SysUpdatedOn { get; set; }

        /// <summary>
        /// Gets or sets additional unmapped JSON properties returned by the ServiceNow API.
        /// </summary>
        [NotMapped]
        [JsonExtensionData]
        public Dictionary<string, object>? ExtensionData { get; set; }
    }
}
