using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Blacus.ServiceNowTableAPIWrapper
{
    public interface IServiceNowCredential
    {
        string Url { get; set; }
        string BearerString { get; }
    }

    public class ServiceNowCredentialBearerToken : IServiceNowCredential
    {
        public string Url { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;

        public ServiceNowCredentialBearerToken() { }

        public ServiceNowCredentialBearerToken(string url, string token)
        {
            Url = url;
            Token = token;
        }

        private string? bearerString = null;
        public string BearerString
        {
            get
            {
                if (bearerString == null) 
                    bearerString = $"Bearer {Token}";

                return bearerString;
            }
        }

        public static ServiceNowCredentialBearerToken? FromJson(string json) =>
            JsonSerializer.Deserialize<ServiceNowCredentialBearerToken>(json, new JsonSerializerOptions());

        public static List<ServiceNowCredentialBearerToken>? FromJsonList(string json) =>
            JsonSerializer.Deserialize<List<ServiceNowCredentialBearerToken>>(json, new JsonSerializerOptions());

        public string ToJson() =>
            JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }

    public class ServiceNowCredentialUsername : IServiceNowCredential
    {
        public string Url { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public ServiceNowCredentialUsername() { }

        public ServiceNowCredentialUsername(string url, string username, string password)
        {
            Url = url;
            Username = username;
            Password = password;
        }

        private string? bearerString = null;
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

        public static ServiceNowCredentialUsername? FromJson(string json) =>
            JsonSerializer.Deserialize<ServiceNowCredentialUsername>(json, new JsonSerializerOptions());

        public static List<ServiceNowCredentialUsername>? FromJsonList(string json) =>
            JsonSerializer.Deserialize<List<ServiceNowCredentialUsername>>(json, new JsonSerializerOptions());

        public string ToJson() =>
            JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }

    [Obsolete("This class is obsolete. Use IServiceNowCredential or ServiceNowCredentialUsername instead.", false)]
    public class ServiceNowCredential : IServiceNowCredential
    {
        public string Url { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public ServiceNowCredential() { }

        public ServiceNowCredential(string url, string username, string password)
        {
            Url = url;
            Username = username;
            Password = password;
        }

        private string? bearerString = null;
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

        public static ServiceNowCredential? FromJson(string json) =>
            JsonSerializer.Deserialize<ServiceNowCredential>(json, new JsonSerializerOptions());

        public static List<ServiceNowCredential>? FromJsonList(string json) =>
            JsonSerializer.Deserialize<List<ServiceNowCredential>>(json, new JsonSerializerOptions());

        public string ToJson() =>
            JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }
}
