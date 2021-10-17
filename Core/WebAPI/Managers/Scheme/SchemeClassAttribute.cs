using System;
using System.Collections.Generic;
using System.Text;

namespace WebAPI.Managers.Scheme
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SchemeClassAttribute : Attribute
    {
        public string[] Required { get; set; }
    }
}
