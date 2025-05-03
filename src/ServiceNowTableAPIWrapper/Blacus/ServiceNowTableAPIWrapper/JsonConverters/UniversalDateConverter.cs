using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blacus.ServiceNowTableAPIWrapper.JsonConverters
{

    public class UniversalDateConverter<T> : JsonConverter<T>
    {

        public override bool CanConvert(Type t) => t == typeof(T) || t == typeof(T?);

        private string[] formats =
        {
            "yyyy-MM-dd",                       // "2024-04-19"
            "yyyy-MM-dd HH':'mm':'ss",          // 
            "yyyy-MM-dd hh':'mm':'ss tt",  // 
            "dd'-'MMM'-'yy' 'hh':'mm':'ss tt",  // 
            "dd-MMM-yy",                        // "19-Apr-24"
            "HH:mm:ss.fff"
        };

        private DateTimeStyles _dateTimeStyles = DateTimeStyles.RoundtripKind;
        private string? _dateTimeFormat;
        private CultureInfo? _culture;

        public DateTimeStyles DateTimeStyles
        {
            get => _dateTimeStyles;
            set => _dateTimeStyles = value;
        }

        public string? DateTimeFormat
        {
            get => _dateTimeFormat ?? string.Empty;
            set => _dateTimeFormat = (string.IsNullOrEmpty(value)) ? null : value;
        }

        public CultureInfo Culture
        {
            get => _culture ?? CultureInfo.CurrentCulture;
            set => _culture = value;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {

            var method = typeof(T).GetMethods()
                .Where(method =>
                    method.Name == "ToString" &&
                    method.GetParameters().Count() == 1 &&
                    method.GetParameters().Count(p => p.Name == "format") == 1).First();

            var text = method.Invoke(value, new object[] { "yyyy-MM-dd HH:mm:ss" });

            writer.WriteStringValue((string?)text);

        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {

            string? dateText = reader.GetString(); 

            var methods = typeof(T).GetMethods()
                .Where(method =>
                    method.Name == "TryParseExact" &&
                    method.GetParameters().Count(asd => asd.Name == "formats") > 0);

            MethodInfo method = null;

            foreach (var meth in methods)
            {
                var paramss = meth.GetParameters();
                if (
                    (paramss[0].Name == "s" || paramss[0].Name == "input") &&
                    paramss[1].Name == "formats")
                {
                    method = meth;
                    break;
                }
            }

            if (method == null)
                return default(T);

            object[] paramiters = new object[] { dateText, formats, Culture, _dateTimeStyles, null };

            var results = method.Invoke(null, paramiters);

            var toReturn = (T)paramiters.Last();

            return toReturn;

        }

        public static readonly UniversalDateConverter<T> Singleton = (UniversalDateConverter<T>)Activator.CreateInstance(typeof(UniversalDateConverter<T>));

    }

}
