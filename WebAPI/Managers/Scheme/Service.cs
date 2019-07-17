using System;

namespace WebAPI.Managers.Scheme
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceAttribute : Attribute
    {
        public ServiceAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ActionAttribute : Attribute
    {
        public ActionAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}