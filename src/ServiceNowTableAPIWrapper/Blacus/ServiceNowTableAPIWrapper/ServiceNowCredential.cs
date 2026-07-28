using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Blacus.ServiceNowTableAPIWrapper
{
    /// <summary>
    /// Contract defining ServiceNow instance credentials.
    /// </summary>
    public interface IServiceNowCredential
    {
        /// <summary>
        /// Gets or sets the target ServiceNow instance URL.
        /// </summary>
        string Url { get; set; }

        /// <summary>
        /// Gets the raw authentication string (Basic or Bearer token format).
        /// </summary>
        string BearerString { get; }
    }

    /// <summary>
    /// Represents ServiceNow authentication using a OAuth Bearer token.
    /// </summary>
    public class ServiceNowCredentialBearerToken : IServiceNowCredential
    {
        /// <summary>
        /// Gets or sets the ServiceNow instance URL.
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the OAuth Bearer token.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceNowCredentialBearerToken"/> class.
        /// </summary>
        public ServiceNowCredentialBearerToken() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceNowCredentialBearerToken"/> class with URL and token.
        /// </summary>
        /// <param name="url">ServiceNow instance URL.</param>
        /// <param name="token">OAuth Bearer token.</param>
        public ServiceNowCredentialBearerToken(string url, string token)
        {
            Url = url;
            Token = token;
        }

        private string? bearerString = null;

        /// <summary>
        /// Gets the Bearer token authorization header value.
        /// </summary>
        public string BearerString
        {
            get
            {
                if (bearerString == null) 
                    bearerString = $"Bearer {Token}";

                return bearerString;
            }
        }

        /// <summary>
        /// Deserializes a <see cref="ServiceNowCredentialBearerToken"/> from JSON.
        /// </summary>
        /// <param name="json">JSON input string.</param>
        /// <returns>A deserialized <see cref="ServiceNowCredentialBearerToken"/> or null.</returns>
        public static ServiceNowCredentialBearerToken? FromJson(string json) =>
            JsonSerializer.Deserialize<ServiceNowCredentialBearerToken>(json, JsonSerializerOptionsBuilder.Singleton);

        /// <summary>
        /// Deserializes a list of <see cref="ServiceNowCredentialBearerToken"/> from JSON.
        /// </summary>
        /// <param name="json">JSON input string.</param>
        /// <returns>A list of deserialized credentials or null.</returns>
        public static List<ServiceNowCredentialBearerToken>? FromJsonList(string json) =>
            JsonSerializer.Deserialize<List<ServiceNowCredentialBearerToken>>(json, JsonSerializerOptionsBuilder.Singleton);

        /// <summary>
        /// Serializes this credential instance to indented JSON.
        /// </summary>
        /// <returns>JSON representation of this object.</returns>
        public string ToJson() =>
            JsonSerializer.Serialize(this, JsonSerializerOptionsBuilder.Singleton);
    }

    /// <summary>
    /// Represents ServiceNow authentication using Basic authentication (username and password).
    /// </summary>
    public class ServiceNowCredentialUsername : IServiceNowCredential
    {
        /// <summary>
        /// Gets or sets the ServiceNow instance URL.
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ServiceNow username.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ServiceNow password.
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceNowCredentialUsername"/> class.
        /// </summary>
        public ServiceNowCredentialUsername() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceNowCredentialUsername"/> class with instance URL, username, and password.
        /// </summary>
        /// <param name="url">ServiceNow instance URL.</param>
        /// <param name="username">ServiceNow username.</param>
        /// <param name="password">ServiceNow password.</param>
        public ServiceNowCredentialUsername(string url, string username, string password)
        {
            Url = url;
            Username = username;
            Password = password;
        }

        private string? bearerString = null;

        /// <summary>
        /// Gets the Base64-encoded basic authentication credential string.
        /// </summary>
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

        /// <summary>
        /// Deserializes a <see cref="ServiceNowCredentialUsername"/> from JSON.
        /// </summary>
        /// <param name="json">JSON input string.</param>
        /// <returns>A deserialized <see cref="ServiceNowCredentialUsername"/> or null.</returns>
        public static ServiceNowCredentialUsername? FromJson(string json) =>
            JsonSerializer.Deserialize<ServiceNowCredentialUsername>(json, JsonSerializerOptionsBuilder.Singleton);

        /// <summary>
        /// Deserializes a list of <see cref="ServiceNowCredentialUsername"/> from JSON.
        /// </summary>
        /// <param name="json">JSON input string.</param>
        /// <returns>A list of deserialized credentials or null.</returns>
        public static List<ServiceNowCredentialUsername>? FromJsonList(string json) =>
            JsonSerializer.Deserialize<List<ServiceNowCredentialUsername>>(json, JsonSerializerOptionsBuilder.Singleton);

        /// <summary>
        /// Serializes this credential instance to indented JSON.
        /// </summary>
        /// <returns>JSON representation of this object.</returns>
        public string ToJson() =>
            JsonSerializer.Serialize(this, JsonSerializerOptionsBuilder.Singleton);
    }

    /// <summary>
    /// Legacy credential representation for Basic Authentication.
    /// </summary>
    [Obsolete("This class is obsolete. Use IServiceNowCredential or ServiceNowCredentialUsername instead.", false)]
    public class ServiceNowCredential : IServiceNowCredential
    {
        /// <summary>
        /// Gets or sets the ServiceNow instance URL.
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ServiceNow username.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ServiceNow password.
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceNowCredential"/> class.
        /// </summary>
        public ServiceNowCredential() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceNowCredential"/> class with URL, username, and password.
        /// </summary>
        /// <param name="url">ServiceNow instance URL.</param>
        /// <param name="username">ServiceNow username.</param>
        /// <param name="password">ServiceNow password.</param>
        public ServiceNowCredential(string url, string username, string password)
        {
            Url = url;
            Username = username;
            Password = password;
        }

        private string? bearerString = null;

        /// <summary>
        /// Gets the Base64-encoded basic authentication credential string.
        /// </summary>
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

        /// <summary>
        /// Deserializes a <see cref="ServiceNowCredential"/> from JSON.
        /// </summary>
        /// <param name="json">JSON input string.</param>
        /// <returns>A deserialized <see cref="ServiceNowCredential"/> or null.</returns>
        public static ServiceNowCredential? FromJson(string json) =>
            JsonSerializer.Deserialize<ServiceNowCredential>(json, JsonSerializerOptionsBuilder.Singleton);

        /// <summary>
        /// Deserializes a list of <see cref="ServiceNowCredential"/> from JSON.
        /// </summary>
        /// <param name="json">JSON input string.</param>
        /// <returns>A list of deserialized credentials or null.</returns>
        public static List<ServiceNowCredential>? FromJsonList(string json) =>
            JsonSerializer.Deserialize<List<ServiceNowCredential>>(json, JsonSerializerOptionsBuilder.Singleton);

        /// <summary>
        /// Serializes this credential instance to indented JSON.
        /// </summary>
        /// <returns>JSON representation of this object.</returns>
        public string ToJson() =>
            JsonSerializer.Serialize(this, JsonSerializerOptionsBuilder.Singleton);
    }
}
