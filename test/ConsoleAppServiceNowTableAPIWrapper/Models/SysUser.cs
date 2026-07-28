using Blacus.ServiceNowTableAPIWrapper;
using System.Text.Json.Serialization;

namespace ConsoleAppServiceNowTableAPIWrapper.Models
{

    [TableName("sys_user")]
    //[SysParmDisplayValue("true")]
    [SysParmExcludeReferenceLink(false)]
    public class SysUser : ServiceNowTableModel
    {

        public override string ToString() =>
            $"[{UserName}] {Name}";

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("active")]
        public bool? Active { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("user_name")]
        public string? UserName { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("sys_created_by")]
        public string? SysCreatedBy { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        [JsonPropertyName("sys_created_on")]
        public DateTimeOffset SysCreatedOn { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("location")]
        [JsonConverter(typeof(ReferenceLinkConverter<CmnLocation>))]
        public ReferenceLink<CmnLocation>? Location { get; set; }

    } 

}
