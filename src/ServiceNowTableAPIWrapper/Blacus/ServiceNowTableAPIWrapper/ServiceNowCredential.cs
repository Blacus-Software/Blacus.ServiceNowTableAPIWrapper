using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

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

        private string bearerString = null;
        public string BearerString
        {
            get
            {

                if (bearerString == null)
                {
                    string key = $"{Username}:{Password}";
                    bearerString = Convert.ToBase64String(Encoding.ASCII.GetBytes(key));
                }

                return bearerString;

            }
        }

        public static ServiceNowCredential FromJson(string json) =>
            JsonSerializer.Deserialize<ServiceNowCredential>(json, new System.Text.Json.JsonSerializerOptions());

        public static List<ServiceNowCredential> FromJsonList(string json) =>
            JsonSerializer.Deserialize<List<ServiceNowCredential>>(json, new System.Text.Json.JsonSerializerOptions());

        public string ToJson() =>
            JsonSerializer.Serialize<ServiceNowCredential>(this, new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });

    }

}
