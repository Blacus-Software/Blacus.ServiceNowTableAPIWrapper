using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Blacus.ServiceNowTableAPIWrapper
{

    public static class ServiceNowHttpRequestMessage
    {
         
        private static ConcurrentDictionary<string, AuthenticationHeaderValue> _authStringCache = new();

        public static HttpRequestMessage NewHttpRequestMessage(HttpMethod method, string url, ServiceNowCredential credential)
        {

            HttpRequestMessage toReturn = new(method, url);

            toReturn.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            toReturn.Headers.Authorization = GetAuth(credential.Username, credential.Password);

            return toReturn;

        }

        public static AuthenticationHeaderValue GetAuth(string username, string password)
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
