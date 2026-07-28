using System;

namespace Blacus.ServiceNowTableAPIWrapper
{
    /// <summary>
    /// Base attribute class holding an integer configuration value.
    /// </summary>
    public class TableAttributeInt : Attribute
    {
        /// <summary>
        /// Gets or sets the integer attribute value.
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableAttributeInt"/> class.
        /// </summary>
        /// <param name="value">Integer value.</param>
        public TableAttributeInt(int value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// Base attribute class holding a string configuration value.
    /// </summary>
    public class TableAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the string attribute value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableAttribute"/> class.
        /// </summary>
        /// <param name="value">String value.</param>
        public TableAttribute(string value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// Attribute indicating that automatic field filtering should be disabled for the model class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SysParmFieldsDisableAuto : Attribute
    {
    }

    /// <summary>
    /// Specifies the ServiceNow table name associated with a model class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class ServiceNowTableName : TableAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceNowTableName"/> class.
        /// </summary>
        /// <param name="requestPath">The ServiceNow table API name (e.g. <c>sys_user</c>).</param>
        public ServiceNowTableName(string requestPath) : base(requestPath) { }
    }

    /// <summary>
    /// Alias for <see cref="ServiceNowTableName"/> allowing <c>[TableName("...")]</c> usage on models.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class TableNameAttribute : ServiceNowTableName
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableNameAttribute"/> class.
        /// </summary>
        /// <param name="requestPath">The ServiceNow table API name (e.g. <c>sys_user</c>).</param>
        public TableNameAttribute(string requestPath) : base(requestPath) { }
    }

    /// <summary>
    /// Controls the display value behavior for reference fields in queries.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SysParmDisplayValue : TableAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SysParmDisplayValue"/> class.
        /// </summary>
        /// <param name="value">Display value option.</param>
        public SysParmDisplayValue(SysParmDisplayValueOptions value) : base(value.ToString()) { }
    }

    /// <summary>
    /// Options for ServiceNow display value response formats.
    /// </summary>
    public enum SysParmDisplayValueOptions
    {
        /// <summary>
        /// Returns display values for reference fields.
        /// </summary>
        True,

        /// <summary>
        /// Returns raw sys_id values for reference fields.
        /// </summary>
        False,

        /// <summary>
        /// Returns both display value and sys_id value pairs.
        /// </summary>
        All
    }

    /// <summary>
    /// Controls whether reference field links are excluded from responses.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SysParmExcludeReferenceLink : TableAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SysParmExcludeReferenceLink"/> class.
        /// </summary>
        /// <param name="value">Whether to exclude reference links.</param>
        public SysParmExcludeReferenceLink(bool value) : base(value.ToString()) { }
    }

    /// <summary>
    /// Specifies explicit fields to include in query responses.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SysParmFields : TableAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SysParmFields"/> class.
        /// </summary>
        /// <param name="value">Comma-separated field names.</param>
        public SysParmFields(string value) : base(value) { }
    }

    /// <summary>
    /// Specifies the maximum number of records to return per request.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SysparmLimit : TableAttributeInt
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SysparmLimit"/> class.
        /// </summary>
        /// <param name="value">Record limit count.</param>
        public SysparmLimit(int value) : base(value) { }
    }

    /// <summary>
    /// Suppresses total count calculation in query responses for performance optimization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SysparmNoCount : TableAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SysparmNoCount"/> class.
        /// </summary>
        /// <param name="value">Suppression value.</param>
        public SysparmNoCount(string value) : base(value) { }
    }

    /// <summary>
    /// Specifies record offset index for pagination.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SysparmOffset : TableAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SysparmOffset"/> class.
        /// </summary>
        /// <param name="value">Offset string.</param>
        public SysparmOffset(string value) : base(value) { }
    }

    /// <summary>
    /// Defines default encoded query string for the model class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SysparmQuery : TableAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SysparmQuery"/> class.
        /// </summary>
        /// <param name="value">Encoded query string.</param>
        public SysparmQuery(string value) : base(value) { }
    }

    /// <summary>
    /// Specifies query category for logging/tracking in ServiceNow.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SysparmQueryCategory : TableAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SysparmQueryCategory"/> class.
        /// </summary>
        /// <param name="value">Category name.</param>
        public SysparmQueryCategory(string value) : base(value) { }
    }

    /// <summary>
    /// Suppresses domain separation filtering for domain-separated ServiceNow instances.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SysparmQueryNoDomain : TableAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SysparmQueryNoDomain"/> class.
        /// </summary>
        /// <param name="value">Domain filter setting.</param>
        public SysparmQueryNoDomain(string value) : base(value) { }
    }

    /// <summary>
    /// Suppresses pagination header generation in ServiceNow responses.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SysparmSuppressPaginationHeader : TableAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SysparmSuppressPaginationHeader"/> class.
        /// </summary>
        /// <param name="value">Header suppression setting.</param>
        public SysparmSuppressPaginationHeader(string value) : base(value) { }
    }

    /// <summary>
    /// Specifies the ServiceNow form/list view to use when querying fields.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SysparmView : TableAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SysparmView"/> class.
        /// </summary>
        /// <param name="value">View name.</param>
        public SysparmView(string value) : base(value) { }
    }
}
