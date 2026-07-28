using Blacus.ServiceNowTableAPIWrapper;
using System.Text.Json;

namespace ServiceNowTableAPIWrapperTestProject
{

    public class TestRunner<T> where T : IServiceNowTableModel
    {

        public Type Type = typeof(T);

        private string? outputResult;
        public string OutputResult
        {
            get
            {
                if (outputResult == null)
                {
                    runTest();
                }
                return outputResult ?? string.Empty;
            }
        }

        private string? outputSample { get; set; }
        public string OutputSample
        {
            get
            {
                if (outputSample == null)
                {
                    runTest();
                }
                return outputSample ?? string.Empty;
            }
        }

        public bool Matches { get => OutputResult.Equals(OutputSample); }

        private void runTest()
        {

            string inputSample = Type.GetField("SampleDataInput").GetValue(null).ToString();
            outputSample = Type.GetField("SampleDataOutput").GetValue(null).ToString();

            var inputResult = FromJSON<T>(inputSample);
            outputResult = ToJSON<T>(inputResult);

        }

        public static TableGetResponse<U>? FromJSON<U>(string json) where U : IServiceNowTableModel =>
            JsonSerializer.Deserialize<TableGetResponse<U>>(json, JsonSerializerOptionsBuilder.Singleton);

        public static string ToJSON<U>(TableGetResponse<U> data) where U : IServiceNowTableModel =>
            JsonSerializer.Serialize<TableGetResponse<U>>(data, JsonSerializerOptionsBuilder.Singleton);

    }

}
