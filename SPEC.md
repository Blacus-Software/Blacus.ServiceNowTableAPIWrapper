# Technical Specification: Blacus.ServiceNow

## 1. Architectural Invariants (Hard-Lock Rules)

Every file, interface, and implementation in this repository MUST strictly adhere to the following non-negotiable rules:

1. **Zero-Exception Transport, Validation, & Deserialization Contract:**
   - Neither Tier 1 (`IServiceNowStatelessClient`), Tier 2 (`IServiceNowClient`), nor `ReferenceLink` methods may throw exceptions for HTTP error status codes (`4xx`/`5xx`), network/timeout failures, input validation failures, dynamic binder conversions, or JSON payload mismatches.
   - `HttpResponseMessage.EnsureSuccessStatusCode()` is strictly forbidden.
   - Every operation MUST return a structured result envelope (`ServiceNowResult<T>`, `ServiceNowResult<dynamic>`, `ServiceNowPageResult<T>`, `ServiceNowPageResult<dynamic>`, or `ServiceNowDeleteResult`) exposing `IsSuccess`, `StatusCode`, `Error` (`ServiceNowError`), and the deserialized payload.
   - Missing required inputs (e.g., missing `TableName` on a non-generic call, null record payload, missing `Value`/`Link` on `ReferenceLink.LoadAsync`, or missing `sys_id` on `Patch`/`Replace`/`Delete`) MUST return `IsSuccess = false` with `ServiceNowErrorKind.InvalidRequest` rather than throwing `ArgumentException` or `InvalidOperationException`.
   - Constructor/DI guardrails (`ArgumentNullException` for null constructor dependencies) and explicit caller `CancellationToken` cancellation (`OperationCanceledException`) are the ONLY permitted exceptions.
2. **Strict Two-Tier Composition (Stateless -> Stateful):**
   - **Tier 1 (`IServiceNowStatelessClient`):** Holds an `HttpClient` and zero instance/credential state. Every method requires a `ServiceNowConnection` parameter.
   - **Tier 2 (`IServiceNowClient` — Stateful):** Holds a bound `ServiceNowConnection` and MUST compose `IServiceNowStatelessClient` via constructor injection. Tier 2 MUST NOT reference `System.Net.Http`, `HttpClient`, `HttpRequestMessage`, `HttpResponseMessage`, or `JsonSerializer`.
3. **1:1 Typed `<T>` and Non-Generic `dynamic` Parity:**
   - Every method on `IServiceNowStatelessClient` and `IServiceNowClient` that accepts `<T>` MUST have a corresponding non-generic overload with an identical parameter signature (minus `<T>`), returning `dynamic` (`ExpandoObject` at runtime) instead of `T`.
   - Non-generic `dynamic` payloads MUST deserialize JSON objects into `ExpandoObject` (`IDictionary<string, object?>`), JSON arrays into `List<object?>`, and primitives into native CLR types (`string`, `bool`, `long`, `double`, `null`). Raw `JsonElement` values MUST NOT leak into `dynamic` results.
4. **Zero Caching:**
   - Neither Tier 1 nor Tier 2 caches table queries, metadata stamps, or cross-request records. Every client call executes live against ServiceNow.
5. **Annotation-First Wire Contract with `TableRequestOptions` Overrides:**
   - For generic `<T>` calls, class annotations (`[ServiceNowTable]`, `[SysparmDisplayValue]`, `[SysparmExcludeReferenceLink]`, `[SysparmFields]`, etc.) define the baseline `TableRequestOptions` for `T` so the returned JSON structure always matches `T`'s schema. Any non-null property in the per-call `TableRequestOptions? options` parameter overrides the annotation default for that single request.
   - For non-generic (`dynamic`) calls, `options?.TableName` provides the target table name (except URI/Link-based calls where the URI already contains the path).
   - All class and property metadata (`TableMetadata<T>`) MUST be extracted once per closed generic type `T` and cached statically—never via string-based reflection on hot paths.
6. **Client-Bound `ReferenceLink` & `ReferenceLink<T>` (`[ServiceNowEagerLoad]` & `LoadAsync`):**
   - `ReferenceLink<T>` inherits from `ReferenceLink` (`Value`, `DisplayValue`, `Link`) and exposes a pure in-memory `[JsonIgnore] public T? Record { get; set; }` property with implicit conversion operators (`T?` <-> `ReferenceLink<T>`). Accessing `.Record` MUST NEVER trigger hidden synchronous network I/O.
   - Both `ReferenceLink` and `ReferenceLink<T>` hold an internal `[JsonIgnore]` client context (`IServiceNowStatelessClient` + `ServiceNowConnection`) attached automatically when the parent record is deserialized.
   - Decorating a `ReferenceLink<T>` property with `[ServiceNowEagerLoad]` causes `ServiceNowStatelessClient` to batch-populate `.Record` (`sys_idIN...`) immediately when the parent record/page is retrieved. Unannotated `ReferenceLink<T>` properties leave `.Record` as `null` upon initial parent retrieval.
   - Calling `await link.LoadAsync(...)` ALWAYS executes a fresh network fetch against ServiceNow regardless of `[ServiceNowEagerLoad]` or whether `.Record` is already populated, updates `.Record` when `IsSuccess` is `true`, and returns `ServiceNowResult<T>` (or `ServiceNowResult<dynamic>`).
   - Outbound JSON serialization (`POST`/`PUT`/`PATCH`) of both `ReferenceLink` and `ReferenceLink<T>` MUST transmit ONLY the raw `sys_id` string (`Value ?? Record?.SysId`), or `null` if empty—never a JSON object.
7. **Dual-Mode Instantiation Parity (`IHttpClientFactory` & Standalone):**
   - **Injectable Mode:** Registered via `IServiceCollection.AddServiceNowClient(...)` or `AddServiceNowStatelessClient(...)` leveraging `IHttpClientFactory`.
   - **Standalone Mode:** Instantiated via `StandaloneServiceNowFactory` backed by `SocketsHttpHandler` (`PooledConnectionLifetime = TimeSpan.FromMinutes(2)`, `UseCookies = false`).
   - `UseCookies = false` MUST always be enforced on the primary HTTP message handler in both modes.
   - `IServiceNowStatelessClient` and `IServiceNowClient` MUST NOT inherit `IDisposable` (preventing DI consumers from disposing pooled `IHttpClientFactory` handlers); standalone factory methods return dedicated `IStandaloneServiceNowStatelessClient` / `IStandaloneServiceNowClient` interfaces that extend `IDisposable`.
8. **Async & Cancellation Discipline:**
   - Every network method MUST be asynchronous, end with `Async`, use `.ConfigureAwait(false)`, and accept `CancellationToken cancellationToken = default` as its final parameter.

---

## 2. Target Frameworks & Project Metadata

- **TargetFrameworks:** `net8.0;net9.0;net10.0`
- **Language Version:** Latest (`Nullable = enable`, `ImplicitUsings = disable`, `TreatWarningsAsErrors = true`, `GenerateDocumentationFile = true`)
- **Root Namespace:** `Blacus.ServiceNow`
- **NuGet Dependencies:**
  - `Microsoft.Extensions.Http`
  - `Microsoft.Extensions.Http.Resilience`
  - `Microsoft.Extensions.Options`
  - `Microsoft.CSharp`

---

## 3. Repository & Directory Structure

```text
Blacus.ServiceNow/
├── SPEC.md
├── AGENTS.md
├── README.md
├── src/
│   └── Blacus.ServiceNow/
│       ├── Blacus.ServiceNow.csproj
│       ├── Abstractions/
│       │   ├── IServiceNowRecord.cs
│       │   ├── IServiceNowStatelessClient.cs
│       │   ├── IServiceNowClient.cs
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
    └── Blacus.ServiceNow.Tests/
        └── ...