using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Managers.Scheme;

namespace WebAPI.Reflection
{
    public class MethodParam
    {
        public Type Type { get; set; }
        public Type GenericType { get; set; }
        public bool IsOptional { get; set; }
        public bool IsNullable { get; set; }
        public bool IsEnum { get; set; }
        public bool IsKalturaObject { get; set; }
        public bool IsKalturaMultilingualString { get; set; }
        public bool IsList { get; set; }
        public bool IsMap { get; set; }
        public bool IsDateTime { get; set; }
        public RuntimeSchemeArgumentAttribute SchemeArgument { get; set; }
        public object DefaultValue { get; set; }

        public MethodParam()
        {
            IsOptional = false;
            IsNullable = false;
            IsEnum = false;
            IsKalturaObject = false;
            IsKalturaMultilingualString = false;
            IsList = false;
            IsMap = false;
            IsDateTime = false;
        }
    }
}