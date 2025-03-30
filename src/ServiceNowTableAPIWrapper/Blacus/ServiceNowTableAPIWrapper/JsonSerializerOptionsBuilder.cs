using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;

namespace Blacus.ServiceNowTableAPIWrapper
{

    public class JsonSerializerOptionsBuilder
    {

        public static JsonSerializerOptions GetJsonSerializerOptionsByType<T>() =>
            GetJsonSerializerOptionsByType(typeof(T));

        private static Dictionary<string, JsonSerializerOptions> jsonSerializerOptions = new();

        public static JsonSerializerOptions GetJsonSerializerOptionsByType(Type objectType)
        {

            lock (jsonSerializerOptions)
            {

                string keyName = objectType.FullName;

                if (jsonSerializerOptions.ContainsKey(keyName))
                {
                    return jsonSerializerOptions[objectType.FullName];
                }

                JsonSerializerOptions toReturn = new(JsonSerializerDefaults.General)
                {
                    Converters = {
                    ParseBoolStringConverter.Singleton,
                    ReferenceLinkConverter.Singleton,
                    LongConverter.Singleton,
                },
                    WriteIndented = true
                };

                string displayValue = objectType.GetCustomAttribute<SysParmDisplayValue>()?.Value;

                switch (displayValue?.ToLower())
                {
                    case "true":
                        toReturn.Converters.Add(DisplayDateTimeOffsetConverter.Singleton);
                        toReturn.Converters.Add(DisplayDateTimeConverter.Singleton);
                        break;

                    case "all":
                        break;

                    case "false":
                    default:
                        toReturn.Converters.Add(DateTimeConverter.Singleton);
                        toReturn.Converters.Add(DateTimeOffsetConverter.Singleton);
                        break;
                }

                jsonSerializerOptions.Add(keyName, toReturn);

                return toReturn;

            }

        }

    }

}
