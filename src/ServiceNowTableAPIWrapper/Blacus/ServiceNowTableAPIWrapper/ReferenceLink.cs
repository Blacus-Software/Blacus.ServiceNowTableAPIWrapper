using System;
using System.Linq;
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

            return base.ToString() ?? string.Empty;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("value")]
        public string? Value
        {
            get
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    value = Link?.ToString()?.Split('/')?.LastOrDefault();
                }
                return value;
            }
            set => this.value = value;
        }
        private string? value;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("display_value")]
        public string? DisplayValue { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("link")]
        public Uri? Link { get; set; }

        public async Task<T?> GetRecordAsync<T>(ServiceNowTableClient serviceNow) where T : IServiceNowTableModel
        {
            if (Link == null)
                throw new InvalidOperationException("Reference link URL is null.");

            var response = await serviceNow.GetRecordByURLAsync<T>(Link);
            return response.Record;
        }
    }

    public abstract class ReferenceLink<T> : ReferenceLink where T : IServiceNowTableModel
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public T? Record { get => lazyRecord.Value; }

        private Lazy<T?> lazyRecord;

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public Func<Uri, T>? Get { get; set; }

        public ReferenceLink()
        {
            lazyRecord = new(() => Get != null && Link != null ? Get(Link) : default);
        }

        public Task<T?> GetRecordAsync(ServiceNowTableClient serviceNow) => GetRecordAsync<T>(serviceNow);
    }
}
