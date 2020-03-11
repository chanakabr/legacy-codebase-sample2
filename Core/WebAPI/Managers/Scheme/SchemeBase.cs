using System;

namespace WebAPI.Managers.Scheme
{
    public class SchemeBaseAttribute : Attribute
    {
        public Type BaseType { get; set; }

        public SchemeBaseAttribute(Type baseType)
        {
            BaseType = baseType;
        }
    }
}