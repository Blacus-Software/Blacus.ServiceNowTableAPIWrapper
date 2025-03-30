using Blacus.ServiceNowTableAPIWrapper;
using System.Text.Json.Serialization;

namespace ServiceNowTableAPIWrapperTestProject.Models
{

    [SysParmDisplayValue(SysParmDisplayValueOptions.False)]
    [SysParmExcludeReferenceLink(false)]
    public class DisplayFalseTestValue : TestModel
    {

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public static new string SampleDataInput = @"

{
  ""result"": [
    {
      ""sys_id"": ""7488ceb7479e9a9020009ebd416d4362"",
      ""string_sample"": ""Test 1234"",
      ""datetime_sample"": ""2025-03-26 23:50:22"",
      ""datetimeoffset_sample"": ""2025-03-26 23:50:22"",
      ""long_sample"": ""11"",
      ""bool_sample"": ""false""
    }
  ]
}

".Trim();

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public static new string SampleDataOutput = SampleDataInput;

    }

}
