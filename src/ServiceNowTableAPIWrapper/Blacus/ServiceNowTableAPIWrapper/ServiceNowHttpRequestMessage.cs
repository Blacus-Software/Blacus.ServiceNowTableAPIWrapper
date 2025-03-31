using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Blacus.ServiceNowTableAPIWrapper
{

    public static class ServiceNowHttpRequestMessage
    {

        public static HttpRequestMessage NewHttpRequestMessage(HttpMethod method, Uri url, ServiceNowCredential credential)
        {

            HttpRequestMessage toReturn = new HttpRequestMessage(method, url);

            toReturn.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            toReturn.Headers.Authorization = new AuthenticationHeaderValue("Basic", credential.BearerString);

            return toReturn;

        }

    }

}
