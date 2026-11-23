# Technical Specification: Blacus ServiceNow TableAPI Wrapper

## 1. Architectural Invariants (Hard-Lock Rules)

Every file, interface, and implementation in this repository MUST strictly adhere to the following non-negotiable rules:

1. **Zero-Exception Transport, Validation, LINQ Translation, & Deserialization Contract:**
   - Neither Tier 1 (`IServiceNowStatelessClient`), Tier 2 (`IServiceNowClient`), the LINQ Queryables (`IServiceNowQueryable<T>` / `IServiceNowDynamicQueryable`), nor `ReferenceLink` methods may throw exceptions for HTTP error status codes (`4xx`/`5xx`), network/timeout failures, input validation failures, unsupported LINQ expressions, dynamic binder conversions, or JSON payload mismatches.
   - `HttpResponseMessage.EnsureSuccessStatusCode()` is strictly forbidden.
   - Every operation MUST return a structured result envelope (`ServiceNowResult<T>`, `ServiceNowResult<dynamic>`, `ServiceNowPageResult<T>`, `ServiceNowPageResult<dynamic>`, or `ServiceNowDeleteResult`) exposing `IsSuccess`, `StatusCode`, `Error` (`ServiceNowError`), and the deserialized payload.
   - Missing required inputs (e.g., missing `TableName` on a non-generic call, null record payload, missing `Value`/`Link` on `ReferenceLink.LoadAsync`, or missing `sys_id` on `Patch`/`Replace`/`Delete`) MUST return `IsSuccess = false` with `ServiceNowErrorKind.InvalidRequest`.
   - Unsupported C# expressions inside `IServiceNowQueryable` server-side predicates/orderings MUST NOT silently fall back to internal full-table downloads and MUST NOT throw `NotSupportedException`; they MUST fail fast before sending an HTTP request and return `IsSuccess = false` with `ServiceNowErrorKind.UnsupportedQueryExpression`.
   - Constructor/DI guardrails (`ArgumentNullException` for null constructor dependencies) and explicit caller `CancellationToken` cancellation (`OperationCanceledException`) are the ONLY permitted exceptions.
2. **Optional Server-Side LINQ Query Builder + Unencumbered Post-Retrieval LINQ:**
   - Querying via `Query<T>()` / `Query(...)` is an optional fluent builder alongside direct `TableRequestOptions` methods (`ListPageAsync`, `ListAllAsync`, `StreamPagesAsync`, `StreamRecordsAsync`).
   - All operators on `IServiceNowQueryable<T>` and `IServiceNowDynamicQueryable` (`Where`, `OrderBy`, `OrderByDescending`, `ThenBy`, `ThenByDescending`, `SelectFields`, `Take`, `Skip`) compile strictly into server-side ServiceNow Table API query parameters (`sysparm_query`, `sysparm_fields`, `sysparm_limit`, `sysparm_offset`) via `ExpressionVisitor` prior to sending the HTTP request.
   - Once results are materialized (`ServiceNowPageResult<T>.Items`, `ServiceNowResult<IReadOnlyList<T>>.Value`, or `IAsyncEnumerable`), they expose standard `IReadOnlyList<T>` / `IEnumerable<T>` / `IAsyncEnumerable` collections so callers can freely apply any post-retrieval client-side LINQ-to-Objects filtering, projection, grouping, or custom C# predicates in memory.
3. **Strict Two-Tier Composition (Stateless -> Stateful):**
   - **Tier 1 (`IServiceNowStatelessClient`):** Holds an `HttpClient` and zero instance/credential state. Every method requires a `ServiceNowConnection` parameter.
   - **Tier 2 (`IServiceNowClient` — Stateful):** Holds a bound `ServiceNowConnection` and MUST compose `IServiceNowStatelessClient` via constructor injection. Tier 2 MUST NOT reference `System.Net.Http`, `HttpClient`, `HttpRequestMessage`, `HttpResponseMessage`, or `JsonSerializer`.
4. **1:1 Typed `<T>` and Non-Generic `dynamic` Parity:**
   - Every CRUD/List/Stream/Query method on `IServiceNowStatelessClient` and `IServiceNowClient` that accepts `<T>` MUST have a corresponding non-generic overload with an identical parameter signature (minus `<T>`), returning `dynamic` (`ExpandoObject` at runtime) instead of `T`.
   - Non-generic `dynamic` payloads MUST deserialize JSON objects into `ExpandoObject` (`IDictionary<string, object?>`), JSON arrays into `List<object?>`, and primitives into native CLR types (`string`, `bool`, `long`, `double`, `null`). Raw `JsonElement` values MUST NOT leak into `dynamic` results.
5. **Zero Caching:**
   - Neither Tier 1 nor Tier 2 caches table queries, metadata stamps, or cross-request records. Every client call executes live against ServiceNow.
6. **Annotation-First Wire Contract with `TableRequestOptions` Overrides:**
   - For generic `<T>` calls, class annotations (`[ServiceNowTable]`, `[SysparmDisplayValue]`, `[SysparmExcludeReferenceLink]`, `[SysparmFields]`, etc.) define the baseline `TableRequestOptions` for `T` so the returned JSON structure always matches `T`'s schema. Any non-null property in the per-call `TableRequestOptions? options` parameter (or translated LINQ clause) overrides/merges with the annotation default for that single request.
   - For non-generic (`dynamic`) calls, `options?.TableName` provides the target table name (except URI/Link-based calls where the URI already contains the path).
   - All class and property metadata (`TableMetadata<T>`) MUST be extracted once per closed generic type `T` and cached statically—never via string-based reflection on hot paths.
7. **Client-Bound `ReferenceLink` & `ReferenceLink<T>` (`[ServiceNowEagerLoad]` & `LoadAsync`):**
   - `ReferenceLink<T>` inherits from `ReferenceLink` (`Value`, `DisplayValue`, `Link`) and exposes a pure in-memory `[JsonIgnore] public T? Record { get; set; }` property with implicit conversion operators (`T?` <-> `ReferenceLink<T>`). Accessing `.Record` MUST NEVER trigger hidden synchronous network I/O.
   - Both `ReferenceLink` and `ReferenceLink<T>` hold an internal `[JsonIgnore]` client context (`IServiceNowStatelessClient` + `ServiceNowConnection`) attached automatically when the parent record is deserialized.
   - Decorating a `ReferenceLink<T>` property with `[ServiceNowEagerLoad]` causes `ServiceNowStatelessClient` to batch-populate `.Record` (`sys_idIN...`) immediately when the parent record/page is retrieved. Unannotated `ReferenceLink<T>` properties leave `.Record` as `null` upon initial parent retrieval.
   - Calling `await link.LoadAsync(...)` ALWAYS executes a fresh network fetch against ServiceNow regardless of `[ServiceNowEagerLoad]` or whether `.Record` is already populated, updates `.Record` when `IsSuccess` is `true`, and returns `ServiceNowResult<T>` (or `ServiceNowResult<dynamic>`).
   - Outbound JSON serialization (`POST`/`PUT`/`PATCH`) of both `ReferenceLink` and `ReferenceLink<T>` MUST transmit ONLY the raw `sys_id` string (`Value ?? Record?.SysId`), or `null` if empty—never a JSON object.
8. **Dual-Mode Instantiation Parity (`IHttpClientFactory` & Standalone):**
   - **Injectable Mode:** Registered via `IServiceCollection.AddServiceNowClient(...)` or `AddServiceNowStatelessClient(...)` leveraging `IHttpClientFactory`.
   - **Standalone Mode:** Instantiated via `StandaloneServiceNowFactory` backed by `SocketsHttpHandler` (`PooledConnectionLifetime = TimeSpan.FromMinutes(2)`, `UseCookies = false`).
   - `UseCookies = false` MUST always be enforced on the primary HTTP message handler in both modes.
   - `IServiceNowStatelessClient` and `IServiceNowClient` MUST NOT inherit `IDisposable`; standalone factory methods return dedicated `IStandaloneServiceNowStatelessClient` / `IStandaloneServiceNowClient` interfaces that extend `IDisposable`.
9. **Async & Cancellation Discipline:**
   - Every network method MUST be asynchronous, end with `Async`, use `.ConfigureAwait(false)`, and accept `CancellationToken cancellationToken = default` as its final parameter.

---

## 2. Target Frameworks & Project Metadata

- **Project Title:** `Blacus ServiceNow TableAPI Wrapper`
- **PackageId / Assembly:** `Blacus.ServiceNowTableAPIWrapper`
- **Root Namespace:** `Blacus.ServiceNowTableAPIWrapper`
- **TargetFrameworks:** `net8.0;net9.0;net10.0`
- **Language Version:** Latest (`Nullable = enable`, `ImplicitUsings = disable`, `TreatWarningsAsErrors = true`, `GenerateDocumentationFile = true`)
- **NuGet Dependencies:**
  - `Microsoft.Extensions.Http`
  - `Microsoft.Extensions.Http.Resilience`
  - `Microsoft.Extensions.Options`
  - `Microsoft.CSharp`

---

## 3. Repository & Directory Structure

```text
Blacus.ServiceNowTableAPIWrapper/
├── SPEC.md
├── AGENTS.md
├── README.md
├── src/
│   └── ServiceNowTableAPIWrapper/
│       ├── ServiceNowTableAPIWrapper.csproj
│       ├── Abstractions/
│       │   ├── IServiceNowRecord.cs
│       │   ├── IServiceNowStatelessClient.cs
│       │   ├── IServiceNowClient.cs
│       │   ├── IServiceNowQueryable.cs
│       │   ├── IServiceNowClientFactory.cs
│       │   ├── IStandaloneServiceNowClient.cs
│       │   └── IServiceNowCredentialProvider.cs
│       ├── Attributes/
│       │   ├── ServiceNowTableAttribute.cs
│       │   ├── SysparmAttributes.cs
│       │   ├── ServiceNowReadOnlyAttribute.cs
│       │   └── ServiceNowEagerLoadAttribute.cs
│       ├── Authentication/
│       │   ├── ServiceNowConnection.cs
│       │   ├── BasicCredentialProvider.cs
│       │   ├── BearerTokenCredentialProvider.cs
│       │   └── DynamicBearerCredentialProvider.cs
│       ├── Configuration/
│       │   ├── ServiceNowTransportOptions.cs
│       │   ├── ServiceNowServiceCollectionExtensions.cs
│       │   └── StandaloneServiceNowFactory.cs
│       ├── Linq/
│       │   ├── ServiceNowQueryable.cs
│       │   ├── ServiceNowDynamicQueryable.cs
│       │   ├── ServiceNowRow.cs
│       │   └── Internal/
│       │       ├── ServiceNowExpressionTranslator.cs
│       │       ├── PartialEvaluator.cs
│       │       └── DisjunctiveNormalFormRewriter.cs
│       ├── Metadata/
│       │   ├── TableMetadata.cs
│       │   └── DynamicPayloadInspector.cs
│       ├── Models/
│       │   ├── DisplayValueMode.cs
│       │   ├── ReferenceLink.cs
│       │   ├── PaginationLinks.cs
│       │   ├── ServiceNowError.cs
│       │   ├── ServiceNowResult.cs
│       │   ├── ServiceNowPageResult.cs
│       │   └── TableRequestOptions.cs
│       ├── Serialization/
│       │   ├── ServiceNowJsonOptions.cs
│       │   └── Converters/
│       │       ├── DynamicRecordConverter.cs
│       │       ├── ServiceNowBoolConverter.cs
│       │       ├── ServiceNowInt64Converter.cs
│       │       ├── ServiceNowDateConverters.cs
│       │       ├── ReferenceLinkConverterFactory.cs
│       │       └── ReadOnlyPropertyModifier.cs
│       ├── Stateless/
│       │   ├── ServiceNowStatelessClient.cs
│       │   └── Internal/
│       │       ├── ReferenceLinkHydrator.cs
│       │       ├── LinkHeaderParser.cs
│       │       ├── WireEnvelope.cs
│       │       └── EndpointUriBuilder.cs
│       └── Stateful/
│           ├── ServiceNowClient.cs
│           └── ServiceNowClientFactory.cs
└── tests/
    └── ServiceNowTableAPIWrapper.Tests/
        └── ...
```

---

## 4. Core Domain Models & Non-Throwing Envelopes

### 4.1 Base Record Interface
```csharp
public interface IServiceNowRecord
{
    [JsonPropertyName("sys_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? SysId { get; set; }
}
```

### 4.2 Result & Error Envelopes
`ServiceNowResult<T>` and `ServiceNowPageResult<T>` omit generic constraints so `T = dynamic` compiles cleanly, and expose standard `IReadOnlyList<T>` collections so callers can apply post-retrieval LINQ-to-Objects immediately after inspecting `IsSuccess`.

```csharp
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

public sealed record PaginationLinks(
    Uri? First = null,
    Uri? Previous = null,
    Uri? Next = null,
    Uri? Last = null)
{
    public static PaginationLinks Empty { get; } = new();
}

public sealed record ServiceNowPageResult<T>(
    bool IsSuccess,
    HttpStatusCode? StatusCode,
    IReadOnlyList<T> Items,
    int? TotalCount,
    PaginationLinks Links,
    ServiceNowError? Error = null,
    string? RawJson = null)
{
    public bool HasNextPage => IsSuccess && Links.Next is not null;
}
```

### 4.3 `TableRequestOptions` & Annotations
```csharp
public enum DisplayValueMode
{
    False,
    True,
    All
}

public sealed record TableRequestOptions
{
    public string? TableName { get; init; }
    public string? Query { get; init; }                      // sysparm_query
    public IReadOnlyList<string>? Fields { get; init; }      // sysparm_fields
    public bool? DisableAutoFields { get; init; }            // Suppresses auto-projected fields on <T>
    public DisplayValueMode? DisplayValue { get; init; }     // sysparm_display_value (false | true | all)
    public bool? ExcludeReferenceLink { get; init; }         // sysparm_exclude_reference_link
    public string? View { get; init; }                       // sysparm_view
    public int? Limit { get; init; }                         // sysparm_limit (default 1000 on <T> list queries)
    public int? Offset { get; init; }                        // sysparm_offset
    public bool? NoCount { get; init; }                      // sysparm_no_count
    public string? QueryCategory { get; init; }              // sysparm_query_category
    public bool? QueryNoDomain { get; init; }                // sysparm_query_no_domain
    public bool? SuppressPaginationHeader { get; init; }     // sysparm_suppress_pagination_header
    public int? MaxEagerReferenceDepth { get; init; }        // Overrides transport default depth (0 disables eager load)

    public TableRequestOptions MergeWith(TableRequestOptions? overrides);
}
```

| Attribute | Target | Maps To `TableRequestOptions` / Serialization Behavior |
| :--- | :--- | :--- |
| `[ServiceNowTable(string tableName)]` | Class | `TableName` |
| `[SysparmQuery(string encodedQuery)]` | Class | `Query` (`sysparm_query`) |
| `[SysparmFields(params string[] fields)]` | Class | `Fields` (`sysparm_fields`) |
| `[SysparmDisableAutoFields]` | Class | `DisableAutoFields = true` (omits auto-collected `[JsonPropertyName]` list) |
| `[SysparmDisplayValue(DisplayValueMode mode)]` | Class | `DisplayValue` (`sysparm_display_value`) |
| `[SysparmExcludeReferenceLink(bool exclude)]` | Class | `ExcludeReferenceLink` (`sysparm_exclude_reference_link`) |
| `[SysparmView(string view)]` | Class | `View` (`sysparm_view`) |
| `[SysparmLimit(int limit)]` | Class | `Limit` (`sysparm_limit`) |
| `[SysparmOffset(int offset)]` | Class | `Offset` (`sysparm_offset`) |
| `[SysparmNoCount(bool noCount)]` | Class | `NoCount` (`sysparm_no_count`) |
| `[SysparmQueryCategory(string category)]` | Class | `QueryCategory` (`sysparm_query_category`) |
| `[SysparmQueryNoDomain(bool noDomain)]` | Class | `QueryNoDomain` (`sysparm_query_no_domain`) |
| `[SysparmSuppressPaginationHeader(bool suppress)]` | Class | `SuppressPaginationHeader` (`sysparm_suppress_pagination_header`) |
| `[ServiceNowReadOnly]` | Property | Included in `GET` `sysparm_fields`, omitted during `POST`/`PUT`/`PATCH` write serialization |
| `[ServiceNowEagerLoad(bool enabled = true)]` | Property | Batch-loads `ReferenceLink<T>.Record` upon parent retrieval when `enabled` is `true` |

---

## 5. Optional Server-Side LINQ Query Builder & Post-Retrieval Filtering (`Linq/`)

### 5.1 Clear Separation of Server-Side vs. Client-Side LINQ
1. **Server-Side Query Generation (`client.Query<T>()` / `client.Query(...)`):**
   - Optional alternative to manually writing raw ServiceNow `sysparm_query` strings in `TableRequestOptions`.
   - Compiles C# expression trees into `TableRequestOptions` (`sysparm_query`, `sysparm_fields`, `sysparm_limit`, `sysparm_offset`) and delegates execution to `ListPageAsync` / `ListAllAsync` / `Stream*`.
   - `IServiceNowQueryable` itself never performs hidden post-retrieval filtering to compensate for untranslatable server expressions.
2. **Post-Retrieval Client-Side Filtering (Caller-Controlled LINQ-to-Objects):**
   - Once any query or list method produces results (`pageResult.Items`, `listResult.Value`, or `StreamRecordsAsync`), consumers have standard CLR collections (`IReadOnlyList<T>`, `IReadOnlyList<dynamic>`, `IAsyncEnumerable<ServiceNowResult<T>>`) and can freely chain standard `System.Linq` (`Where`, `Select`, `OrderBy`, `GroupBy`, `ToDictionary`, etc.) in memory for complex C# transformations, regex matching, or business logic that ServiceNow's encoded query grammar does not support.

### 5.2 Typed `IServiceNowQueryable<T>` & Non-Generic `IServiceNowDynamicQueryable`
```csharp
public interface IServiceNowQueryable<T> where T : class, IServiceNowRecord
{
    IServiceNowQueryable<T> Where(Expression<Func<T, bool>> predicate);
    IServiceNowQueryable<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector);
    IServiceNowQueryable<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector);
    IServiceNowQueryable<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector);
    IServiceNowQueryable<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector);
    IServiceNowQueryable<T> SelectFields<TProjection>(Expression<Func<T, TProjection>> fieldSelector);
    IServiceNowQueryable<T> Take(int count);
    IServiceNowQueryable<T> Skip(int count);
    IServiceNowQueryable<T> WithOptions(TableRequestOptions options);

    ServiceNowResult<TableRequestOptions> ToRequestOptions();

    Task<ServiceNowPageResult<T>> ToPageAsync(CancellationToken cancellationToken = default);
    Task<ServiceNowResult<IReadOnlyList<T>>> ToListAsync(CancellationToken cancellationToken = default);
    Task<ServiceNowResult<T?>> FirstOrDefaultAsync(CancellationToken cancellationToken = default);
    IAsyncEnumerable<ServiceNowPageResult<T>> StreamPagesAsync(CancellationToken cancellationToken = default);
    IAsyncEnumerable<ServiceNowResult<T>> StreamRecordsAsync(CancellationToken cancellationToken = default);
}

public interface IServiceNowDynamicQueryable
{
    IServiceNowDynamicQueryable Where(Expression<Func<ServiceNowRow, bool>> predicate);
    IServiceNowDynamicQueryable OrderBy(string fieldName);
    IServiceNowDynamicQueryable OrderByDescending(string fieldName);
    IServiceNowDynamicQueryable ThenBy(string fieldName);
    IServiceNowDynamicQueryable ThenByDescending(string fieldName);
    IServiceNowDynamicQueryable SelectFields(params string[] fields);
    IServiceNowDynamicQueryable Take(int count);
    IServiceNowDynamicQueryable Skip(int count);
    IServiceNowDynamicQueryable WithOptions(TableRequestOptions options);

    ServiceNowResult<TableRequestOptions> ToRequestOptions();

    Task<ServiceNowPageResult<dynamic>> ToPageAsync(CancellationToken cancellationToken = default);
    Task<ServiceNowResult<IReadOnlyList<dynamic>>> ToListAsync(CancellationToken cancellationToken = default);
    Task<ServiceNowResult<dynamic?>> FirstOrDefaultAsync(CancellationToken cancellationToken = default);
    IAsyncEnumerable<ServiceNowPageResult<dynamic>> StreamPagesAsync(CancellationToken cancellationToken = default);
    IAsyncEnumerable<ServiceNowResult<dynamic>> StreamRecordsAsync(CancellationToken cancellationToken = default);
}
```

### 5.3 Supported C# Expression -> ServiceNow `sysparm_query` Mapping

`PartialEvaluator` evaluates local closures, variables, and constant expressions first. `ServiceNowExpressionTranslator` maps properties via `[JsonPropertyName]` (or `ServiceNowRow.Field<T>(name)`) and translates operators as follows:

| C# LINQ Expression | ServiceNow Encoded Query (`sysparm_query`) |
| :--- | :--- |
| `x.ShortDescription == "Outage"` | `short_description=Outage` |
| `x.ShortDescription != "Outage"` | `short_description!=Outage` |
| `x.ShortDescription == null` / `string.IsNullOrEmpty(x.ShortDescription)` | `short_descriptionISEMPTY` |
| `x.ShortDescription != null` / `!string.IsNullOrEmpty(x.ShortDescription)` | `short_descriptionISNOTEMPTY` |
| `x.Active` / `x.Active == true` | `active=true` |
| `!x.Active` / `x.Active == false` | `active=false` |
| `x.Priority < 3`, `<= 3`, `> 1`, `>= 1` | `priority<3`, `priority<=3`, `priority>1`, `priority>=1` |
| `x.ShortDescription.Contains("VPN")` | `short_descriptionLIKEVPN` |
| `!x.ShortDescription.Contains("VPN")` | `short_descriptionNOT LIKEVPN` |
| `x.Number.StartsWith("INC")` | `numberSTARTSWITHINC` |
| `x.Number.EndsWith("99")` | `numberENDSWITH99` |
| `allowedIds.Contains(x.SysId)` (`IEnumerable<T>` / array) | `sys_idINid1,id2,id3` |
| `!allowedIds.Contains(x.SysId)` | `sys_idNOT INid1,id2,id3` |
| `x.Caller!.Value == userId` / `x.Caller == userId` | `caller_id={userId}` |
| `x.Caller!.Record!.Email == "a@b.com"` | `caller_id.email=a@b.com` *(ReferenceLink dot-walking)* |
| `a && b` (or chained `.Where(a).Where(b)`) | `a^b` |
| `a \|\| b` (atomic conditions) | `a^ORb` |
| `(a && b) \|\| (c && d)` (compound OR branches) | Rewritten via `DisjunctiveNormalFormRewriter` into `a^b^NQc^d` (`^NQ` New Query operator) |
| `.OrderBy(x => x.CreatedOn)` | Appends `^ORDERBYsys_created_on` |
| `.OrderByDescending(x => x.Priority)` | Appends `^ORDERBYDESCpriority` |
| `.Take(n)` / `.FirstOrDefaultAsync()` | Sets `Limit = n` (`Limit = 1` for `FirstOrDefaultAsync`) |
| `.Skip(n)` | Sets `Offset = n` |

- **Escaping Rule:** Caret literals (`^`) inside constant string values MUST be escaped as `^^` to prevent encoded query injection.
- **Annotation Merge Rule:** If model `T` declares `[SysparmQuery("...")]`, translated LINQ filter clauses are combined with the model's base query via `^` (unless explicitly replaced via `.WithOptions(...)`).

---

## 6. `ReferenceLink`, `ReferenceLink<T>`, & Converters

- **`ReferenceLink` & `ReferenceLink<T>`:**
  - `ReferenceLink` exposes `Value` (`string?`, falls back to trailing segment of `Link`), `DisplayValue` (`string?`), `Link` (`Uri?`), internal `[JsonIgnore] ClientContext`, `LoadAsync<T>(TableRequestOptions?, CancellationToken)`, and `LoadAsync(TableRequestOptions?, CancellationToken)` (`dynamic`).
  - `ReferenceLink<T> : ReferenceLink` (`where T : class, IServiceNowRecord`) adds pure `[JsonIgnore] public T? Record { get; set; }`, unconditional network reload `LoadAsync(TableRequestOptions?, CancellationToken)` (updates `Record` on success), and implicit conversions `T?` <-> `ReferenceLink<T>`.
  - Outbound JSON (`POST`/`PUT`/`PATCH`) writes ONLY the `sys_id` string (`Value ?? Record?.SysId`) or `null`.
  - `[ServiceNowEagerLoad]` properties are batch-hydrated (`sys_idIN...`) inside `ReferenceLinkHydrator` upon parent retrieval up to `MaxEagerReferenceDepth` (default `2`).
- **JSON Converters (`ServiceNowJsonOptions`):**
  - `ServiceNowBoolConverter`, `ServiceNowInt64Converter`, `ServiceNowDateConverters` (`DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly` + `""` -> `null`), `ReferenceLinkConverterFactory`, `DynamicRecordConverter` (`ExpandoObject`), and `ReadOnlyPropertyModifier` (`[ServiceNowReadOnly]`).

---

## 7. Client Interfaces (`IServiceNowStatelessClient` & `IServiceNowClient`)

### 7.1 Tier 1: `IServiceNowStatelessClient`
Requires `ServiceNowConnection` on every call; zero mutable state; 1:1 `<T>` (`where T : class, IServiceNowRecord`) and non-generic `dynamic` pairs:

- **Optional LINQ Query Entry Points:**
  - `IServiceNowQueryable<T> Query<T>(ServiceNowConnection connection, TableRequestOptions? options = null) where T : class, IServiceNowRecord`
  - `IServiceNowDynamicQueryable Query(ServiceNowConnection connection, TableRequestOptions options)`
- **Read:**
  - `GetByIdAsync<T>(ServiceNowConnection, string sysId, TableRequestOptions? options = null, CancellationToken ct = default)`
  - `GetByIdAsync(ServiceNowConnection, string sysId, TableRequestOptions? options = null, CancellationToken ct = default)`
  - `GetByLinkAsync<T>(ServiceNowConnection, ReferenceLink<T> link, TableRequestOptions? options = null, CancellationToken ct = default)`
  - `GetByLinkAsync(ServiceNowConnection, ReferenceLink link, TableRequestOptions? options = null, CancellationToken ct = default)`
  - `GetByUriAsync<T>(ServiceNowConnection, Uri recordUri, TableRequestOptions? options = null, CancellationToken ct = default)`
  - `GetByUriAsync(ServiceNowConnection, Uri recordUri, TableRequestOptions? options = null, CancellationToken ct = default)`
- **List & Stream:**
  - `ListPageAsync<T>(ServiceNowConnection, TableRequestOptions? options = null, CancellationToken ct = default)`
  - `ListPageAsync(ServiceNowConnection, TableRequestOptions? options = null, CancellationToken ct = default)`
  - `ListPageByUriAsync<T>(ServiceNowConnection, Uri pageUri, TableRequestOptions? options = null, CancellationToken ct = default)`
  - `ListPageByUriAsync(ServiceNowConnection, Uri pageUri, TableRequestOptions? options = null, CancellationToken ct = default)`
  - `ListAllAsync<T>(ServiceNowConnection, TableRequestOptions? options = null, CancellationToken ct = default)`
  - `ListAllAsync(ServiceNowConnection, TableRequestOptions? options = null, CancellationToken ct = default)`
  - `StreamPagesAsync<T>(ServiceNowConnection, TableRequestOptions? options = null, CancellationToken ct = default)`
  - `StreamPagesAsync(ServiceNowConnection, TableRequestOptions? options = null, CancellationToken ct = default)`
  - `StreamRecordsAsync<T>(ServiceNowConnection, TableRequestOptions? options = null, CancellationToken ct = default)`
  - `StreamRecordsAsync(ServiceNowConnection, TableRequestOptions? options = null, CancellationToken ct = default)`
- **Create, Patch, Replace, Upsert, Delete:**
  - `CreateAsync<T>(ServiceNowConnection, T record, TableRequestOptions? options = null, CancellationToken ct = default)`
  - `CreateAsync(ServiceNowConnection, dynamic record, TableRequestOptions? options = null, CancellationToken ct = default)`
  - `PatchAsync<T>(ServiceNowConnection, string sysId, object partialPayload, TableRequestOptions? options = null, CancellationToken ct = default)`
  - `PatchAsync(ServiceNowConnection, string sysId, object partialPayload, TableRequestOptions? options = null, CancellationToken ct = default)`
  - `PatchAsync<T>(ServiceNowConnection, T record, TableRequestOptions? options = null, CancellationToken ct = default)`
  - `PatchAsync(ServiceNowConnection, dynamic record, TableRequestOptions? options = null, CancellationToken ct = default)`
  - `ReplaceAsync<T>(ServiceNowConnection, T record, TableRequestOptions? options = null, CancellationToken ct = default)`
  - `ReplaceAsync(ServiceNowConnection, dynamic record, TableRequestOptions? options = null, CancellationToken ct = default)`
  - `UpsertAsync<T>(ServiceNowConnection, T record, TableRequestOptions? options = null, CancellationToken ct = default)`
  - `UpsertAsync(ServiceNowConnection, dynamic record, TableRequestOptions? options = null, CancellationToken ct = default)`
  - `DeleteAsync<T>(ServiceNowConnection, string sysId, TableRequestOptions? options = null, CancellationToken ct = default)`
  - `DeleteAsync(ServiceNowConnection, string sysId, TableRequestOptions? options = null, CancellationToken ct = default)`

### 7.2 Tier 2: `IServiceNowClient`
Exposes `ServiceNowConnection Connection { get; }`, `IServiceNowStatelessClient Stateless { get; }`, and the exact same 26 methods above without the leading `ServiceNowConnection` parameter, delegating 100% of execution to `Stateless`.

---

## 8. Authentication & Dual-Mode Lifecycle

- **`IServiceNowCredentialProvider`:** `ValueTask<AuthenticationHeaderValue?> GetAuthorizationHeaderAsync(CancellationToken ct = default)`
  - Implementations: `BasicCredentialProvider`, `BearerTokenCredentialProvider`, `DynamicBearerCredentialProvider`.
- **Dependency Injection (`ServiceNowServiceCollectionExtensions`):**
  - `AddServiceNowStatelessClient(Action<ServiceNowTransportOptions>? configure = null)` configures `IHttpClientFactory` with `SocketsHttpHandler { UseCookies = false }`, optional transient resilience handler (`429`/`503` backoff; never throws out of client), and registers `IServiceNowClientFactory`.
  - `AddServiceNowClient(ServiceNowConnection connection, Action<ServiceNowTransportOptions>? configure = null)` registers the stateless client plus a default `IServiceNowClient`.
- **Standalone (`StandaloneServiceNowFactory`):**
  - `CreateStateless(ServiceNowTransportOptions? options = null)` returns `IStandaloneServiceNowStatelessClient` (`: IServiceNowStatelessClient, IDisposable`).
  - `CreateClient(ServiceNowConnection connection, ServiceNowTransportOptions? options = null)` returns `IStandaloneServiceNowClient` (`: IServiceNowClient, IDisposable`).
  - Uses `SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(2), UseCookies = false }`.