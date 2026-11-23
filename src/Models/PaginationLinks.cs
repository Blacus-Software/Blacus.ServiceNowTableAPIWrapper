namespace Blacus.ServiceNowTableAPIWrapper.Models;

using System;

public sealed record PaginationLinks(
    Uri? First = null,
    Uri? Previous = null,
    Uri? Next = null,
    Uri? Last = null)
{
    public static PaginationLinks Empty { get; } = new();
}
