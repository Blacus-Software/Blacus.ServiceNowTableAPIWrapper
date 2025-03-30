using Blacus;
using Blacus.ServiceNowTableAPIWrapper;
using ConsoleAppServiceNowTableAPIWrapper.Models;
using ServiceNowTableAPIWrapperTestProject;
using ServiceNowTableAPIWrapperTestProject.Models;

namespace ConsoleAppServiceNowTableAPIWrapper
{

    internal class Program
    {

        private static readonly string url = "";
        private static readonly string username = "";
        private static readonly string password = "";

        static ServiceNowTableClient serviceNow = new(new ServiceNowCredential(url, username, password));

        static void Main(string[] args)
        {

            // Run();

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

            // Console.ReadLine();

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

    }

}
