# Blacus ServiceNow TableAPI Wrapper

A modern, non-throwing, two-tier .NET client for the **ServiceNow Table REST API** (`Blacus.ServiceNowTableAPIWrapper`) targeting **.NET 8, .NET 9, and .NET 10**.

## Key Features

- **Flexible Querying (Server-Side LINQ, `TableRequestOptions`, + Client-Side LINQ):** Build server-side `sysparm_query` filters using strongly typed C# LINQ expressions (`client.Query<T>()`) or explicit `TableRequestOptions`, and freely apply standard .NET LINQ-to-Objects (`Where`, `Select`, `GroupBy`) in memory on the returned `IReadOnlyList<T>` or `IAsyncEnumerable` results.
- **Two-Tier Architecture:**
  - **Stateless Client (`IServiceNowStatelessClient`):** Zero instance state; pass any `ServiceNowConnection` per call for multi-instance/multi-tenant workloads.
  - **Stateful Client (`IServiceNowClient`):** Binds a `ServiceNowConnection` over `IServiceNowStatelessClient` for clean single-instance ergonomics.
- **Injectable & Standalone:** First-class `IHttpClientFactory` Dependency Injection registration alongside a socket-safe `StandaloneServiceNowFactory` (`PooledConnectionLifetime` + `UseCookies = false`).
- **Zero Exceptions by Design:** HTTP `400`/`401`/`403`/`404`/`429`/`500` responses—including ServiceNow's `{ error: { message, detail } }` JSON payloads—and invalid query expressions are returned as structured `ServiceNowResult<T>` and `ServiceNowPageResult<T>` values.
- **Typed `<T>` + `dynamic` Parity:** Use strongly typed C# models with declarative `[Sysparm*]` annotations, or call the exact same methods without `<T>` to work with `dynamic` (`ExpandoObject`) records on the fly.
- **Smart `ReferenceLink<T>` Hydration:** Serialize foreign keys automatically as `sys_id` strings, batch-hydrate child records automatically via `[ServiceNowEagerLoad]`, or reload referenced records anytime via `await link.LoadAsync()`.

---

## Quick Start

### 1. Define a Model with Annotations (Optional)

Model annotations lock the query parameters (`sysparm_display_value`, `sysparm_fields`, `sysparm_view`, etc.) so ServiceNow always returns JSON matching your C# class structure.

```csharp
using System.Text.Json.Serialization;
using Blacus.ServiceNowTableAPIWrapper.Abstractions;
using Blacus.ServiceNowTableAPIWrapper.Attributes;
using Blacus.ServiceNowTableAPIWrapper.Models;

[ServiceNowTable("sys_user")]
[SysparmDisplayValue(DisplayValueMode.All)]
public sealed class ServiceNowUser : IServiceNowRecord
{
    [JsonPropertyName("sys_id")]
    public string? SysId { get; set; }

    [JsonPropertyName("user_name")]
    public string? UserName { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }
}

[ServiceNowTable("incident")]
[SysparmDisplayValue(DisplayValueMode.All)]
[SysparmLimit(500)]
public sealed class Incident : IServiceNowRecord
{
    [JsonPropertyName("sys_id")]
    public string? SysId { get; set; }

    [ServiceNowReadOnly]
    [JsonPropertyName("number")]
    public string? Number { get; set; }

    [JsonPropertyName("short_description")]
    public string? ShortDescription { get; set; }

    [JsonPropertyName("priority")]
    public long? Priority { get; set; }

    [JsonPropertyName("active")]
    public bool Active { get; set; }

    // Automatically batch-fetched when Incident(s) are retrieved
    [ServiceNowEagerLoad]
    [JsonPropertyName("caller_id")]
    public ReferenceLink<ServiceNowUser>? Caller { get; set; }

    // Loaded on-demand via AssignedTo.LoadAsync()
    [JsonPropertyName("assigned_to")]
    public ReferenceLink<ServiceNowUser>? AssignedTo { get; set; }
}
```

---

### 2. Instantiation: Dependency Injection vs. Standalone

#### Dependency Injection (`IHttpClientFactory`)
```csharp
using Blacus.ServiceNowTableAPIWrapper.Authentication;
using Blacus.ServiceNowTableAPIWrapper.Configuration;

var connection = new ServiceNowConnection(
    new Uri("[https://acme.service-now.com](https://acme.service-now.com)"),
    new BasicCredentialProvider("svc_account", "secret"));

// Registers both IServiceNowStatelessClient and bound IServiceNowClient
builder.Services.AddServiceNowClient(connection, options =>
{
    options.Timeout = TimeSpan.FromSeconds(60);
    options.CaptureRawJson = false;
});
```

#### Standalone (Console Apps, Scripts, Non-DI Hosts)
```csharp
using Blacus.ServiceNowTableAPIWrapper.Authentication;
using Blacus.ServiceNowTableAPIWrapper.Configuration;

var connection = new ServiceNowConnection(
    new Uri("[https://acme.service-now.com](https://acme.service-now.com)"),
    new BearerTokenCredentialProvider(oauthAccessToken));

using var client = StandaloneServiceNowFactory.CreateClient(connection);
```

---

### 3. Querying Records (Server-Side LINQ + Post-Retrieval Client-Side LINQ)

Use `client.Query<T>()` to compile server-side `sysparm_query` filters (or call `client.ListPageAsync<T>(options)` directly), and chain standard C# LINQ-to-Objects on the retrieved results whenever needed:

```csharp
using System.Linq;
using System.Text.RegularExpressions;

var targetPriorities = new long?[] { 1, 2 };

// 1. Server-side LINQ compiles to:
// sysparm_query=active=true^priorityIN1,2^short_descriptionLIKEVPN^caller_id.emailENDSWITH@acme.com^ORDERBYDESCpriority
// sysparm_limit=50
var page = await client.Query<Incident>()
    .Where(x => x.Active && targetPriorities.Contains(x.Priority))
    .Where(x => x.ShortDescription != null && x.ShortDescription.Contains("VPN"))
    .Where(x => x.Caller!.Record!.Email!.EndsWith("@acme.com"))
    .OrderByDescending(x => x.Priority)
    .Take(50)
    .ToPageAsync();

if (!page.IsSuccess)
{
    Console.WriteLine($"Error ({page.StatusCode}): {page.Error?.Message} - {page.Error?.Detail}");
    return;
}

// 2. Post-retrieval client-side LINQ-to-Objects on the returned IReadOnlyList<Incident>
var ticketCodeRegex = new Regex(@"VPN-\d{4}", RegexOptions.Compiled);
var matchingIncidents = page.Items
    .Where(i => i.ShortDescription is not null && ticketCodeRegex.IsMatch(i.ShortDescription))
    .ToList();

foreach (var incident in matchingIncidents)
{
    // Caller was eagerly hydrated via [ServiceNowEagerLoad]
    ServiceNowUser? caller = incident.Caller;
    Console.WriteLine($"{incident.Number}: {incident.ShortDescription} (Caller: {caller?.Email})");

    // AssignedTo was not eager-loaded; LoadAsync() fetches (or reloads) it live using the bound client
    if (incident.AssignedTo is not null)
    {
        var assignedResult = await incident.AssignedTo.LoadAsync();
        if (assignedResult.IsSuccess)
        {
            Console.WriteLine($"Assigned To: {incident.AssignedTo.Record?.UserName}");
        }
    }
}
```

---

### 4. CRUD & Non-Generic `dynamic` Usage (No Model Class Required)

Every `<T>` method—including `Query`—has a 1:1 non-generic overload accepting `TableName` via `TableRequestOptions` and returning `dynamic` (`ExpandoObject`):

```csharp
using System.Linq;

var assetOptions = new TableRequestOptions { TableName = "u_custom_asset" };

// Dynamic server-side LINQ query
var assetsResult = await client.Query(assetOptions)
    .Where(row => row.Field<string>("u_asset_tag").StartsWith("AST-") && row.Field<bool>("active"))
    .OrderBy("u_asset_tag")
    .Take(50)
    .ToListAsync();

if (assetsResult.IsSuccess && assetsResult.Value is not null)
{
    // Post-retrieval client-side filtering on dynamic results
    var highCostAssets = assetsResult.Value
        .Where(a => decimal.TryParse((string?)a.u_cost, out var cost) && cost > 1000m)
        .ToList();
}

// Dynamic Create & Partial Patch
var createResult = await client.CreateAsync(
    new { u_asset_tag = "AST-9001", u_cost = "1250.00" },
    assetOptions);

if (createResult.IsSuccess)
{
    string createdId = createResult.Value!.sys_id;
    await client.PatchAsync(createdId, new { u_cost = "1100.00" }, assetOptions);
}
```