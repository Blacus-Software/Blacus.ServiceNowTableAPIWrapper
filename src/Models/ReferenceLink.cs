namespace Blacus.ServiceNowTableAPIWrapper.Models;

using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Blacus.ServiceNowTableAPIWrapper.Abstractions;
using Blacus.ServiceNowTableAPIWrapper.Authentication;

/// <summary>
/// Represents a client context holding the stateless client and connection used to load reference links.
/// </summary>
internal sealed record ServiceNowClientContext(
    IServiceNowStatelessClient StatelessClient,
    ServiceNowConnection Connection);

/// <summary>
/// Represents a ServiceNow reference field containing a sys_id value, an optional display value, and a REST URI link.
/// </summary>
public class ReferenceLink
{
    private string? _value;

    /// <summary>
    /// Gets or sets the sys_id of the referenced record.
    /// Falls back to the trailing URI path segment of <see cref="Link"/> if not explicitly set.
    /// </summary>
    [JsonPropertyName("value")]
    public string? Value
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(_value))
            {
                return _value;
            }

            if (Link is not null)
            {
                string path = Link.AbsolutePath.TrimEnd('/');
                int lastSlash = path.LastIndexOf('/');
                if (lastSlash >= 0 && lastSlash < path.Length - 1)
                {
                    return path.Substring(lastSlash + 1);
                }
            }

            return _value;
        }
        set => _value = value;
    }

    /// <summary>
    /// Gets or sets the human-readable display value of the referenced record.
    /// </summary>
    [JsonPropertyName("display_value")]
    public string? DisplayValue { get; set; }

    /// <summary>
    /// Gets or sets the direct REST API URI for the referenced record.
    /// </summary>
    [JsonPropertyName("link")]
    public Uri? Link { get; set; }

    /// <summary>
    /// Gets or sets the internal client context attached during deserialization.
    /// </summary>
    [JsonIgnore]
    internal ServiceNowClientContext? ClientContext { get; set; }

    /// <summary>
    /// Asynchronously fetches or reloads the referenced record using the specified strongly typed model.
    /// </summary>
    /// <typeparam name="TTarget">The target record model type.</typeparam>
    /// <param name="options">Optional table request options overriding defaults.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result envelope containing the retrieved record or error details.</returns>
    public async Task<ServiceNowResult<TTarget>> LoadAsync<TTarget>(
        TableRequestOptions? options = null,
        CancellationToken cancellationToken = default)
        where TTarget : class, IServiceNowRecord
    {
        if (ClientContext is null)
        {
            return ServiceNowResult<TTarget>.Failure(
                ServiceNowErrorKind.InvalidRequest,
                "ReferenceLink is not bound to an active ServiceNow client context.");
        }

        ReferenceLink<TTarget> typedLink = this as ReferenceLink<TTarget> ?? new ReferenceLink<TTarget>
        {
            Value = Value,
            DisplayValue = DisplayValue,
            Link = Link,
            ClientContext = ClientContext
        };

        return await ClientContext.StatelessClient.GetByLinkAsync<TTarget>(
            ClientContext.Connection,
            typedLink,
            options,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously fetches or reloads the referenced record as a non-generic dynamic object.
    /// </summary>
    /// <param name="options">Optional table request options overriding defaults.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result envelope containing the dynamic record or error details.</returns>
    public async Task<ServiceNowResult<dynamic>> LoadAsync(
        TableRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (ClientContext is null)
        {
            return ServiceNowResult<dynamic>.Failure(
                ServiceNowErrorKind.InvalidRequest,
                "ReferenceLink is not bound to an active ServiceNow client context.");
        }

        return await ClientContext.StatelessClient.GetByLinkAsync(
            ClientContext.Connection,
            this,
            options,
            cancellationToken).ConfigureAwait(false);
    }
}

/// <summary>
/// Strongly typed ServiceNow reference link holding the sys_id, metadata, and an in-memory cached instance of <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The referenced record type.</typeparam>
public class ReferenceLink<T> : ReferenceLink where T : class, IServiceNowRecord
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReferenceLink{T}"/> class.
    /// </summary>
    public ReferenceLink()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReferenceLink{T}"/> class with a pre-populated record.
    /// </summary>
    /// <param name="record">The record instance.</param>
    public ReferenceLink(T record)
    {
        Record = record;
        Value = record?.SysId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReferenceLink{T}"/> class with a sys_id value.
    /// </summary>
    /// <param name="sysId">The sys_id of the referenced record.</param>
    public ReferenceLink(string sysId)
    {
        Value = sysId;
    }

    /// <summary>
    /// Gets or sets the in-memory strongly typed record. Reading or setting this property never triggers network I/O.
    /// </summary>
    [JsonIgnore]
    public T? Record { get; set; }

    /// <summary>
    /// Implicitly converts a record instance into a <see cref="ReferenceLink{T}"/>.
    /// </summary>
    /// <param name="record">The record instance.</param>
    public static implicit operator ReferenceLink<T>?(T? record) =>
        record is null ? null : new ReferenceLink<T>(record);

    /// <summary>
    /// Implicitly extracts the in-memory record instance from a <see cref="ReferenceLink{T}"/>.
    /// </summary>
    /// <param name="link">The reference link.</param>
    public static implicit operator T?(ReferenceLink<T>? link) => link?.Record;

    /// <summary>
    /// Asynchronously reloads the referenced record live over the network, updating <see cref="Record"/> on success.
    /// </summary>
    /// <param name="options">Optional table request options overriding defaults.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result envelope containing the retrieved record or error details.</returns>
    public new async Task<ServiceNowResult<T>> LoadAsync(
        TableRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ServiceNowResult<T> result = await base.LoadAsync<T>(options, cancellationToken).ConfigureAwait(false);
        if (result.IsSuccess && result.Value is not null)
        {
            Record = result.Value;
        }

        return result;
    }
}
