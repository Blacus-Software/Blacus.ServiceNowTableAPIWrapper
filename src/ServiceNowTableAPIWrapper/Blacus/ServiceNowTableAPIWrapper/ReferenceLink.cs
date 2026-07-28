using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Blacus.ServiceNowTableAPIWrapper
{
    /// <summary>
    /// Represents a ServiceNow reference link containing a value, display value, and resource URI.
    /// </summary>
    public class ReferenceLink
    {
        /// <summary>
        /// Returns a string representation of the reference link (display value or base string).
        /// </summary>
        /// <returns>A string representation.</returns>
        public override string ToString()
        {
            if (DisplayValue != null)
            {
                return DisplayValue;
            }

            return base.ToString() ?? string.Empty;
        }

        private string? value;

        /// <summary>
        /// Gets or sets the target record system identifier (<c>sys_id</c>).
        /// </summary>
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

        /// <summary>
        /// Gets or sets the display text for the referenced field.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("display_value")]
        public string? DisplayValue { get; set; }

        /// <summary>
        /// Gets or sets the direct API URL for the referenced record.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("link")]
        public Uri? Link { get; set; }

        /// <summary>
        /// Asynchronously fetches the referenced record using the specified ServiceNow client.
        /// </summary>
        /// <typeparam name="T">The referenced model type implementing <see cref="IServiceNowTableModel"/>.</typeparam>
        /// <param name="serviceNow">The ServiceNow client instance.</param>
        /// <returns>The referenced model instance or null.</returns>
        public async Task<T?> GetRecordAsync<T>(ServiceNowTableClient serviceNow) where T : IServiceNowTableModel
        {
            if (Link == null)
                throw new InvalidOperationException("Reference link URL is null.");

            var response = await serviceNow.GetRecordByURLAsync<T>(Link);
            return response.Record;
        }
    }

    /// <summary>
    /// Strongly-typed generic variant of <see cref="ReferenceLink"/>.
    /// </summary>
    /// <typeparam name="T">The model type representing the referenced table.</typeparam>
    public abstract class ReferenceLink<T> : ReferenceLink where T : IServiceNowTableModel
    {
        /// <summary>
        /// Gets the lazily loaded referenced record instance.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public T? Record { get => lazyRecord.Value; }

        private Lazy<T?> lazyRecord;

        /// <summary>
        /// Gets or sets the delegate function used to load the target record from its URI.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public Func<Uri, T>? Get { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceLink{T}"/> class.
        /// </summary>
        public ReferenceLink()
        {
            lazyRecord = new(() => Get != null && Link != null ? Get(Link) : default);
        }

        /// <summary>
        /// Asynchronously retrieves the typed record.
        /// </summary>
        /// <param name="serviceNow">The ServiceNow table client instance.</param>
        /// <returns>The referenced model instance or null.</returns>
        public Task<T?> GetRecordAsync(ServiceNowTableClient serviceNow) => GetRecordAsync<T>(serviceNow);
    }
}
