using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blacus.ServiceNowTableAPIWrapper
{

    public class ServiceNowTableModel : IServiceNowTableModel
    {

        [Key]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("sys_id")]
        [JsonPropertyOrder(-2)]
        public string? SysId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("sys_updated_on")]
        [JsonPropertyOrder(-1)]
        public virtual DateTime? SysUpdatedOn { get; set; }

        [NotMapped]
        [JsonExtensionData]
        public Dictionary<string, object>? ExtensionData { get; set; }

    }

}
