using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Blacus.ServiceNowTableAPIWrapper
{

    public class ReferenceLink
    {

        public override string ToString()
        {

            if (DisplayValue != null)
            {
                return DisplayValue;
            }

            return base.ToString();

        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("value")]
        public string? Value { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("display_value")]
        public string? DisplayValue { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("link")]
        public Uri? Link { get; set; }

        public async Task<T> GetRecordAsync<T>(ServiceNowTableClient serviceNow) where T : IServiceNowTableModel
        {
            return (await serviceNow.GetRecordByURLAsync<T>(Link)).Record;
        }

    }

    public class ReferenceLink<T> : ReferenceLink where T : IServiceNowTableModel
    {

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public T Record { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public async Task<T> GetRecordAsync(ServiceNowTableClient serviceNow)
        {
            Record = (await serviceNow.GetRecordByURLAsync<T>(Link)).Record;
            return Record;
        }

    }

}