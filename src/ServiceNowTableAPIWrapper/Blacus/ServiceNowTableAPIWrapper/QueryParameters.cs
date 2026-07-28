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

    public interface IQueryParameters
    {

        public static NameValueCollection GetQueryString(IQueryParameters queryParameters, IQueryParameters? overrideQueryParameters = null)
        {

            var query = HttpUtility.ParseQueryString(new UriBuilder() { Port = -1 }.Query);

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

    public class QueryParameters : IQueryParameters
    {

        public static QueryParameters Clone(QueryParameters queryParameters)
        {

            QueryParameters toReturn = new();

            foreach (var prop in queryParameters.GetType().GetProperties().Where(p => p.Name != "Item"))
            {
                toReturn[prop.Name] = queryParameters[prop.Name];
            }

            return toReturn;

        }

        public QueryParameters Clone() => Clone(this);

        public NameValueCollection GetQueryString(IQueryParameters? overrideQueryParameters = null) =>
            IQueryParameters.GetQueryString(this, overrideQueryParameters);

        public string? TableName { get; set; }

        [JsonPropertyName("sysparm_view")]
        public string? View { get; set; }

        [JsonPropertyName("sysparm_limit")]
        public int Limit { get; set; } = 1000;

        [JsonPropertyName("sysparm_query")]
        public string? Query { get; set; }

        [JsonPropertyName("sysparm_offset")]
        public string? Offset { get; set; }

        [JsonPropertyName("sysparm_fields")]
        public string? Fields { get; set; }

        [JsonPropertyName("sysparm_no_count")]
        public string? NoCount { get; set; }

        [JsonPropertyName("sysparm_query_category")]
        public string? QueryCategory { get; set; }

        [JsonPropertyName("sysparm_query_no_domain")]
        public string? QueryNoDomain { get; set; }

        [JsonPropertyName("sysparm_display_value")]
        public string? DisplayValue { get; set; }

        [JsonPropertyName("sysparm_exclude_reference_link")]
        public string? ExcludeReferenceLink { get; set; }

        [JsonPropertyName("sysparm_suppress_pagination_header")]
        public string? SuppressPaginationHeader { get; set; }

        public object? this[string name]
        {
            get => this.GetType().GetProperty(name)?.GetValue(this);
            set => this.GetType().GetProperty(name)?.SetValue(this, value);
        }

    }

    public class QueryParameters<T> : QueryParameters
    {

        public new object? this[string name]
        {
            get => this.GetType().GetProperty(name)?.GetValue(this);
            set => this.GetType().GetProperty(name)?.SetValue(this, value);
        }

        private static ConcurrentDictionary<string, QueryParameters> _parameterCache = new();
        private static ConcurrentDictionary<string, string> _fieldsCache = new();

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

        public new NameValueCollection GetQueryString(IQueryParameters? overrideQueryParameters = null) =>
            IQueryParameters.GetQueryString(this, overrideQueryParameters);

        public new string TableName
        {
            get
            {
                var type = typeof(T);
                var tableNameAttribute = type.GetCustomAttribute<ServiceNowTableName>();
                return tableNameAttribute?.Value ?? string.Empty;
            }
        }

        [JsonPropertyName("sysparm_limit")]
        public new int Limit { get => getAttributeValueInt("SysparmLimit") ?? 1000; }

        [JsonPropertyName("sysparm_view")]
        public new string? View { get => getAttributeValueString("SysparmView"); }

        [JsonPropertyName("sysparm_query")]
        public new string? Query { get => getAttributeValueString("SysparmQuery"); }

        [JsonPropertyName("sysparm_offset")]
        public new string? Offset { get => getAttributeValueString("SysparmOffset"); }

        [JsonPropertyName("sysparm_no_count")]
        public new string? NoCount { get => getAttributeValueString("SysparmNoCount"); }

        [JsonPropertyName("sysparm_query_category")]
        public new string? QueryCategory { get => getAttributeValueString("sysparm_query_category"); }

        [JsonPropertyName("sysparm_query_no_domain")]
        public new string? QueryNoDomain { get => getAttributeValueString("sysparm_query_no_domain"); }

        [JsonPropertyName("sysparm_display_value")]
        public new string? DisplayValue { get => getAttributeValueString("SysParmDisplayValue"); }

        [JsonPropertyName("sysparm_exclude_reference_link")]
        public new string? ExcludeReferenceLink { get => getAttributeValueString("SysParmExcludeReferenceLink"); }

        [JsonPropertyName("sysparm_suppress_pagination_header")]
        public new string? SuppressPaginationHeader { get => getAttributeValueString("sysparm_suppress_pagination_header"); }

        [JsonPropertyName("sysparm_fields")]
        public new string? Fields
        {
            get
            {

                string? toReturn = string.Empty;

                var sysParmFields = getAttributeValueString("SysParmFields");
                if (sysParmFields != null)
                    return sysParmFields;

                var sysParmFieldsDisableAuto = path.FirstOrDefault(attrib => attrib.GetType().Name == "SysParmFieldsDisableAuto");

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
