# AI Project Instructions & Guidelines

This document records all user instructions provided during the engineering and refactoring sessions for `Blacus.ServiceNowTableAPIWrapper`.

---

## Core Operational Constraints

1. **Folder Scope Constraints**
   - **STRICT RULE:** Completely ignore the `/test` folder and all files under `/test/`.
   - Never modify, alter, format, or run build/verification commands targeting `/test` or test projects.
   - All analysis, code modifications, refactoring, and build verifications must be strictly limited to `src/` and root documentation files.

2. **Git Version Control & Commits**
   - After completing requested tasks, stage modified/new files (excluding `/test`) and create clean git commits.
   - Provide concise commit messages matching project repository standards.

---

## Project Refactoring & Engineering Directives

3. **Bug Tracking & Reporting (`BUGS.md`)**
   - When bug fixes are excluded from code implementation, document detailed reports in `BUGS.md` in the project root.
   - For excluded bug items (e.g. attribute string lookup mismatches and `TableGetResponse<T>.GetNextPageAsync()` missing client context), summarize:
     - Target file & line numbers.
     - Severity & runtime impact.
     - Description of the defect.
     - Consequences.
     - Recommended step-by-step fix.

4. **Architecture Suggestions (`SUGGESTIONS.md`)**
   - Summarize deferred architectural enhancements in `SUGGESTIONS.md` in the project root.
   - Include code examples, design rationale, and benefits for:
     - Fluent Query Parameters Builder API (`WithQuery`, `Where`, `WithLimit`, etc.).
     - Structured Exception Handling (`ServiceNowApiException` with HTTP status code and response body).
     - ASP.NET Core & Dependency Injection Registration Extensions (`IServiceCollection.AddServiceNowTableClient`).

5. **Asynchronous Streaming & Low Memory Footprint**
   - Implement `IAsyncEnumerable<T>` streaming pagination (`StreamAllRecordsAsync<T>`) for querying large ServiceNow tables without buffering all records in memory.

6. **Configurable Serialization**
   - Provide per-client instance `JsonSerializerOptions` properties and constructor overloads on `ServiceNowTableClient` while defaulting to shared singleton options.

7. **Documentation & Quality Standards**
   - Maintain XML documentation comments (`/// <summary>`, `/// <param>`, `/// <returns>`, `/// <exception>`) across all public types and members.
   - Ensure zero compiler warnings and zero errors across all supported .NET target frameworks (`net8.0`, `net9.0`, `net10.0`).
