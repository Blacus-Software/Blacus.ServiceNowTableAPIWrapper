using Blacus.ServiceNowTableAPIWrapper;
using System.Text.Json.Serialization;

namespace ServiceNowTableAPIWrapperTestProject.Models
{

    public class TestModel : ServiceNowTableModel
    {

        [JsonPropertyOrder(2)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("string_sample")]
        public string StringSample { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        [JsonPropertyName("datetime_sample")]
        [JsonPropertyOrder(3)]
        public DateTime DatetimeSample { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        [JsonPropertyName("datetimeoffset_sample")]
        [JsonPropertyOrder(4)]
        public DateTimeOffset DatetimeOffsetSample { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        [JsonPropertyName("long_sample")]
        [JsonPropertyOrder(5)]
        public long? LongSample { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("bool_sample")]
        [JsonPropertyOrder(6)]
        public bool? BoolSample { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public static string SampleDataInput;

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public static string SampleDataOutput;

    }
     
}
