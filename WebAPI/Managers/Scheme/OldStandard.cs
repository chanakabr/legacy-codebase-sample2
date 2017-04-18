using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using WebAPI.Filters;
using WebAPI.Reflection;

namespace WebAPI.Managers.Scheme
{
    abstract public class OldStandardAttribute : Attribute
    {
        public const string Version = "3.6.287.21521";

        public OldStandardAttribute(string oldName)
        {
            this.oldName = oldName;
        }

        public string oldName { get; set; }

        public static bool isCurrentRequestOldVersion()
        {
            if (HttpContext.Current.Items[RequestParser.REQUEST_VERSION] == null)
                return true;


            Version old = new Version(Version);
            Version current = (Version)HttpContext.Current.Items[RequestParser.REQUEST_VERSION];

            return current.CompareTo(old) < 0;
        }

        public static Dictionary<string, string> getOldMembers(MethodInfo action)
        {
            Version currentVersion = (Version)HttpContext.Current.Items[RequestParser.REQUEST_VERSION];
            return DataModel.getOldMembers(action, currentVersion);
        }

        public static Dictionary<string, string> getOldMembers(Type type)
        {
            if (isCurrentRequestOldVersion())
                return DataModel.getOldMembers(type);

            return null;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class OldStandardPropertyAttribute : OldStandardAttribute
    {
        public OldStandardPropertyAttribute(string oldName)
            : base(oldName)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class OldStandardArgumentAttribute : OldStandardAttribute
    {
        public OldStandardArgumentAttribute(string newName, string oldName)
            : base(oldName)
        {
            this.newName = newName;
        }

        public OldStandardArgumentAttribute(string newName, string oldName, string version) :
            this(newName, oldName)
        {
            this.sinceVersion = version;
        }

        public string newName { get; set; }

        public string sinceVersion { get; set; }

        public int CompareVersion(string version)
        {
            if (sinceVersion != null && version == null)
            {
                return 1;
            }
            else if (sinceVersion == null && version != null)
            {
                return -1;
            }
            else if (sinceVersion != null && version != null)
            {
                return (new Version(sinceVersion)).CompareTo(new Version(version));
            }

            return 0;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class OldStandardActionAttribute : OldStandardAttribute
    {
        public OldStandardActionAttribute(string oldName)
            : base(oldName)
        {
        }
    }
}