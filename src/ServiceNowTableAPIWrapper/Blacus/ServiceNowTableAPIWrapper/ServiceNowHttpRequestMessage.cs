using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Blacus.ServiceNowTableAPIWrapper
{

    public static class ServiceNowHttpRequestMessage
    {

        public static HttpRequestMessage NewHttpRequestMessage(HttpMethod method, string url, IServiceNowCredential credential) =>
                        NewHttpRequestMessage(method, new Uri(url), credential);

        public static HttpRequestMessage NewHttpRequestMessage(HttpMethod method, Uri url, IServiceNowCredential credential)
        {

            HttpRequestMessage toReturn = new HttpRequestMessage(method, url);

            toReturn.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (credential == null)
                throw new ArgumentNullException(nameof(credential), "Credential cannot be null.");

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
