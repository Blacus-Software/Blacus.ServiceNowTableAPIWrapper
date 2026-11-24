namespace Blacus.ServiceNowTableAPIWrapper.Models;

using System.Net;

public sealed record ServiceNowResult<T>(
    bool IsSuccess,
    HttpStatusCode? StatusCode,
    T? Value,
    ServiceNowError? Error = null,
    string? RawJson = null)
{
    
    public static ServiceNowResult<T> Success(T value, HttpStatusCode statusCode, string? rawJson = null) =>
        new(true, statusCode, value, null, rawJson);

    public static ServiceNowResult<T> Failure(
        ServiceNowErrorKind kind,
        string? message,
        string? detail = null,
        HttpStatusCode? statusCode = null,
        string? serverStatus = null,
        string? rawJson = null) =>
        new(false, statusCode, default, new ServiceNowError(kind, message, detail, serverStatus), rawJson);

}

public sealed record ServiceNowDeleteResult(
    bool IsSuccess,
    HttpStatusCode? StatusCode,
    ServiceNowError? Error = null,
    string? RawJson = null);
