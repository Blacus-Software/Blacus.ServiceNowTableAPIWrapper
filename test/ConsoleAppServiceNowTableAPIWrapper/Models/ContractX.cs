using Blacus.ServiceNowTableAPIWrapper;
using System.Text.Json.Serialization;

namespace ConsoleAppServiceNowTableAPIWrapper.Models
{

    [TableName("ast_contract")]
    [SysParmDisplayValue(SysParmDisplayValueOptions.True)]
    [SysParmExcludeReferenceLink(true)]
    public class Contract1 : ServiceNowTableModel
    {
         
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("number")]
        public string Number { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("renewal_end_date")]
        public DateTime? RenewalEndDate { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("u_manufacture")]
        public string Manufacture { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("short_description")]
        public string ShortDescription { get; set; }

    }

    [TableName("ast_contract")]
    [SysParmDisplayValue(SysParmDisplayValueOptions.False)]
    [SysParmExcludeReferenceLink(true)]
    public class Contract2 : ServiceNowTableModel
    {

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("number")]
        public string Number { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("renewal_end_date")]
        public DateTime? RenewalEndDate { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("u_manufacture")]
        public string Manufacture { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("short_description")]
        public string ShortDescription { get; set; }

    }

}
