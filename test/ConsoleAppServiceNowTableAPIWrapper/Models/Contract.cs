using Blacus.ServiceNowTableAPIWrapper;
using System.Text.Json.Serialization;

namespace ConsoleAppServiceNowTableAPIWrapper.Models
{

    [TableName("ast_contract")]
    [SysParmDisplayValue(SysParmDisplayValueOptions.True)]
    [SysParmExcludeReferenceLink(true)]
    public class Contract : ServiceNowTableModel
    {
         
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("number")]
        public string Number { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("renewal_end_date")]
        public string RenewalEndDate { get; set; }        

    }

}
