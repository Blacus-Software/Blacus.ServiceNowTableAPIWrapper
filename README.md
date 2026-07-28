# Blacus.ServiceNowTableAPIWrapper

A lightweight, high-performance, dependency-free .NET library for wrapping the ServiceNow Table API. It simplifies interacting with ServiceNow tables by providing a strongly-typed, intuitive interface that abstracts away the complexities of underlying HTTP requests, authentication, automatic pagination, asynchronous record streaming, and data transformations.

Target Frameworks: `.NET 8.0`, `.NET 9.0`, `.NET 10.0`.

---

## Key Features

* **Strongly-Typed Models**: Map ServiceNow tables to standard C# classes by implementing `IServiceNowTableModel`.
* **Attribute-Driven Configuration**: Use attributes like `[TableName("...")]]`, `[SysparmLimit(...)]`, `[SysparmView(...)]`, and `[SysparmQuery(...)]` to define default query behavior directly on models.
* **Authentication Options**: Support for Basic Authentication (`ServiceNowCredentialUsername`) and OAuth Bearer Tokens (`ServiceNowCredentialBearerToken`).
* **Asynchronous Streaming (`IAsyncEnumerable<T>`)**: Stream large record sets asynchronously via `StreamAllRecordsAsync<T>` with minimal memory footprint without buffering entire result sets in memory.
* **Complete CRUD Operations**: Methods for querying (`GetAsync`), fetching all pages (`ListAllRecordsAsync`), creating (`PostAsync`), updating (`PatchAsync`), and upserting (`UpsertAsync`).
* **Custom JSON Converters**: Built-in converters handling ServiceNow data types:
  * Reference links (`ReferenceLink<T>`) supporting lazy-loading and direct async record navigation.
  * Universal date/time parsing (`DateTime`, `DateTimeOffset`, `DateOnly`, and `TimeOnly`).
  * Flexible string-to-boolean parsing (`ParseBoolStringConverter`).
  * String-to-long numeric parsing (`LongConverter`).
* **Integrated Caching**: Optional caching layer (`IServiceNowCacheService`) to compare record metadata (`sys_updated_on`) and avoid redundant API requests.
* **Zero External Dependencies**: Relies exclusively on standard .NET runtime capabilities (`System.Text.Json`, `System.Net.Http`).

---

## Getting Started

### Installation

Add `Blacus.ServiceNowTableAPIWrapper` to your .NET project via NuGet:

```bash
dotnet add package Blacus.ServiceNowTableAPIWrapper
```

---

## Authentication

The library supports multiple authentication mechanisms via `IServiceNowCredential`:

### Basic Authentication
```csharp
using Blacus.ServiceNowTableAPIWrapper;

var credentials = new ServiceNowCredentialUsername(
    "https://your-instance.service-now.com",
    "your-username",
    "your-password"
);
```

### OAuth Bearer Token Authentication
```csharp
using Blacus.ServiceNowTableAPIWrapper;

var credentials = new ServiceNowCredentialBearerToken(
    "https://your-instance.service-now.com",
    "your-oauth-bearer-token"
);
```

---

## Defining ServiceNow Table Models

Create a class representing your ServiceNow table. The class must implement `IServiceNowTableModel`, which defines the `SysId` property. Use `[TableName]` (or `[ServiceNowTableName]`) and standard System.Text.Json attributes to configure mappings.

```csharp
using System.Text.Json.Serialization;
using Blacus.ServiceNowTableAPIWrapper;

[TableName("sys_user")]
[SysparmLimit(100)]
[SysparmView("ess")]
public class SysUser : IServiceNowTableModel
{
    [JsonPropertyName("sys_id")]
    public string? SysId { get; set; }

    [JsonPropertyName("user_name")]
    public string? UserName { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("active")]
    [JsonConverter(typeof(ParseBoolStringConverter))]
    public bool Active { get; set; }

    // Reference field mapping
    [JsonPropertyName("department")]
    [JsonConverter(typeof(ReferenceLinkConverter<CmnDepartment>))]
    public ReferenceLink<CmnDepartment>? Department { get; set; }
}

[TableName("cmn_department")]
public class CmnDepartment : IServiceNowTableModel
{
    [JsonPropertyName("sys_id")]
    public string? SysId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
```

### Table & Query Attributes

Decorate model classes with attributes to declare default query parameters:

| Attribute | Parameter | Description |
| :--- | :--- | :--- |
| `[TableName("...")]` | Table Name | Specifies the ServiceNow table endpoint (e.g. `sys_user`, `incident`). |
| `[SysparmLimit(int)]` | `sysparm_limit` | Default maximum record count per page. |
| `[SysparmView("...")]` | `sysparm_view` | ServiceNow form/list view filter. |
| `[SysparmQuery("...")]` | `sysparm_query` | Default encoded ServiceNow query string (e.g. `active=true^emailISNOTNULL`). |
| `[SysParmDisplayValue(Options)]` | `sysparm_display_value` | Set display value mode (`True`, `False`, or `All`). |
| `[SysparmNoCount("true")]` | `sysparm_no_count` | Suppresses total record count computation for faster query performance. |
| `[SysParmFieldsDisableAuto]` | `sysparm_fields` | Disables automatic parameter generation from property `[JsonPropertyName]` declarations. |

---

## Usage Examples

### 1. Initializing the Client

```csharp
using System;
using Blacus.ServiceNowTableAPIWrapper;

var credentials = new ServiceNowCredentialUsername(
    "https://your-instance.service-now.com",
    "admin",
    "password"
);

var client = new ServiceNowTableClient(credentials);
```

### 2. Querying a Single Record by `sys_id`

```csharp
RecordGetResponse<SysUser> response = await client.GetAsync<SysUser>("6816f79cc0a8016401c5a33be04be441");

if (response.Record != null)
{
    Console.WriteLine($"Found User: {response.Record.UserName} ({response.Record.Email})");
}
```

### 3. Querying Paged Records with Parameter Overrides

```csharp
var overrides = new QueryParameters
{
    Query = "active=true^emailISNOTNULL",
    Limit = 25
};

TableGetResponse<SysUser> page = await client.GetAsync<SysUser>(overrides);

if (page.Records != null)
{
    foreach (var user in page.Records)
    {
        Console.WriteLine($"{user.UserName}: {user.Email}");
    }
}
```

### 4. Asynchronous Low-Memory Streaming (`StreamAllRecordsAsync`)

Stream large tables record-by-record asynchronously across automatically followed pagination links:

```csharp
var query = new QueryParameters
{
    Query = "active=true",
    Limit = 500
};

await foreach (SysUser user in client.StreamAllRecordsAsync<SysUser>(query))
{
    Console.WriteLine($"Processing {user.UserName}...");
}
```

### 5. Fetching All Records (`ListAllRecordsAsync`)

Retrieve all matching records across all pagination pages in a single `List<T>`:

```csharp
var query = new QueryParameters
{
    Query = "active=true"
};

List<SysUser> allUsers = await client.ListAllRecordsAsync<SysUser>(query);
Console.WriteLine($"Total active users retrieved: {allUsers.Count}");
```

---

## Data Modification (CRUD)

### Creating a Record (`PostAsync`)

```csharp
var newUser = new SysUser
{
    UserName = "jdoe",
    Email = "jdoe@example.com",
    Active = true
};

RecordGetResponse<SysUser> created = await client.PostAsync<SysUser>(newUser);
Console.WriteLine($"Created User SysID: {created.Record?.SysId}");
```

### Updating a Record (`PatchAsync`)

```csharp
var updateUser = new SysUser
{
    SysId = "6816f79cc0a8016401c5a33be04be441",
    Email = "john.doe.updated@example.com"
};

RecordGetResponse<SysUser> updated = await client.PatchAsync<SysUser>(updateUser);
```

### Upserting a Record (`UpsertAsync`)

Automatically executes HTTP `POST` if `SysId` is null, or `PATCH` if `SysId` is present:

```csharp
var userRecord = new SysUser
{
    SysId = existingSysIdOrNull,
    UserName = "jdoe",
    Email = "jdoe@example.com"
};

RecordGetResponse<SysUser> result = await client.UpsertAsync<SysUser>(userRecord);
```

---

## Caching Support

Reduce ServiceNow API queries by leveraging `IServiceNowCacheService`:

```csharp
var cacheService = new ServiceNowCacheService
{
    CacheGet = (key) => MyMemoryCache.Get(key),
    CacheUpdate = (key, json) => MyMemoryCache.Set(key, json),
    Compare = (cached, live) => false // Return true if cached record is still valid
};

List<SysUser> cachedUsers = await client.GetWithCacheAsync<SysUser>(cacheService);
```

---

## Dependency Injection & Custom HTTP Client

Inject a custom `HttpClient` or `IHttpClientFactory` instance into `ServiceNowTableClient`:

```csharp
public class ServiceNowIntegrationService
{
    private readonly ServiceNowTableClient _client;

    public ServiceNowIntegrationService(HttpClient httpClient)
    {
        var credentials = new ServiceNowCredentialBearerToken(
            "https://your-instance.service-now.com",
            "bearer-token"
        );

        _client = new ServiceNowTableClient(credentials, httpClient);
    }
}
```

---

## License

This project is licensed under the terms of the [MIT License](LICENSE).
