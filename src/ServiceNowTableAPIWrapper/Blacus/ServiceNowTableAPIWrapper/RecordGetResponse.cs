using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Blacus.ServiceNowTableAPIWrapper
{

    public class RecordGetResponse<T>
    {

        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        [JsonPropertyName("result")]
        public T? Result { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public T Record { get => Result; }

        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        [JsonPropertyName("error")]
        public ResponseErrorMessage? Error { get; set; }

        [JsonExtensionData]
        public Dictionary<string, object>? ExtensionData { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public string? SourceJSON { get; set; }

    }

}
