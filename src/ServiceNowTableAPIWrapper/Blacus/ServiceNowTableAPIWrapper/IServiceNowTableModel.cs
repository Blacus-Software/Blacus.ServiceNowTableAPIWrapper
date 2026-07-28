namespace Blacus.ServiceNowTableAPIWrapper
{
    /// <summary>
    /// Represents a strongly-typed model mapping to a ServiceNow table record.
    /// </summary>
    public interface IServiceNowTableModel
    {
        
        /// <summary>
        /// Gets or sets the ServiceNow system identifier (<c>sys_id</c>).
        /// </summary>
        string? SysId { get; set; }

    }
}
