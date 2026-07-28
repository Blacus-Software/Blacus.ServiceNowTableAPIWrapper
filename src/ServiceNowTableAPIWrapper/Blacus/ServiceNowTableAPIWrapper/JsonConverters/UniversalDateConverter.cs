using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blacus.ServiceNowTableAPIWrapper.JsonConverters
{
    /// <summary>
    /// Universal JSON converter for parsing various ServiceNow date and time string formats into <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The date type (<see cref="DateTime"/>, <see cref="DateTimeOffset"/>, or <see cref="DateOnly"/>).</typeparam>
    public class UniversalDateConverter<T> : JsonConverter<T>
    {
        /// <summary>
        /// Determines whether the specified type can be converted.
        /// </summary>
        public override bool CanConvert(Type t) => t == typeof(T) || t == typeof(T?);

        private static readonly string[] Formats =
        {
            "yyyy-MM-dd",                       // "2024-04-19"
            "yyyy-MM-dd HH':'mm':'ss",          
            "yyyy-MM-dd hh':'mm':'ss tt",  
            "dd'-'MMM'-'yy' 'hh':'mm':'ss tt",  
            "dd-MMM-yy",                        // "19-Apr-24"
            "HH:mm:ss.fff"
        };

        private static readonly MethodInfo? ToStringMethod = typeof(T).GetMethods()
            .FirstOrDefault(method =>
                method.Name == "ToString" &&
                method.GetParameters().Length == 1 &&
                method.GetParameters()[0].Name == "format");

        private static readonly MethodInfo? TryParseExactMethod = GetTryParseExactMethod();

        private static MethodInfo? GetTryParseExactMethod()
        {
            var methods = typeof(T).GetMethods()
                .Where(method =>
                    method.Name == "TryParseExact" &&
                    method.GetParameters().Any(p => p.Name == "formats"));

            foreach (var meth in methods)
            {
                var paramss = meth.GetParameters();
                if (paramss.Length >= 2 &&
                    (paramss[0].Name == "s" || paramss[0].Name == "input") &&
                    paramss[1].Name == "formats")
                {
                    return meth;
                }
            }
            return null;
        }

        private DateTimeStyles _dateTimeStyles = DateTimeStyles.RoundtripKind;
        private string? _dateTimeFormat;
        private CultureInfo? _culture;

        /// <summary>
        /// Gets or sets the <see cref="DateTimeStyles"/> used during date parsing.
        /// </summary>
        public DateTimeStyles DateTimeStyles
        {
            get => _dateTimeStyles;
            set => _dateTimeStyles = value;
        }

        /// <summary>
        /// Gets or sets the custom date time format string.
        /// </summary>
        public string? DateTimeFormat
        {
            get => _dateTimeFormat ?? string.Empty;
            set => _dateTimeFormat = (string.IsNullOrEmpty(value)) ? null : value;
        }

        /// <summary>
        /// Gets or sets the culture used during date parsing.
        /// </summary>
        public CultureInfo Culture
        {
            get => _culture ?? CultureInfo.CurrentCulture;
            set => _culture = value;
        }

        /// <summary>
        /// Writes a date value to JSON formatted as <c>yyyy-MM-dd HH:mm:ss</c>.
        /// </summary>
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            if (ToStringMethod != null)
            {
                var text = ToStringMethod.Invoke(value, new object[] { "yyyy-MM-dd HH:mm:ss" });
                writer.WriteStringValue((string?)text);
            }
            else
            {
                writer.WriteStringValue(value.ToString());
            }
        }

        /// <summary>
        /// Reads and converts JSON to <typeparamref name="T"/>.
        /// </summary>
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? dateText = reader.GetString();
            if (string.IsNullOrEmpty(dateText) || TryParseExactMethod == null)
            {
                return default!;
            }

            object?[] parameters = new object?[] { dateText, Formats, Culture, _dateTimeStyles, null };
            TryParseExactMethod.Invoke(null, parameters);

            return (T)parameters.Last()!;
        }

        /// <summary>
        /// Singleton instance of <see cref="UniversalDateConverter{T}"/>.
        /// </summary>
        public static readonly UniversalDateConverter<T> Singleton = (UniversalDateConverter<T>)Activator.CreateInstance(typeof(UniversalDateConverter<T>))!;
    }
}
