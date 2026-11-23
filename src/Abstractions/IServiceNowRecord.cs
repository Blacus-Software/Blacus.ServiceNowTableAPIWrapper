namespace Blacus.ServiceNowTableAPIWrapper.Abstractions;

using System.Text.Json.Serialization;

public interface IServiceNowRecord
{
    [JsonPropertyName("sys_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? SysId { get; set; }
}
