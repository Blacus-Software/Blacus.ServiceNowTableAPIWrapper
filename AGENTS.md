# AI Coding Guardrails (`AGENTS.md`)

**Project:** Blacus ServiceNow TableAPI Wrapper (`Blacus.ServiceNowTableAPIWrapper`)

Read `SPEC.md` completely before generating, refactoring, or reviewing any file in this repository. Every contribution MUST obey all architectural rules, coding standards, and verification gates below without exception.

---

## 1. Core Architectural Boundaries

1. **Strict Two-Tier Composition (`Stateless` -> `Stateful`):**
   - **Tier 1 (`IServiceNowStatelessClient` in `Stateless/`):** Owns all HTTP transport, URI construction, header parsing (`Link` and `X-Total-Count`), JSON serialization/deserialization, and `ReferenceLink` hydration. It holds zero instance or credential state; every method accepts `ServiceNowConnection` as its first parameter.
   - **Tier 2 (`IServiceNowClient` in `Stateful/`):** Holds a bound `ServiceNowConnection` and composes `IServiceNowStatelessClient` via constructor injection.
   - **Hard Isolation:** Files under `Stateful/` MUST NOT contain `using System.Net.Http;`, `using System.Text.Json;`, `HttpClient`, `HttpRequestMessage`, `HttpResponseMessage`, or URI-building logic. Every method in `ServiceNowClient` MUST be a 1-line pass-through delegating to `IServiceNowStatelessClient`.
2. **Dual-Mode Instantiation Parity (`IHttpClientFactory` & Standalone):**
   - **Injectable Mode:** Registered via `IServiceCollection.AddServiceNowStatelessClient(...)` and `AddServiceNowClient(...)` backed by `IHttpClientFactory`.
   - **Standalone Mode:** Created via `StandaloneServiceNowFactory` backed by `SocketsHttpHandler` configured with `PooledConnectionLifetime = TimeSpan.FromMinutes(2)` and `UseCookies = false`.
   - **Handler Invariant:** `UseCookies = false` MUST be enforced on the primary HTTP message handler in both DI and Standalone modes.
   - **Disposal Safety:** `IServiceNowStatelessClient` and `IServiceNowClient` MUST NOT inherit `IDisposable`. Only standalone wrapper interfaces (`IStandaloneServiceNowStatelessClient`, `IStandaloneServiceNowClient`) may implement `IDisposable`.

---

## 2. Hard-Locked Prohibitions (NEVER Violate)

### Rule 1: NEVER Throw Operational, HTTP, Validation, LINQ, or Serialization Exceptions
- `response.EnsureSuccessStatusCode()` is **strictly forbidden**.
- Never throw `HttpRequestException`, `TaskCanceledException` (on HTTP timeout), `JsonException`, `NotSupportedException`, `ArgumentException`, `InvalidOperationException`, or `KeyNotFoundException` from any client method, LINQ queryable method, or `ReferenceLink` method.
- **Validation Failures:** Missing `TableName`, null record payloads, unbound `ReferenceLink` instances, or missing `sys_id` values on `Patch`/`Replace`/`Delete` MUST return a failed result envelope (`IsSuccess = false`, `Error.Kind = ServiceNowErrorKind.InvalidRequest`).
- **Unsupported LINQ Expressions:** If `ServiceNowExpressionTranslator` encounters an untranslatable C# method, member, or operator inside `IServiceNowQueryable`, it MUST record an error and cause terminal execution (`ToPageAsync`, `ToListAsync`, `FirstOrDefaultAsync`, `Stream*`, `ToRequestOptions`) to return `IsSuccess = false` with `Error.Kind = ServiceNowErrorKind.UnsupportedQueryExpression`—never throw `NotSupportedException`.
- **HTTP Error Responses (`4xx` / `5xx`):** Parse ServiceNow's `{ "error": { "message": "...", "detail": "..." }, "status": "failure" }` payload into `ServiceNowError` (`ServiceNowErrorKind.ApiError`) alongside the `HttpStatusCode`. If the error body is non-JSON (e.g., HTML proxy/hibernation page), return `ServiceNowErrorKind.HttpProtocolError`.
- **Allowed Exceptions (ONLY These Two):**
  1. `ArgumentNullException` inside constructors or DI registration extension methods when required dependencies/options are `null`.
  2. `OperationCanceledException` **only** when the caller's passed `CancellationToken.IsCancellationRequested` is `true` (internal HTTP timeouts where the caller token was *not* canceled MUST be caught and mapped to `ServiceNowErrorKind.TransportError`).

### Rule 2: Keep `IServiceNowQueryable` Server-Side Only, While Keeping Returned Collections Standard for Client-Side LINQ
- `Query<T>()` and `Query(...)` are **optional** server-side query builders alongside direct `TableRequestOptions` list/read methods.
- Operators on `IServiceNowQueryable<T>` and `IServiceNowDynamicQueryable` MUST compile exclusively into server-side `TableRequestOptions` (`sysparm_query`, `sysparm_fields`, `sysparm_limit`, `sysparm_offset`). The library MUST NOT secretly pull unfiltered records into memory inside `IServiceNowQueryable` to evaluate predicates that failed server translation.
- Do NOT implement `IEnumerable<T>` directly on `IServiceNowQueryable<T>` (avoids accidental synchronous HTTP blocking before `await`).
- Once records are retrieved, result envelopes (`ServiceNowPageResult<T>.Items` as `IReadOnlyList<T>`, `ServiceNowResult<IReadOnlyList<T>>.Value`, and `IAsyncEnumerable`) MUST remain standard LINQ-compatible .NET collections so callers can freely apply post-retrieval client-side LINQ-to-Objects (`Where`, `Select`, `GroupBy`, `OrderBy`, etc.) on the produced results.
- Always escape literal `^` characters inside translated server query constants as `^^` to prevent ServiceNow encoded query injection, and rewrite compound `||` (`OR`) groups using `^NQ` (Disjunctive Normal Form) so C# operator precedence is preserved on the server.

### Rule 3: NEVER Add Record or Query Caching
- Do NOT introduce `IMemoryCache`, `ConcurrentDictionary` record stores, ETag caches, or delta-sync caches in either client tier. Every client method and every `LoadAsync` call executes live against ServiceNow.
- **Permitted Static Metadata Cache Only:** `TableMetadata<T>` MUST cache static reflection metadata (table name, annotation defaults, `[JsonPropertyName]` field lists, `[ServiceNowReadOnly]` properties, and `ReferenceLink` property accessors) once per closed generic type `T`. Never use string-indexed property reflection on request hot paths.

### Rule 4: NEVER Break 1:1 `<T>` and Non-Generic `dynamic` Symmetry
- Every `<T>` method (`where T : class, IServiceNowRecord`) on `IServiceNowStatelessClient` and `IServiceNowClient` MUST have a matching non-generic overload with the exact same method name and parameter list (minus `<T>`), returning `dynamic` (or `IServiceNowDynamicQueryable` for `Query`).
- **No `JsonElement` Leakage:** All `dynamic` deserialization MUST use `DynamicRecordConverter` to materialize JSON objects as `ExpandoObject`, JSON arrays as `List<object?>`, and JSON primitives as native CLR types (`string`, `bool`, `long`, `double`, `null`).
- **Overload Disambiguation:** Keep `where T : class, IServiceNowRecord` on generic methods so POCO/anonymous/`ExpandoObject` arguments route cleanly to the non-generic `dynamic` write overloads without C# compiler ambiguity. Do NOT add `where T : IServiceNowRecord` to `ServiceNowResult<T>` or `ServiceNowPageResult<T>`.

### Rule 5: NEVER Perform Hidden I/O on `ReferenceLink` Properties or Write Object Payloads
- `ReferenceLink<T>` inherits from `ReferenceLink` (`Value`, `DisplayValue`, `Link`) and exposes `[JsonIgnore] public T? Record { get; set; }`.
- **Pure Property Getter:** Reading or writing `ReferenceLink<T>.Record` (or invoking implicit conversions `T?` <-> `ReferenceLink<T>`) MUST NEVER trigger network I/O, `Lazy<T>`, or `.GetAwaiter().GetResult()`.
- **Eager vs. On-Demand Reload:**
  - Properties decorated with `[ServiceNowEagerLoad]` (when `Enabled == true` and within `MaxEagerReferenceDepth`) are batch-populated (`sys_idIN...`) inside `ReferenceLinkHydrator` before the parent call returns.
  - Unannotated `ReferenceLink<T>` properties leave `Record = null` on initial retrieval.
  - `await link.LoadAsync(...)` MUST ALWAYS perform a fresh HTTP fetch using the internal `[JsonIgnore] ClientContext` attached during parent deserialization—regardless of `[ServiceNowEagerLoad]` or whether `Record` is already populated—updating `Record` when `IsSuccess == true`.
- **Wire Serialization (`POST` / `PUT` / `PATCH`):** Both `ReferenceLink` and `ReferenceLink<T>` MUST serialize strictly as a JSON string containing the `sys_id` (`Value ?? Record?.SysId`), or JSON `null` if neither is present. Never serialize a `ReferenceLink` as `{ "value": ..., "display_value": ..., "link": ... }`.

### Rule 6: NEVER Omit Shape-Controlling Parameters on Write Requests
- On `CreateAsync`, `PatchAsync`, `ReplaceAsync`, and `UpsertAsync`, ALWAYS append resolved shape-controlling query parameters (`sysparm_display_value`, `sysparm_exclude_reference_link`, `sysparm_fields`, `sysparm_view`) to the request URI so ServiceNow's response body matches `T`'s expected JSON schema.
- Properties marked `[ServiceNowReadOnly]` MUST be included in auto-projected `GET` `sysparm_fields`, but MUST be omitted when serializing `POST`/`PUT`/`PATCH` request bodies via `ReadOnlyPropertyModifier`.

### Rule 7: NEVER Overwrite or Revert Intervening User Edits
- Always treat the working directory's current disk state as the source of truth immediately prior to applying changes.
- Never blindly overwrite files using stale in-memory buffers or cached reads if the file has been modified since it was last inspected.
- When modifying existing files, inspect recent changes or use targeted diff-based edits. If intervening user modifications are detected, re-read the updated file and reconcile your changes on top of the user's edits rather than replacing or clobbering them.

---

## 3. C# & Project Coding Standards

- **Namespace & Package:** Root namespace and PackageId MUST be `Blacus.ServiceNowTableAPIWrapper`.
- **Target Frameworks:** Multi-target `net8.0;net9.0;net10.0` cleanly with zero `#pragma warning disable` workarounds.
- **Compiler Strictness:**
  - `<ImplicitUsings>disable</ImplicitUsings>` — declare explicit `using` statements at the top of every file.
  - `<Nullable>enable</Nullable>` — maintain strict nullability annotations.
  - `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` and `<GenerateDocumentationFile>true</GenerateDocumentationFile>` — every `public` type and member MUST have accurate XML documentation comments.
- **Async Hygiene:**
  - Every asynchronous method name MUST end with `Async` and accept `CancellationToken cancellationToken = default` as its final parameter.
  - Every `await` and `await foreach` inside `src/ServiceNowTableAPIWrapper/` MUST use `.ConfigureAwait(false)`.
- **Static State Prohibition:**
  - Never introduce `static HttpClient` fields or mutable `static` configuration properties.

---

## 4. Pre-Completion Self-Check for AI Agents

Before finalizing any code generation or edit task, verify:

1. [ ] Did any client, LINQ translator, or `ReferenceLink` method throw an exception instead of returning a `ServiceNowResult` / `ServiceNowPageResult` / `ServiceNowDeleteResult`?
2. [ ] Does `IServiceNowQueryable` translate strictly to server-side `sysparm_*` parameters while exposing standard `IReadOnlyList<T>` / `IAsyncEnumerable` outputs for optional post-retrieval LINQ-to-Objects?
3. [ ] Does every `<T>` method on `IServiceNowStatelessClient` and `IServiceNowClient` have its exact non-generic `dynamic` twin?
4. [ ] Is `Stateful/` completely free of `System.Net.Http` and `System.Text.Json` references?
5. [ ] Do `dynamic` results materialize as `ExpandoObject` (never `JsonElement`)?
6. [ ] Does `ReferenceLink` serialize outbound as a bare `sys_id` string, keep `.Record` free of hidden I/O, and always reload over the network when `LoadAsync` is called?
7. [ ] Are `ImplicitUsings = disable`, `.ConfigureAwait(false)`, XML doc comments, and `UseCookies = false` preserved everywhere?
8. [ ] Were all file edits verified against the latest disk state to ensure intervening user changes were preserved and not clobbered?