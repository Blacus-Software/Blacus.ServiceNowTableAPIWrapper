using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Blacus.ServiceNowTableAPIWrapper
{

    public static class ServiceNowHttpRequestMessage
    {

        private static MediaTypeWithQualityHeaderValue mediaTypeWithQualityHeaderValue = new MediaTypeWithQualityHeaderValue("application/json");
        private static ConcurrentDictionary<string, AuthenticationHeaderValue> _authStringCache = new();

        public static HttpRequestMessage NewHttpRequestMessage(HttpMethod get, Uri url, ServiceNowCredential credential)
        {

            HttpRequestMessage toReturn = new(get, url);

            toReturn.Headers.Accept.Add(mediaTypeWithQualityHeaderValue);
            toReturn.Headers.Authorization = GetAuth(credential.Username, credential.Password);

            return toReturn;

        }

        private static AuthenticationHeaderValue GetAuth(string username, string password)
        {

            string key = $"{username}:{password}";

            AuthenticationHeaderValue toReturn = null;

            if (!_authStringCache.TryGetValue(key, out toReturn))
            {
                
                toReturn = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(key)));
                _authStringCache[key] = toReturn;
            
                return toReturn;
        
            }

            return toReturn;

        }

    }

}
