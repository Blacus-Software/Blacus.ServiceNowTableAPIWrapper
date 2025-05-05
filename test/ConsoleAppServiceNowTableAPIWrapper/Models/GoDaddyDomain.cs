using Blacus.ServiceNowTableAPIWrapper;
using System.Text.Json.Serialization;

namespace ConsoleAppServiceNowTableAPIWrapper.Models
{

    [TableName("x_kaba2_godaddy_domains")]
    [SysParmDisplayValue(SysParmDisplayValueOptions.True)]
    [SysParmExcludeReferenceLink(true)]
    public class GDDomain : ServiceNowTableModel
    {

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("domain")]
        public string Domain { get; set; }

    }

}
