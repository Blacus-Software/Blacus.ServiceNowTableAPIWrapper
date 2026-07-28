namespace Blacus.ServiceNowTableAPIWrapper
{
    /// <summary>
    /// Represents a service for interacting with the ServiceNow Table API.
    /// </summary>
    public interface IServiceNowTableService
    {
        /// <summary>
        /// Gets or sets the credentials used to authenticate against the ServiceNow instance.
        /// </summary>
        IServiceNowCredential Credentials { get; set; }
    }
}
