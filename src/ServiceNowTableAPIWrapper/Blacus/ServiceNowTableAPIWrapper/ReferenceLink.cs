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

    public abstract class ReferenceLink<T> : ReferenceLink where T : IServiceNowTableModel
    {

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public T Record { get => lazyRecord.Value; }

        private Lazy<T> lazyRecord;

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public Func<Uri, T>? Get { get; set; }

        public ReferenceLink()
        {
            lazyRecord = new(() => Get(Link));
        }

    }

}