namespace Blacus.ServiceNowTableAPIWrapper
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    public partial class ServiceNowTableClient
    {

        public async Task<TableGetResponse<T>> GetTableByURLAsync<T>(Uri link, QueryParameters queryParameters = null) where T : IServiceNowTableModel
            => await ServiceNowTableClient.GetTableByURLAsync<T>(Credentials, link);

        public static async Task<TableGetResponse<T>> GetTableByURLAsync<T>(IServiceNowCredential credential, Uri url, QueryParameters queryParameterOverride = null) where T : IServiceNowTableModel
        {

            Uri requestUri = url;
            string responseString = null;
            LinkHeader linkHeader = null;

            if (url.Query.Length == 0)
            {
                var queryParameters = new QueryParameters<T>();
                var query = queryParameters.GetQueryString(queryParameterOverride);
                requestUri = new($"{url}?{query.ToString()}");
            }

            var request = ServiceNowHttpRequestMessage.NewHttpRequestMessage(HttpMethod.Get, requestUri, credential);

            HttpResponseMessage response = ServiceNowTableClient.HttpClient.Send(request);

            responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception(responseString);

            var options = JsonSerializerOptionsBuilder.Singleton;

            TableGetResponse<T> result = null;
            result = System.Text.Json.JsonSerializer.Deserialize<TableGetResponse<T>>(responseString, options);
            result.OriginalJSON = responseString;

            IEnumerable<string> linkString = null;

            if (response.Headers.TryGetValues("Link", out linkString))
            {

                linkHeader = LinkHeader.LinksFromHeader(linkString.First());
                result.NextLink = linkHeader.NextLink;

            }

            return result;

        }

    }

}
