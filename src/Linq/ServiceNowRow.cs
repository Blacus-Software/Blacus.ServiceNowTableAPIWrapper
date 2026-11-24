namespace Blacus.ServiceNowTableAPIWrapper.Linq;

/// <summary>
/// Represents a row in a dynamic ServiceNow table query for expression translation.
/// </summary>
public sealed class ServiceNowRow
{
    /// <summary>
    /// References a typed field by its ServiceNow column name within a query expression predicate.
    /// </summary>
    /// <typeparam name="T">The expected CLR type of the field.</typeparam>
    /// <param name="fieldName">The ServiceNow column name.</param>
    /// <returns>A default value of <typeparamref name="T"/> used for expression tree construction.</returns>
    public T Field<T>(string fieldName) => default!;
}
