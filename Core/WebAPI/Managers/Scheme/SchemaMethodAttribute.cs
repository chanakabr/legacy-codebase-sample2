using System;

namespace WebAPI.Managers.Scheme
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SchemaMethodAttribute : Attribute
    {
        public string[] OneOf { get; set; }
    }
}