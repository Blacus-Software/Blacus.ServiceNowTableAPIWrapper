namespace Blacus.ServiceNowTableAPIWrapper.Models;

using System.Collections.Generic;
using System.Net;

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
