using Blacus;
using Blacus.ServiceNowTableAPIWrapper;
using ConsoleAppServiceNowTableAPIWrapper.Models;
using ServiceNowTableAPIWrapperTestProject;
using ServiceNowTableAPIWrapperTestProject.Models;

namespace ConsoleAppServiceNowTableAPIWrapper
{

    internal class Program
    {

        private static FileInfo fileInfo = new System.IO.FileInfo(Environment.GetEnvironmentVariable("SNCredPath"));
        private static ServiceNowTableClient serviceNow = new(ServiceNowCredential.FromJson(System.IO.File.ReadAllText(fileInfo.FullName)));

        static void Main(string[] args)
        {

            Console.Clear();

            // RunParseTests();

            updateTests();

            try
            {

                var results1 = serviceNow.GetAsync<Contract1>(new() { Query = "sys_id=0182781c1b74d550e260a7d1ec4bcb90" }).Result;
                var record1 = results1.Records.FirstOrDefault();

                var results2 = serviceNow.GetAsync<Contract2>(new() { Query = "sys_id=0182781c1b74d550e260a7d1ec4bcb90" }).Result;
                var record2 = results2.Records.FirstOrDefault();

                Console.WriteLine($"First: {record1.SysUpdatedOn}\nSecond: {record2.SysUpdatedOn}");

            }
            catch (AggregateException aex)
            {
                Console.WriteLine(aex.ToString());
            }

            // Console.ReadLine();

        }

        private static void updateTests()
        {

            string sysId = "c5847925339522107d7b92ac4f5c7bf4";
            var results = serviceNow.GetAsync<GDDomain>(sysId).Result;
            var record = results.Record;

            // GDDomain record = new();
            record.Domain = $"Test - {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}";

            var updateResult = serviceNow.UpsertAsync<GDDomain>(record).Result;

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
            var newLocationResponse = await serviceNow.UpsertAsync<CmnLocation>(newLocation);

            newLocationResponse.Record.Name = $"This is a test : {Guid.NewGuid().ToString()}";
            newLocationResponse.Record.Street = null;
            var changeResponse = await serviceNow.PatchAsync<CmnLocation>(newLocationResponse.Record);

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
