using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Web;

namespace Blacus.ServiceNowTableAPIWrapper
{
    /// <summary>
    /// Represents query parameter operations for ServiceNow API endpoints.
    /// </summary>
    public interface IQueryParameters
    {
        /// <summary>
        /// Converts the query parameter properties into a <see cref="NameValueCollection"/> query string representation.
        /// </summary>
        /// <param name="queryParameters">The base query parameters.</param>
        /// <param name="overrideQueryParameters">Optional query parameter overrides.</param>
        /// <returns>A populated <see cref="NameValueCollection"/>.</returns>
        public static NameValueCollection GetQueryString(IQueryParameters queryParameters, IQueryParameters? overrideQueryParameters = null)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            var properties = queryParameters.GetType().GetProperties();

            foreach (var property in properties.Where(p => p.Name != "Item" && p.Name != "TableName"))
            {
                string propertyName = property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name;
                object? propertyValue = property.GetValue(queryParameters);

                if (propertyValue != null)
                {
                    query[propertyName] = propertyValue.ToString();
                }
            }

            if (overrideQueryParameters != null)
            {
                var overrideQueryParametersProperties = overrideQueryParameters.GetType().GetProperties().Where(p => p.Name != "Item");

                foreach (var property in overrideQueryParametersProperties)
                {
                    object? value = property.GetValue(overrideQueryParameters);
                    string propertyName = property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name;

                    if (value != null)
                    {
                        query[propertyName] = value.ToString();
                    }
                }
            }

            return query;
        }
    }

    /// <summary>
    /// Represents HTTP query parameters passed to ServiceNow Table API requests.
    /// </summary>
    public class QueryParameters : IQueryParameters
    {
        /// <summary>
        /// Creates a copy of the specified <see cref="QueryParameters"/> object.
        /// </summary>
        /// <param name="queryParameters">The source query parameters.</param>
        /// <returns>A new <see cref="QueryParameters"/> clone.</returns>
        public static QueryParameters Clone(QueryParameters queryParameters)
        {
            QueryParameters toReturn = new();

            foreach (var prop in queryParameters.GetType().GetProperties().Where(p => p.Name != "Item"))
            {
                toReturn[prop.Name] = queryParameters[prop.Name];
            }

            return toReturn;
        }

        /// <summary>
        /// Creates a clone of this <see cref="QueryParameters"/> instance.
        /// </summary>
        /// <returns>A cloned instance.</returns>
        public QueryParameters Clone() => Clone(this);

        /// <summary>
        /// Generates a <see cref="NameValueCollection"/> of query string key/value pairs.
        /// </summary>
        /// <param name="overrideQueryParameters">Optional parameter overrides.</param>
        /// <returns>A <see cref="NameValueCollection"/> of query keys and values.</returns>
        public NameValueCollection GetQueryString(IQueryParameters? overrideQueryParameters = null) =>
            IQueryParameters.GetQueryString(this, overrideQueryParameters);

        /// <summary>
        /// Gets or sets the target table name.
        /// </summary>
        public string? TableName { get; set; }

        /// <summary>
        /// Gets or sets the ServiceNow view parameter (<c>sysparm_view</c>).
        /// </summary>
        [JsonPropertyName("sysparm_view")]
        public string? View { get; set; }

        /// <summary>
        /// Gets or sets the maximum record count (<c>sysparm_limit</c>).
        /// </summary>
        [JsonPropertyName("sysparm_limit")]
        public int Limit { get; set; } = 1000;

        /// <summary>
        /// Gets or sets the ServiceNow encoded query string (<c>sysparm_query</c>).
        /// </summary>
        [JsonPropertyName("sysparm_query")]
        public string? Query { get; set; }

        /// <summary>
        /// Gets or sets the record offset for pagination (<c>sysparm_offset</c>).
        /// </summary>
        [JsonPropertyName("sysparm_offset")]
        public string? Offset { get; set; }

        /// <summary>
        /// Gets or sets the list of fields to request (<c>sysparm_fields</c>).
        /// </summary>
        [JsonPropertyName("sysparm_fields")]
        public string? Fields { get; set; }

        /// <summary>
        /// Gets or sets the total count suppression option (<c>sysparm_no_count</c>).
        /// </summary>
        [JsonPropertyName("sysparm_no_count")]
        public string? NoCount { get; set; }

        /// <summary>
        /// Gets or sets the query category (<c>sysparm_query_category</c>).
        /// </summary>
        [JsonPropertyName("sysparm_query_category")]
        public string? QueryCategory { get; set; }

        /// <summary>
        /// Gets or sets the domain separation query option (<c>sysparm_query_no_domain</c>).
        /// </summary>
        [JsonPropertyName("sysparm_query_no_domain")]
        public string? QueryNoDomain { get; set; }

        /// <summary>
        /// Gets or sets the display value option (<c>sysparm_display_value</c>).
        /// </summary>
        [JsonPropertyName("sysparm_display_value")]
        public string? DisplayValue { get; set; }

        /// <summary>
        /// Gets or sets the reference link exclusion option (<c>sysparm_exclude_reference_link</c>).
        /// </summary>
        [JsonPropertyName("sysparm_exclude_reference_link")]
        public string? ExcludeReferenceLink { get; set; }

        /// <summary>
        /// Gets or sets the pagination header suppression option (<c>sysparm_suppress_pagination_header</c>).
        /// </summary>
        [JsonPropertyName("sysparm_suppress_pagination_header")]
        public string? SuppressPaginationHeader { get; set; }

        /// <summary>
        /// Gets or sets property values by string property name.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <returns>The property value or null.</returns>
        public object? this[string name]
        {
            get => this.GetType().GetProperty(name)?.GetValue(this);
            set => this.GetType().GetProperty(name)?.SetValue(this, value);
        }
    }

    /// <summary>
    /// Generic query parameters class populated automatically from model class attributes for type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The ServiceNow table model type.</typeparam>
    public class QueryParameters<T> : QueryParameters
    {
        /// <summary>
        /// Indexer accessing property values by property name.
        /// </summary>
        /// <param name="name">Property name.</param>
        public new object? this[string name]
        {
            get => this.GetType().GetProperty(name)?.GetValue(this);
            set => this.GetType().GetProperty(name)?.SetValue(this, value);
        }

        private static ConcurrentDictionary<string, QueryParameters> _parameterCache = new();
        private static ConcurrentDictionary<string, string> _fieldsCache = new();

        /// <summary>
        /// Constructs a merged <see cref="QueryParameters"/> object populated from attributes on model type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="queryParameterOverride">Optional overrides provided at runtime.</param>
        /// <returns>A populated <see cref="QueryParameters"/> object.</returns>
        public QueryParameters GetQueryParameters(QueryParameters? queryParameterOverride = null)
        {
            QueryParameters? toReturn = null;
            Type typeSource = this.GetType();
            string cacheKey = typeSource.FullName ?? typeSource.Name;

            if (!_parameterCache.TryGetValue(cacheKey, out toReturn))
            {
                toReturn = new QueryParameters();
                Type typeDest = toReturn.GetType();
                var props = typeDest.GetProperties();

                foreach (var prop in props.Where(p => p.Name != "Item"))
                {
                    object? sourceValue = typeSource.GetProperty(prop.Name)?.GetValue(this, null);
                    prop.SetValue(toReturn, sourceValue);
                }

                _parameterCache[cacheKey] = toReturn.Clone();
            }

            toReturn = toReturn.Clone();

            if (queryParameterOverride != null)
            {
                foreach (var property in queryParameterOverride.GetType().GetProperties().Where(p => p.Name != "Item"))
                {
                    object? value = property.GetValue(queryParameterOverride);

                    if (value != null)
                    {
                        toReturn[property.Name] = value;
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Generates a <see cref="NameValueCollection"/> query string representation.
        /// </summary>
        /// <param name="overrideQueryParameters">Optional parameter overrides.</param>
        /// <returns>A populated <see cref="NameValueCollection"/>.</returns>
        public new NameValueCollection GetQueryString(IQueryParameters? overrideQueryParameters = null) =>
            IQueryParameters.GetQueryString(this, overrideQueryParameters);

        /// <summary>
        /// Gets the ServiceNow table name extracted from the <see cref="ServiceNowTableName"/> attribute on model <typeparamref name="T"/>.
        /// </summary>
        public new string TableName
        {
            get
            {
                var type = typeof(T);
                var tableNameAttribute = type.GetCustomAttribute<ServiceNowTableName>();
                return tableNameAttribute?.Value ?? string.Empty;
            }
        }

        /// <summary>
        /// Gets the record limit for model queries.
        /// </summary>
        [JsonPropertyName("sysparm_limit")]
        public new int Limit { get => getAttributeValueInt("SysparmLimit") ?? 1000; }

        /// <summary>
        /// Gets the form/list view option for model queries.
        /// </summary>
        [JsonPropertyName("sysparm_view")]
        public new string? View { get => getAttributeValueString("SysparmView"); }

        /// <summary>
        /// Gets the default encoded query string for model queries.
        /// </summary>
        [JsonPropertyName("sysparm_query")]
        public new string? Query { get => getAttributeValueString("SysparmQuery"); }

        /// <summary>
        /// Gets the record offset index for model queries.
        /// </summary>
        [JsonPropertyName("sysparm_offset")]
        public new string? Offset { get => getAttributeValueString("SysparmOffset"); }

        /// <summary>
        /// Gets the total count suppression option for model queries.
        /// </summary>
        [JsonPropertyName("sysparm_no_count")]
        public new string? NoCount { get => getAttributeValueString("SysparmNoCount"); }

        /// <summary>
        /// Gets the query category setting for model queries.
        /// </summary>
        [JsonPropertyName("sysparm_query_category")]
        public new string? QueryCategory { get => getAttributeValueString("sysparm_query_category"); }

        /// <summary>
        /// Gets the domain separation query setting for model queries.
        /// </summary>
        [JsonPropertyName("sysparm_query_no_domain")]
        public new string? QueryNoDomain { get => getAttributeValueString("SysparmQueryNoDomain"); }

        /// <summary>
        /// Gets the display value option for model queries.
        /// </summary>
        [JsonPropertyName("sysparm_display_value")]
        public new string? DisplayValue { get => getAttributeValueString("SysParmDisplayValue"); }

        /// <summary>
        /// Gets the reference link exclusion setting for model queries.
        /// </summary>
        [JsonPropertyName("sysparm_exclude_reference_link")]
        public new string? ExcludeReferenceLink { get => getAttributeValueString("SysParmExcludeReferenceLink"); }

        /// <summary>
        /// Gets the pagination header suppression setting for model queries.
        /// </summary>
        [JsonPropertyName("sysparm_suppress_pagination_header")]
        public new string? SuppressPaginationHeader { get => getAttributeValueString("sysparm_suppress_pagination_header"); }

        /// <summary>
        /// Gets the list of fields auto-generated from model property attributes.
        /// </summary>
        [JsonPropertyName("sysparm_fields")]
        public new string? Fields
        {
            get
            {
                string? toReturn = string.Empty;

                var sysParmFields = getAttributeValueString("SysParmFields");
                if (sysParmFields != null)
                    return sysParmFields;

                var sysParmFieldsDisableAuto = path.OfType<SysParmFieldsDisableAuto>().FirstOrDefault();

                if (sysParmFieldsDisableAuto == null)
                {
                    Type t = typeof(T);
                    string fieldCacheKey = t.FullName ?? t.Name;

                    if (!_fieldsCache.TryGetValue(fieldCacheKey, out toReturn))
                    {
                        List<string> fields = new();

                        foreach (var prop in t.GetProperties())
                        {
                            var jsonPropertyNameAttribute = prop.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name;

                            if (jsonPropertyNameAttribute != null)
                                fields.Add(jsonPropertyNameAttribute);
                        }

                        toReturn = string.Join(",", fields);
                        _fieldsCache[fieldCacheKey] = toReturn;
                    }
                }

                return toReturn;
            }
        }

        private int? getAttributeValueInt(string name)
        {
            try
            {
                var attrib = path.FirstOrDefault(attrib => attrib.GetType().Name == name);
                if (attrib == null)
                    return null;

                object? value = attrib.GetType().GetProperty("Value")?.GetValue(attrib);

                if (null != value)
                {
                    int toReturn = (int)value;
                    return toReturn;
                }
            }
            catch (Exception) { }

            return null;
        }

        private string? getAttributeValueString(string name)
        {
            try
            {
                var attrib = path.FirstOrDefault(attrib => attrib.GetType().Name == name);
                return attrib?.GetType().GetProperty("Value")?.GetValue(attrib)?.ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private Attribute[]? _path;
        private Attribute[] path
        {
            get
            {
                if (_path == null)
                {
                    Type t = typeof(T);
                    TypeInfo info = t.GetTypeInfo();
                    _path = (Attribute[])info.GetCustomAttributes(typeof(Attribute), true);
                }

                return _path;
            }
        }
    }
}
