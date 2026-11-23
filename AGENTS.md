# AI Coding Guardrails (`AGENTS.md`)

Read `SPEC.md` completely before generating, refactoring, or reviewing any file in this repository. Every contribution MUST obey all invariants, coding standards, and verification gates below without exception.

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
   - **Disposal Safety:** `IServiceNowStatelessClient` and `IServiceNowClient` MUST NOT inherit `IDisposable`. Only standalone wrapper interfaces (`IStandaloneServiceNowStatelessClient`, `IStandaloneServiceNowClient`) may implement `IDisposable` to dispose standalone handlers without risking disposal of pooled `IHttpClientFactory` handlers.

---

## 2. Hard-Locked Prohibitions (NEVER Violate)

### Rule 1: NEVER Throw Operational, HTTP, Validation, or Serialization Exceptions
- `response.EnsureSuccessStatusCode()` is **strictly forbidden**.
- Never throw `HttpRequestException`, `TaskCanceledException` (on HTTP timeout), `JsonException`, `ArgumentException`, `InvalidOperationException`, or `KeyNotFoundException` from any client method or `ReferenceLink` method.
- **Validation Failures:** Missing `TableName` (on non-generic calls or unannotated models), null record payloads, unbound `ReferenceLink` instances, or missing `sys_id` values on `Patch`/`Replace`/`Delete` MUST return a failed result envelope (`IsSuccess = false`, `Error.Kind = ServiceNowErrorKind.InvalidRequest`).
- **HTTP Error Responses (`4xx` / `5xx`):** Parse ServiceNow's `{ "error": { "message": "...", "detail": "..." }, "status": "failure" }` payload into `ServiceNowError` (`ServiceNowErrorKind.ApiError`) alongside the `HttpStatusCode`. If the error body is non-JSON (e.g., HTML proxy/hibernation page), return `ServiceNowErrorKind.HttpProtocolError`.
- **Allowed Exceptions (ONLY These Two):**
  1. `ArgumentNullException` inside constructors or DI registration extension methods when required dependencies/options are `null`.
  2. `OperationCanceledException` **only** when the caller's passed `CancellationToken.IsCancellationRequested` is `true` (internal HTTP timeouts where the caller token was *not* canceled MUST be caught and mapped to `ServiceNowErrorKind.TransportError`).

### Rule 2: NEVER Add Record or Query Caching
- Do NOT introduce `IMemoryCache`, `ConcurrentDictionary` record stores, ETag caches, or delta-sync caches in either client tier. Every client method and every `LoadAsync` call executes live against ServiceNow.
- **Permitted Static Metadata Cache Only:** `TableMetadata<T>` MUST cache static reflection metadata (table name, annotation defaults, `[JsonPropertyName]` field lists, `[ServiceNowReadOnly]` properties, and `ReferenceLink` property accessors) once per closed generic type `T`. Never use string-indexed property reflection on request hot paths.

### Rule 3: NEVER Break 1:1 `<T>` and Non-Generic `dynamic` Symmetry
- Every `<T>` method (`where T : class, IServiceNowRecord`) on `IServiceNowStatelessClient` and `IServiceNowClient` MUST have a matching non-generic overload with the exact same method name and parameter list (minus `<T>`), returning `dynamic` in place of `T`:
  - `Task<ServiceNowResult<T>>` <-> `Task<ServiceNowResult<dynamic>>`
  - `Task<ServiceNowPageResult<T>>` <-> `Task<ServiceNowPageResult<dynamic>>`
  - `Task<ServiceNowResult<IReadOnlyList<T>>>` <-> `Task<ServiceNowResult<IReadOnlyList<dynamic>>>`
  - `IAsyncEnumerable<ServiceNowPageResult<T>>` <-> `IAsyncEnumerable<ServiceNowPageResult<dynamic>>`
  - `IAsyncEnumerable<ServiceNowResult<T>>` <-> `IAsyncEnumerable<ServiceNowResult<dynamic>>`
- **No `JsonElement` Leakage:** Default `System.Text.Json` deserialization of `dynamic`/`object` produces boxed `JsonElement` structs that fail at runtime with DLR member access (`record.short_description`). All `dynamic` deserialization MUST use `DynamicRecordConverter` to materialize JSON objects as `ExpandoObject`, JSON arrays as `List<object?>`, and JSON primitives as native CLR types (`string`, `bool`, `long`, `double`, `null`).
- **Overload Disambiguation:** Keep `where T : class, IServiceNowRecord` on generic methods so POCO/anonymous/`ExpandoObject` arguments route cleanly to the non-generic `dynamic` write overloads without C# compiler ambiguity. Do NOT add `where T : IServiceNowRecord` to `ServiceNowResult<T>` or `ServiceNowPageResult<T>`.

### Rule 4: NEVER Perform Hidden I/O on `ReferenceLink` Properties or Write Object Payloads
- `ReferenceLink<T>` inherits from `ReferenceLink` (`Value`, `DisplayValue`, `Link`) and exposes `[JsonIgnore] public T? Record { get; set; }`.
- **Pure Property Getter:** Reading or writing `ReferenceLink<T>.Record` (or invoking implicit conversions `T?` <-> `ReferenceLink<T>`) MUST NEVER trigger network I/O, `Lazy<T>`, or `.GetAwaiter().GetResult()`.
- **Eager vs. On-Demand Reload:**
  - Properties decorated with `[ServiceNowEagerLoad]` (when `Enabled == true` and within `MaxEagerReferenceDepth`) are batch-populated (`sys_idIN...`) inside `ReferenceLinkHydrator` before the parent call returns.
  - Unannotated `ReferenceLink<T>` properties leave `Record = null` on initial retrieval.
  - `await link.LoadAsync(...)` MUST ALWAYS perform a fresh HTTP fetch using the internal `[JsonIgnore] ClientContext` attached during parent deserialization—regardless of `[ServiceNowEagerLoad]` or whether `Record` is already populated—updating `Record` when `IsSuccess == true`.
- **Wire Serialization (`POST` / `PUT` / `PATCH`):** Both `ReferenceLink` and `ReferenceLink<T>` MUST serialize strictly as a JSON string containing the `sys_id` (`Value ?? Record?.SysId`), or JSON `null` if neither is present. Never serialize a `ReferenceLink` as `{ "value": ..., "display_value": ..., "link": ... }`.

### Rule 5: NEVER Omit Shape-Controlling Parameters on Write Requests
- Model annotations (`[ServiceNowTable]`, `[SysparmDisplayValue]`, `[SysparmExcludeReferenceLink]`, `[SysparmFields]`, `[SysparmDisableAutoFields]`, `[SysparmView]`, etc.) establish `TableMetadata<T>.DefaultOptions`.
- On every call, resolve effective options via `DefaultOptions.MergeWith(options)` where non-null properties on `options` (`TableRequestOptions`) win.
- On `CreateAsync`, `PatchAsync`, `ReplaceAsync`, and `UpsertAsync`, ALWAYS append resolved shape-controlling query parameters (`sysparm_display_value`, `sysparm_exclude_reference_link`, `sysparm_fields`, `sysparm_view`) to the request URI so ServiceNow's response body matches `T`'s expected JSON schema.
- Properties marked `[ServiceNowReadOnly]` MUST be included in auto-projected `GET` `sysparm_fields`, but MUST be omitted when serializing `POST`/`PUT`/`PATCH` request bodies via `ReadOnlyPropertyModifier`.

---

## 3. C# & Project Coding Standards

- **Target Frameworks:** Multi-target `net8.0;net9.0;net10.0`. Code must compile cleanly across all three TFMs with zero `#pragma warning disable` hacks.
- **Compiler Strictness:**
  - `<ImplicitUsings>disable</ImplicitUsings>` — declare explicit `using` statements at the top of every file.
  - `<Nullable>enable</Nullable>` — maintain strict nullability annotations; avoid null-forgiving `!` operators unless proven non-null by immediately preceding guards.
  - `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` and `<GenerateDocumentationFile>true</GenerateDocumentationFile>` — every `public` type, property, method, enum, and parameter MUST have accurate XML documentation comments.
- **Async Hygiene:**
  - Every asynchronous method name MUST end with `Async`.
  - Every asynchronous method MUST accept `CancellationToken cancellationToken = default` as its final parameter (using `[EnumeratorCancellation]` on async iterator implementations).
  - Every `await` and `await foreach` inside `src/Blacus.ServiceNow/` MUST use `.ConfigureAwait(false)`.
- **ServiceNow JSON Quirks:**
  - Always route serialization through `ServiceNowJsonOptions` so string-encoded booleans (`"true"`/`"false"`/`""`), string-encoded 64-bit integers, ServiceNow date/time strings (including `""` -> `null`), `ReferenceLinkConverterFactory`, `DynamicRecordConverter`, and `ReadOnlyPropertyModifier` behave consistently.
- **Static State Prohibition:**
  - Never introduce `static HttpClient` fields or mutable `static` configuration properties.

---

## 4. Pre-Completion Self-Check for AI Agents

Before finalizing any code generation or edit task, verify:

1. [ ] Did any method throw an exception instead of returning `ServiceNowResult` / `ServiceNowPageResult` / `ServiceNowDeleteResult`?
2. [ ] Does every `<T>` method on `IServiceNowStatelessClient` and `IServiceNowClient` have its exact non-generic `dynamic` twin?
3. [ ] Is `Stateful/` completely free of `System.Net.Http` and `System.Text.Json` references?
4. [ ] Do `dynamic` results materialize as `ExpandoObject` (never `JsonElement`)?
5. [ ] Does `ReferenceLink` serialize outbound as a bare `sys_id` string, keep `.Record` free of hidden I/O, and always reload over the network when `LoadAsync` is called?
6. [ ] Are `ImplicitUsings = disable`, `.ConfigureAwait(false)`, XML doc comments, and `UseCookies = false` preserved everywhere?