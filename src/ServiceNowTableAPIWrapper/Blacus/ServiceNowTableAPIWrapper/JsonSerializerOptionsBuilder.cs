using Blacus.ServiceNowTableAPIWrapper.JsonConverters;
using System;
using System.Text.Json;

namespace Blacus.ServiceNowTableAPIWrapper
{

    public class JsonSerializerOptionsBuilder
    {

        private static Lazy<JsonSerializerOptions> serializerOptions = new(() =>
        new(JsonSerializerDefaults.General)
        {
            Converters =
            {
                        ParseBoolStringConverter.Singleton,
                        ReferenceLinkConverter.Singleton,
                        LongConverter.Singleton,
                        UniversalDateConverter<DateTime>.Singleton,
                        UniversalDateConverter<DateTimeOffset>.Singleton,
                        UniversalDateConverter<DateOnly>.Singleton,
            },
            WriteIndented = true
        });

        public static JsonSerializerOptions Singleton { get => serializerOptions.Value; }

        //public static JsonSerializerOptions GetJsonSerializerOptionsByType(Type objectType)
        //{

        //    JsonSerializerOptions toReturn = new(JsonSerializerDefaults.General)
        //    {
        //        Converters =
        //            {
        //                ParseBoolStringConverter.Singleton,
        //                ReferenceLinkConverter.Singleton,
        //                LongConverter.Singleton,
        //                DateTimeConverter.Singleton,
        //                DateTimeOffsetConverter.Singleton
        //            },
        //        WriteIndented = true
        //    };

        //    return toReturn;

        //    //lock (jsonSerializerOptions)
        //    //{

        //    //    string keyName = objectType.FullName;

        //    //    if (jsonSerializerOptions.ContainsKey(keyName))
        //    //    {
        //    //        return jsonSerializerOptions[objectType.FullName];
        //    //    }

        //    //    JsonSerializerOptions toReturn = new(JsonSerializerDefaults.General)
        //    //    {
        //    //        Converters = 
        //    //        {
        //    //            ParseBoolStringConverter.Singleton,
        //    //            ReferenceLinkConverter.Singleton,
        //    //            LongConverter.Singleton,
        //    //            DateTimeConverter.Singleton,
        //    //            DateTimeOffsetConverter.Singleton
        //    //        },
        //    //        WriteIndented = true
        //    //    };

        //    //    string displayValue = objectType.GetCustomAttribute<SysParmDisplayValue>()?.Value;

        //    //    switch (displayValue?.ToLower())
        //    //    {
        //    //        case "true":
        //    //            toReturn.Converters.Add(DisplayDateTimeOffsetConverter.Singleton);
        //    //            // toReturn.Converters.Add(DisplayDateTimeConverter.Singleton);
        //    //            break;

        //    //        case "all":
        //    //            break;

        //    //        case "false":
        //    //        default:
        //    //            // toReturn.Converters.Add(DateTimeConverter.Singleton);
        //    //            toReturn.Converters.Add(DateTimeOffsetConverter.Singleton);
        //    //            break;
        //    //    }

        //    //    jsonSerializerOptions.Add(keyName, toReturn);

        //    //    return toReturn;

        //    //}

        //}

    }

}
