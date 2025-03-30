using Blacus;
using Blacus.ServiceNowTableAPIWrapper;
using ConsoleAppServiceNowTableAPIWrapper.Models;
using ServiceNowTableAPIWrapperTestProject;
using ServiceNowTableAPIWrapperTestProject.Models;
using System.Net;

namespace ConsoleAppServiceNowTableAPIWrapper
{

    internal class Program
    {

        private static FileInfo fileInfo = new System.IO.FileInfo(Environment.GetEnvironmentVariable("SNCredPath"));
        private static ServiceNowTableClient serviceNow = new(ServiceNowCredential.FromJson(System.IO.File.ReadAllText(fileInfo.FullName)));

        static void Main(string[] args)
        {

            QueryParameters queryParameters = new QueryParameters<Contract>().GetQueryParameters();

            var query = queryParameters.GetQueryString();

            string urlString = "https://kabamdev.service-now.com/api/now/table/ast_contract?sysparm_query=sys_id=0182781c1b74d550e260a7d1ec4bcb90&sysparm_display_value=true&sysparm_exclude_reference_link=true&sysparm_fields=number,sys_id,sys_updated_on,renewal_end_date";

            Uri url = new($"https://{serviceNow.Credentials.Url}/api/now/table/{((QueryParameters)queryParameters).TableName}?{queryParameters.GetQueryString()}");
            Uri url1 = new(urlString);

            //Console.WriteLine(url.ToString()); 
            //Console.WriteLine(url1.ToString())

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, url1.ToString());// "https://kabamdev.service-now.com/api/now/table/ast_contract?sysparm_query=sys_id=0182781c1b74d550e260a7d1ec4bcb90&sysparm_display_value=true&sysparm_exclude_reference_link=true&sysparm_fields=number,sys_id,sys_updated_on,renewal_end_date");
            //var request = new HttpRequestMessage(HttpMethod.Get, url);

            request.Headers.Add("Authorization", "Basic amFydmlzX2F1dG9tYXRpb246V1FwOGNiNFQ5YjY1MzY3NG0=");
            var response = client.Send(request);

            Console.WriteLine(response.Content.ReadAsStringAsync().Result);

            var sss = serviceNow.GetTableByURLAsync<Contract>(urlString).Result;


            try
            {
                var results = serviceNow.GetAsync<Contract>(new() { Query = "sys_id=0182781c1b74d550e260a7d1ec4bcb90" }).Result;
            }
            catch (AggregateException aex)
            {
                Console.WriteLine(aex.ToString());
            }

            // Console.ReadLine();

        }



        static async void Run()
        {

            List<SysUser> users = new();
            TableGetResponse<SysUser> response = null;

            response = await serviceNow.GetAsync<SysUser>(new()
            {
                Limit = 500,
                Query = "active=true"
            });

            users.AddRange(response.Records);

            while (response.HasNext)
            {
                response = await response.GetNextPageAsync();
                users.AddRange(response.Records);
            }

            var user = users.Where(usr => usr.Location != null).First();
            var location = await user.Location.GetRecordAsync<CmnLocation>(serviceNow);
            var location2 = await user.Location.GetRecordAsync(serviceNow);

            var newLocation = new CmnLocation() { Name = Guid.NewGuid().ToString() };
            var newLocationResponse = await serviceNow.Upsert<CmnLocation>(newLocation);

            newLocationResponse.Name = $"This is a test : {Guid.NewGuid().ToString()}";
            newLocationResponse.Street = null;
            var changeResponse = await serviceNow.PatchAsync<CmnLocation>(newLocationResponse);

        }

        private static void RunParseTests()
        {
            RunTest<DisplayTestValue>();
            RunTest<DisplayTestValue>();
            RunTest<DisplayTestValue>();
            RunTest<DisplayTestValue>();
            RunTest<DisplayTestValue>();
            RunTest<DisplayTestValue>();
            RunTest<DisplayFalseTestValue>();
            RunTest<DisplayFalseTestValue>();
            RunTest<DisplayFalseTestValue>();
            RunTest<DisplayFalseTestValue>();
            RunTest<DisplayFalseTestValue>();
            RunTest<DisplayFalseTestValue>();
        }

        private static void RunTest<T>() where T : IServiceNowTableModel
        {

            var testRunner = new TestRunner<T>();

            Type type = typeof(T);

            var codeTimerBuilder = new CodeTimer(type.Name)
            {
                OnTimerStopped = (codeTimer) =>
                {

                    Console.WriteLine($"Matches: {testRunner.Matches} => {codeTimer.ElapsedMilliseconds}ms\n\n");

                }
            };

            using (var codeTimer = codeTimerBuilder.Start())
            {

                Console.WriteLine($"Type: {testRunner.Type.Name}");

                if (!testRunner.Matches)
                {
                    Console.WriteLine("result:\n" + testRunner.OutputResult);
                    Console.WriteLine("sample:\n" + testRunner.OutputSample);
                }

            }

        }

    }

}
