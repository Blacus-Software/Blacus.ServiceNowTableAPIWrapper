using Blacus.ServiceNowTableAPIWrapper;
using System.Text.Json.Serialization;

namespace ServiceNowTableAPIWrapperTestProject.Models
{

    [SysParmDisplayValue(SysParmDisplayValueOptions.True)]
    [SysParmExcludeReferenceLink(false)]
    public class DisplayTestValue : TestModel
    {

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public static new string SampleDataInput = @"

{
  ""result"": [
    {
      ""sys_id"": ""7488ceb7479e9a9020009ebd416d4362"",
      ""string_sample"": ""Test 1234"",
      ""datetime_sample"": ""13-Dec-24 02:39:16 PM"",
      ""datetimeoffset_sample"": ""13-Dec-24 02:39:16 PM"",
      ""long_sample"": ""11"",
      ""bool_sample"": ""false""
    }
  ]
}

".Trim();

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public static new string SampleDataOutput = @"
{
  ""result"": [
    {
      ""sys_id"": ""7488ceb7479e9a9020009ebd416d4362"",
      ""string_sample"": ""Test 1234"",
      ""datetime_sample"": ""2024-12-13 14:39:16"",
      ""datetimeoffset_sample"": ""2024-12-13 14:39:16"",
      ""long_sample"": ""11"",
      ""bool_sample"": ""false""
    }
  ]
}
".Trim();

    }

}
