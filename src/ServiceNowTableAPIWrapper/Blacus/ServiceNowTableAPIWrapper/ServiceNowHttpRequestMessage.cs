using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Blacus.ServiceNowTableAPIWrapper
{
    /// <summary>
    /// Helper for creating pre-configured <see cref="HttpRequestMessage"/> objects with ServiceNow authentication headers.
    /// </summary>
    public static class ServiceNowHttpRequestMessage
    {
        /// <summary>
        /// Creates a new <see cref="HttpRequestMessage"/> configured with target URL, HTTP method, and credentials.
        /// </summary>
        /// <param name="method">The HTTP method.</param>
        /// <param name="url">The request URL string.</param>
        /// <param name="credential">The ServiceNow credential.</param>
        /// <returns>A configured <see cref="HttpRequestMessage"/>.</returns>
        public static HttpRequestMessage NewHttpRequestMessage(HttpMethod method, string url, IServiceNowCredential credential) =>
            NewHttpRequestMessage(method, new Uri(url), credential);

        /// <summary>
        /// Creates a new <see cref="HttpRequestMessage"/> configured with target URI, HTTP method, and credentials.
        /// </summary>
        /// <param name="method">The HTTP method.</param>
        /// <param name="url">The target URI.</param>
        /// <param name="credential">The ServiceNow credential.</param>
        /// <returns>A configured <see cref="HttpRequestMessage"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="credential"/> is null.</exception>
        public static HttpRequestMessage NewHttpRequestMessage(HttpMethod method, Uri url, IServiceNowCredential credential)
        {
            if (credential == null)
                throw new ArgumentNullException(nameof(credential), "Credential cannot be null.");

            HttpRequestMessage toReturn = new HttpRequestMessage(method, url);
            toReturn.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            switch (credential)
            {
                case ServiceNowCredentialUsername:
                    toReturn.Headers.Authorization = new AuthenticationHeaderValue("Basic", credential.BearerString);
                    break;

                case ServiceNowCredentialBearerToken:
                    toReturn.Headers.Add("Authorization", credential.BearerString);
                    break;

                default:
                    if (!string.IsNullOrWhiteSpace(credential.BearerString))
                    {
                        if (credential.BearerString.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ||
                            credential.BearerString.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                        {
                            toReturn.Headers.Add("Authorization", credential.BearerString);
                        }
                        else
                        {
                            toReturn.Headers.Authorization = new AuthenticationHeaderValue("Basic", credential.BearerString);
                        }
                    }
                    break;
            }

            return toReturn;
        }
    }
}
