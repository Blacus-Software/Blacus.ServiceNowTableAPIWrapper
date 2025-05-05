using Blacus.ServiceNowTableAPIWrapper;
using System.Text.Json.Serialization;

namespace ConsoleAppServiceNowTableAPIWrapper.Models
{

    [TableName("alm_asset")]
    [SysParmDisplayValue(SysParmDisplayValueOptions.True)]
    [SysParmExcludeReferenceLink(true)]
    public class AlmAsset : ServiceNowTableModel
    {
          
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("asset_tag")]
        public string AssetTag { get; set; }

    }

}
