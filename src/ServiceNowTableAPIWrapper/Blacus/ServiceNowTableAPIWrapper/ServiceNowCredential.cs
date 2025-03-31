using System.Net.Http.Headers;
using System.Text;
using System;
using System.Text.Json;
using System.Collections.Concurrent;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace Blacus.ServiceNowTableAPIWrapper
{

    public class ServiceNowCredential
    {

        public string Url { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public ServiceNowCredential() { }

        public ServiceNowCredential(string url, string username, string password)
        {
            Url = url;
            Username = username;
            Password = password;
        }

        public string BearerString
        {
            get
            {

                string key = $"{Username}:{Password}";

                string toReturn = null;

                if (!_authStringCache.TryGetValue(key, out toReturn))
                {

                    toReturn = Convert.ToBase64String(Encoding.ASCII.GetBytes(key));
                    _authStringCache[key] = toReturn;

                    return toReturn;

                }

                return toReturn;

            }
        }

        private static ConcurrentDictionary<string, string> _authStringCache = new();

        public static ServiceNowCredential FromJson(string json) =>
            JsonSerializer.Deserialize<ServiceNowCredential>(json, new System.Text.Json.JsonSerializerOptions() );

        public string ToJson() =>
            JsonSerializer.Serialize<ServiceNowCredential>(this, new System.Text.Json.JsonSerializerOptions(){ WriteIndented = true }  );

    }

}
