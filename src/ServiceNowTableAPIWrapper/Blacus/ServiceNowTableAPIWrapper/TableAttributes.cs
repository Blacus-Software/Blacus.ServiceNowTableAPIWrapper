using System;

namespace Blacus.ServiceNowTableAPIWrapper
{

    public class TableAttributeInt : Attribute
    {

        public int Value { get; set; }

        public TableAttributeInt(int value)
        {
            Value = value;
        }

    }

    public class TableAttribute : Attribute
    {

        public string Value { get; set; }

        public TableAttribute(string value)
        {
            Value = value;
        }

    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SysParmFieldsDisableAuto : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class ServiceNowTableName : TableAttribute
    {
        public ServiceNowTableName(string requestPath) : base(requestPath) { }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class TableNameAttribute : ServiceNowTableName
    {
        public TableNameAttribute(string requestPath) : base(requestPath) { }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SysParmDisplayValue : TableAttribute
    {
        public SysParmDisplayValue(SysParmDisplayValueOptions value) : base(value.ToString()) { }
    }

    public enum SysParmDisplayValueOptions
    {
        True,
        False,
        All
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SysParmExcludeReferenceLink : TableAttribute
    {
        public SysParmExcludeReferenceLink(bool value) : base(value.ToString()) { }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SysParmFields : TableAttribute
    {
        public SysParmFields(string value) : base(value) { }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SysparmLimit : TableAttributeInt
    {
        public SysparmLimit(int value) : base(value) { }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SysparmNoCount : TableAttribute
    {
        public SysparmNoCount(string value) : base(value) { }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SysparmOffset : TableAttribute
    {
        public SysparmOffset(string value) : base(value) { }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SysparmQuery : TableAttribute
    {
        public SysparmQuery(string value) : base(value) { }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SysparmQueryCategory : TableAttribute
    {
        public SysparmQueryCategory(string value) : base(value) { }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SysparmQueryNoDomain : TableAttribute
    {
        public SysparmQueryNoDomain(string value) : base(value) { }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SysparmSuppressPaginationHeader : TableAttribute
    {
        public SysparmSuppressPaginationHeader(string value) : base(value) { }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SysparmView : TableAttribute
    {
        public SysparmView(string value) : base(value) { }
    }

}
