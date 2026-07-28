# Architecture & API Suggestions

This document outlines architectural recommendations and API enhancement proposals for `Blacus.ServiceNowTableAPIWrapper`.

---

## Proposal 1: Fluent Query Parameters Builder API

### Overview
Currently, `QueryParameters` and `QueryParameters<T>` require initializing individual properties manually:
```csharp
var query = new QueryParameters<SysUser>()
{
    Limit = 50,
    View = "ess",
    Query = "active=true^emailISNOTNULL"
};
```

### Recommendation
Add fluent extension/builder methods directly to `QueryParameters` and `QueryParameters<T>`:
```csharp
public class QueryParameters
{
    public QueryParameters WithQuery(string query) { Query = query; return this; }
    public QueryParameters WithLimit(int limit) { Limit = limit; return this; }
    public QueryParameters WithView(string view) { View = view; return this; }
    public QueryParameters WithDisplayValue(string displayValue) { DisplayValue = displayValue; return this; }
    public QueryParameters WithFields(params string[] fields) { Fields = string.Join(",", fields); return this; }
    
    public QueryParameters Where(string field, string value)
    {
        string clause = $"{field}={value}";
        Query = string.IsNullOrEmpty(Query) ? clause : $"{Query}^{clause}";
        return this;
    }
}
```

### Benefits
- Matches code snippets documented in `README.md`.
- Improves query readability and chainability.

---

## Proposal 2: Structured Exception & Error Responses (`ServiceNowApiException`)

### Overview
Currently, API methods deserialize response bodies without validating HTTP status codes (such as `401 Unauthorized`, `403 Forbidden`, `404 Not Found`, `429 Too Many Requests`, or `500 Internal Server Error`). When an HTTP request fails, deserialization may either throw `JsonException` or return null/partially populated objects.

### Recommendation
Create a custom `ServiceNowApiException` inheriting from `HttpRequestException`:
```csharp
public class ServiceNowApiException : HttpRequestException
{
    public HttpStatusCode StatusCode { get; }
    public ResponseErrorMessage? ServiceNowError { get; }
    public string RawResponseBody { get; }

    public ServiceNowApiException(HttpStatusCode statusCode, ResponseErrorMessage? error, string responseBody)
        : base($"ServiceNow API error (HTTP {(int)statusCode} {statusCode}): {error?.Message ?? responseBody}")
    {
        StatusCode = statusCode;
        ServiceNowError = error;
        RawResponseBody = responseBody;
    }
}
```
Enforce status check across all client calls:
```csharp
if (!response.IsSuccessStatusCode)
{
    var errorResponse = JsonSerializer.Deserialize<RecordGetResponse<object>>(responseString, options);
    throw new ServiceNowApiException(response.StatusCode, errorResponse?.Error, responseString);
}
```

### Benefits
- Enables precise error handling in caller code (`try { ... } catch (ServiceNowApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)`).
- Captures raw response payloads for debugging.

---

## Proposal 3: Dependency Injection Extensions (`Microsoft.Extensions.DependencyInjection`)

### Overview
Modern .NET Core, .NET 8, .NET 9, and .NET 10 services configure HTTP clients using `IHttpClientFactory` and `IServiceCollection`.

### Recommendation
Add an optional extensions package or extension class (`ServiceNowTableClientExtensions`):
```csharp
public static class ServiceNowTableClientServiceCollectionExtensions
{
    public static IHttpClientBuilder AddServiceNowTableClient(
        this IServiceCollection services,
        IServiceNowCredential credentials,
        Action<HttpClient>? configureClient = null)
    {
        return services.AddHttpClient<IServiceNowTableService, ServiceNowTableClient>((client) =>
        {
            configureClient?.Invoke(client);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseCookies = false });
    }
}
```

### Benefits
- Integrates naturally into ASP.NET Core, Worker Services, and Azure Functions.
- Inherits resilient HTTP policies (Polly retries, circuit breakers, timeout handlers).
