using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Blacus.ServiceNowTableAPIWrapper
{

    public class TableGetResponse<T> where T : IServiceNowTableModel
    {

        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        [JsonPropertyName("result")]
        public IEnumerable<T>? Records { get; set; } 

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("error")]
        public ResponseErrorMessage? Error { get; set; }

        [JsonExtensionData]
        public Dictionary<string, object>?ExtensionData { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public string? OriginalJSON { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public string? NextLink { get; internal set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public bool HasNext { get => (NextLink != null); }

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public ServiceNowTableClient? Service { get; set; }

        public async Task<TableGetResponse<T>> GetNextPageAsync() =>
           await Service.GetTableByURLAsync<T>(new Uri(NextLink));

    }

}
