namespace Blacus.ServiceNowTableAPIWrapper.Stateless.Internal;

using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Internal wire envelope for single-record responses from the ServiceNow Table API.
/// </summary>
/// <typeparam name="T">The record type.</typeparam>
internal sealed class SingleWireEnvelope<T>
{
    /// <summary>
    /// Gets or sets the result record payload.
    /// </summary>
    [JsonPropertyName("result")]
    public T? Result { get; set; }
}

/// <summary>
/// Internal wire envelope for collection/list responses from the ServiceNow Table API.
/// </summary>
/// <typeparam name="T">The record item type.</typeparam>
internal sealed class ListWireEnvelope<T>
{
    /// <summary>
    /// Gets or sets the collection of result items.
    /// </summary>
    [JsonPropertyName("result")]
    public List<T>? Result { get; set; }
}

/// <summary>
/// Internal wire envelope for error payloads returned by the ServiceNow Table API.
/// </summary>
internal sealed class WireErrorEnvelope
{
    /// <summary>
    /// Gets or sets the structured error details.
    /// </summary>
    [JsonPropertyName("error")]
    public WireErrorDetail? Error { get; set; }

    /// <summary>
    /// Gets or sets the server status string (e.g., "failure").
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

/// <summary>
/// Represents structured error details returned inside a ServiceNow error envelope.
/// </summary>
internal sealed class WireErrorDetail
{
    /// <summary>
    /// Gets or sets the high-level error message.
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets additional error details.
    /// </summary>
    [JsonPropertyName("detail")]
    public string? Detail { get; set; }
}
