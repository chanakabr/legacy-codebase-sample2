using System;

namespace ApiObjects
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class DBFieldMappingAttribute : Attribute
    {
        public string DbFieldName { get; private set; }

        public DBFieldMappingAttribute(string dbFieldName)
        {
            DbFieldName = dbFieldName;
        }

    }
}
