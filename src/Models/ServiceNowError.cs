namespace Blacus.ServiceNowTableAPIWrapper.Models;

public enum ServiceNowErrorKind
{
    None = 0,
    ApiError,                   // ServiceNow JSON error: { "error": { "message", "detail" }, "status": "failure" }
    HttpProtocolError,          // Non-2xx HTTP status with non-JSON body (e.g., proxy 502, HTML hibernation page)
    DeserializationError,       // JSON payload did not match target CLR contract
    TransportError,             // DNS/socket/TLS failure or request timeout
    InvalidRequest,             // Missing TableName, null record, unbound ReferenceLink, or missing sys_id
    UnsupportedQueryExpression  // LINQ expression contains C# constructs that cannot translate to sysparm_query
}

public sealed record ServiceNowError(
    ServiceNowErrorKind Kind,
    string? Message,
    string? Detail = null,
    string? ServerStatus = null);
