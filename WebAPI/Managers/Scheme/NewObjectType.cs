using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Managers.Scheme
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NewObjectTypeAttribute : Attribute
    {
        public Type type { get; set; }

        public NewObjectTypeAttribute(Type newType)
        {
            this.type = newType;
        }
    }
}