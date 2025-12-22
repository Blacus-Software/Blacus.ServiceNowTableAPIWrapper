using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace Blacus.ServiceNowTableAPIWrapper
{

    public interface IServiceNowCredential
    {

        string Url { get; set; }
        string BearerString { get; }

    }

    public class ServiceNowCredentialBearerToken : IServiceNowCredential
    {

        public string Url { get; set; }
        public string Token { get; set; }

        public ServiceNowCredentialBearerToken() { }

        public ServiceNowCredentialBearerToken(string url, string token)
        {
            Url = url;
            Token = token;
        }

        private string bearerString = null;
        public string BearerString
        {
            get
            {

                if (bearerString == null) 
                    bearerString = $"Bearer {Token}";

                return bearerString;

            }
        }

        public static ServiceNowCredentialBearerToken FromJson(string json) =>
            JsonSerializer.Deserialize<ServiceNowCredentialBearerToken>(json, new System.Text.Json.JsonSerializerOptions());

        public static List<ServiceNowCredentialBearerToken> FromJsonList(string json) =>
            JsonSerializer.Deserialize<List<ServiceNowCredentialBearerToken>>(json, new System.Text.Json.JsonSerializerOptions());

        public string ToJson() =>
            JsonSerializer.Serialize<ServiceNowCredentialBearerToken>(this, new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });

    }

    public class ServiceNowCredentialUsername : IServiceNowCredential
    {

        public string Url { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public ServiceNowCredentialUsername() { }

        public ServiceNowCredentialUsername(string url, string username, string password)
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

        public static ServiceNowCredentialUsername FromJson(string json) =>
            JsonSerializer.Deserialize<ServiceNowCredentialUsername>(json, new System.Text.Json.JsonSerializerOptions());

        public static List<ServiceNowCredentialUsername> FromJsonList(string json) =>
            JsonSerializer.Deserialize<List<ServiceNowCredentialUsername>>(json, new System.Text.Json.JsonSerializerOptions());

        public string ToJson() =>
            JsonSerializer.Serialize<ServiceNowCredentialUsername>(this, new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });

    }


    [Obsolete("This property is obsolete. Use IServiceNowCredential instead.", false)]
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
