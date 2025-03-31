using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blacus.ServiceNowTableAPIWrapper
{

    public class DateTimeOffsetConverter : JsonConverter<DateTimeOffset>
    {

        private List<string> formats = new()
        {
            "yyyy-MM-dd",                       // "2024-04-19"
            "yyyy-MM-dd HH':'mm':'ss",          // 
            "dd'-'MMM'-'yy' 'hh':'mm':'ss tt",  // 
            "dd-MMM-yy"                         // "19-Apr-24"
        };

        public override bool CanConvert(Type t) => t == typeof(DateTimeOffset);

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

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {

            string text;

            text = value.ToString("yyyy-MM-dd HH:mm:ss");

            writer.WriteStringValue(text);

        }

        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? dateText = reader.GetString();

            DateTimeOffset toReturn = default(DateTimeOffset);
            DateTimeOffset.TryParseExact(dateText, formats.ToArray(), Culture, _dateTimeStyles, out toReturn);
            return toReturn;

            //if (string.IsNullOrEmpty(dateText) == false)
            //{
            //    if (!string.IsNullOrEmpty(_dateTimeFormat))
            //    {
            //        return DateTimeOffset.ParseExact(dateText, _dateTimeFormat, Culture, _dateTimeStyles);
            //    }
            //    else
            //    {
            //        return DateTimeOffset.Parse(dateText, Culture, _dateTimeStyles);
            //    }
            //}
            //else
            //{
            //    return default(DateTimeOffset);
            //}
        }


        public static readonly DateTimeOffsetConverter Singleton = new DateTimeOffsetConverter();

    }

}