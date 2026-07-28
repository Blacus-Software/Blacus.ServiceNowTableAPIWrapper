namespace Blacus.ServiceNowTableAPIWrapper
{
    using System.Text.Json;

    public interface IServiceNowTableModel
    {

        public string? SysId { get; set; }

        public JsonSerializerOptions GetJsonSerializerOptions() =>
           JsonSerializerOptionsBuilder.Singleton;

    }

}