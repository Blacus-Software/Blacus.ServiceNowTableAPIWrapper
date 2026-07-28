# Known Issues & Bug Reports

This document tracks known issues and architectural bugs identified in `Blacus.ServiceNowTableAPIWrapper`.

---

## Bug 1: Attribute Name Mismatches in QueryParameters Attribute Reflection

- **File:** `src/ServiceNowTableAPIWrapper/Blacus/ServiceNowTableAPIWrapper/QueryParameters.cs`
- **Impact:** High (Custom model attributes silently fail to apply to API requests)
- **Status:** Unfixed (Documented)

### Description
In `QueryParameters<T>`, properties query class attributes applied to model types `T` using helper methods `getAttributeValueString(string name)` and `getAttributeValueInt(string name)`.

These helper methods attempt to find custom attributes by comparing `attrib.GetType().Name == name`. However, the string arguments passed in `QueryParameters<T>` do not match the C# class names defined in `TableAttributes.cs`:

| Property in `QueryParameters<T>` | String Passed to Helper | Actual Attribute Class Name in `TableAttributes.cs` | Match Result |
| :--- | :--- | :--- | :--- |
| `QueryCategory` | `"sysparm_query_category"` | `SysparmQueryCategory` | **Mismatch (False)** |
| `QueryNoDomain` | `"sysparm_query_no_domain"` | `SysparmQueryNoDomain` | **Mismatch (False)** |
| `SuppressPaginationHeader` | `"sysparm_suppress_pagination_header"` | `SysparmSuppressPaginationHeader` | **Mismatch (False)** |

### Consequence
When a developer decorates a model class with `[SysparmQueryCategory("...")]`, `[SysparmQueryNoDomain("...")]`, or `[SysparmSuppressPaginationHeader("...")]`, the attribute is never recognized during request parameter generation. The query parameter is omitted from the resulting ServiceNow HTTP API request.

### Recommended Fix
Refactor `QueryParameters<T>` attribute lookups from runtime string name comparisons to strongly typed generic queries:
```csharp
private TAttribute? GetAttribute<TAttribute>() where TAttribute : Attribute
{
    return path.OfType<TAttribute>().FirstOrDefault();
}
```
And replace helper property getters with:
```csharp
[JsonPropertyName("sysparm_query_category")]
public string? QueryCategory => GetAttribute<SysparmQueryCategory>()?.Value;
```

---

## Bug 2: NullReferenceException in TableGetResponse.GetNextPageAsync()

- **File:** `src/ServiceNowTableAPIWrapper/Blacus/ServiceNowTableAPIWrapper/TableGetResponse.cs`
- **Impact:** High (Automatic pagination via `GetNextPageAsync()` fails at runtime)
- **Status:** Unfixed (Documented)

### Description
`TableGetResponse<T>` includes a `GetNextPageAsync()` method intended to simplify paginated result fetching:

```csharp
public async Task<TableGetResponse<T>> GetNextPageAsync() =>
    await Service.GetTableByURLAsync<T>(new Uri(NextLink));
```

The method relies on the internal `Service` property:
```csharp
[JsonIgnore(Condition = JsonIgnoreCondition.Always)]
public ServiceNowTableClient? Service { get; set; }
```

However, when `TableGetResponse<T>` is deserialized from HTTP responses in `ServiceNowTableClient.GetAsync` or `GetTableByURLAsync`, the `Service` property is never populated or assigned.

### Consequence
Invoking `response.GetNextPageAsync()` on any deserialized `TableGetResponse<T>` throws a `NullReferenceException` at runtime because `Service` is `null`.

Additionally, even if `Service` were manually attached by caller code before calling `GetNextPageAsync()`, `GetTableByURLAsync` does not assign `Service` on the returned next page instance, breaking chained pagination (`page1.GetNextPageAsync().GetNextPageAsync()`).

### Recommended Fix
1. In `ServiceNowTableClient.GetTableByURLAsync<T>` (and `GetAsync`), populate `result.Service = client` or `result.Service = this` before returning.
2. In `GetTableByURLAsync`, ensure `Service` is propagated to the newly deserialized `TableGetResponse<T>` instance.
