# Blacus.ServiceNow

A modern, non-throwing, two-tier .NET client for the **ServiceNow Table REST API** targeting **.NET 8, .NET 9, and .NET 10**.

## Key Features

- **Two-Tier Architecture:**
  - **Stateless Client (`IServiceNowStatelessClient`):** Zero instance state; pass any `ServiceNowConnection` per call for multi-instance/multi-tenant workloads.
  - **Stateful Client (`IServiceNowClient`):** Binds a `ServiceNowConnection` over `IServiceNowStatelessClient` for clean single-instance ergonomics.
- **Injectable & Standalone:** First-class `IHttpClientFactory` Dependency Injection registration alongside a socket-safe `StandaloneServiceNowFactory` (`PooledConnectionLifetime` + `UseCookies = false`).
- **Zero Exceptions by Design:** HTTP `400`/`401`/`403`/`404`/`429`/`500` responses—including ServiceNow's `{ error: { message, detail } }` JSON payloads—are returned as structured `ServiceNowResult<T>` and `ServiceNowPageResult<T>` values.
- **Typed `<T>` + `dynamic` Parity:** Use strongly typed C# models with declarative `[Sysparm*]` annotations, or call the exact same methods without `<T>` to work with `dynamic` (`ExpandoObject`) records on the fly.
- **Smart `ReferenceLink<T>` Hydration:** Serialize foreign keys automatically as `sys_id` strings, batch-hydrate child records automatically via `[ServiceNowEagerLoad]`, or reload referenced records anytime via `await link.LoadAsync()`.

---

## Quick Start

### 1. Define a Model with Annotations (Optional)

Model annotations lock the query parameters (`sysparm_display_value`, `sysparm_fields`, `sysparm_view`, etc.) so ServiceNow always returns JSON matching your C# class structure.

```csharp
using System.Text.Json.Serialization;
using Blacus.ServiceNow.Abstractions;
using Blacus.ServiceNow.Attributes;
using Blacus.ServiceNow.Models;

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