using ApiObjects.Response;
using System;
using System.Collections.Generic;

namespace WebAPI.Managers.Scheme
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceAttribute : Attribute
    {
        public ServiceAttribute(string name, bool isInternal = false)
        {
            Name = name;
            IsInternal = isInternal;
        }

        /// <summary>
        /// Service name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Prevents from this service to be written in client xml
        /// </summary>
        public bool IsInternal{ get; set; }
}
    
    [AttributeUsage(AttributeTargets.Method)]
    public class ActionAttribute : Attribute
    {
        public ActionAttribute(string name, bool isInternal = false)
        {
            Name = name;
            IsInternal = isInternal;
        }

        /// <summary>
        /// Action name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Prevents from this action to be written in xml
        /// </summary>
        public bool IsInternal { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class InternalObjectAttribute : Attribute
    {
        public InternalObjectAttribute(bool isInternal = false)
        {
            IsInternal = isInternal;
        }

        /// <summary>
        /// Prevents from this action to be written in xml
        /// </summary>
        public bool IsInternal { get; set; }
    }
}