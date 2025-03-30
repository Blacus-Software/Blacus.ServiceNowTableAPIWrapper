namespace Blacus.ServiceNowTableAPIWrapper
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    public partial class ServiceNowTableClient
    {

        public async Task<TableGetResponse<T>> GetTableByURLAsync<T>(Uri link) where T : IServiceNowTableModel
            => await ServiceNowTableClient.GetTableByURLAsync<T>(Credentials, link);

        public static async Task<TableGetResponse<T>> GetTableByURLAsync<T>(ServiceNowCredential credential, Uri url) where T : IServiceNowTableModel
        {

            string responseString = null;

            LinkHeader linkHeader = null;

            var request = ServiceNowHttpRequestMessage.NewHttpRequestMessage(HttpMethod.Get, url, credential); 

            HttpResponseMessage response = ServiceNowTableClient.Client.Send(request); 

            responseString = await response.Content.ReadAsStringAsync();

            var options = JsonSerializerOptionsBuilder.GetJsonSerializerOptionsByType<T>();

            TableGetResponse<T> result = System.Text.Json.JsonSerializer.Deserialize<TableGetResponse<T>>(responseString, options);

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
