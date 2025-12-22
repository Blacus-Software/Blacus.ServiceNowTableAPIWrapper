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

            switch (credential)
            {
                case null:
                    throw new ArgumentNullException(nameof(credential), "Credential cannot be null.");
                default:
                    break;

                case ServiceNowCredentialUsername:
                    toReturn.Headers.Authorization = new AuthenticationHeaderValue("Basic", credential.BearerString);
                    break;

                case ServiceNowCredentialBearerToken:
                    toReturn.Headers.Add("Authorization", credential.BearerString);
                    break;
            }


            return toReturn;

        }

    }

}
