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
            Version currentVersion = (Version)HttpContext.Current.Items[RequestParser.REQUEST_VERSION];
            return DataModel.getOldMembers(type, currentVersion);

            return null;
        }
    }

    abstract public class OldStandardVersionedAttribute : OldStandardAttribute
    {
        public OldStandardVersionedAttribute(string oldName)
            : base(oldName)
        {
        }

        public OldStandardVersionedAttribute(string oldName, string version)
            : base(oldName)
        {
            this.sinceVersion = version;
        }

        public string sinceVersion { get; set; }

        public virtual int Compare(OldStandardVersionedAttribute attribute)
        {
            if (sinceVersion != null && attribute.sinceVersion == null)
            {
                return 1;
            }
            else if (sinceVersion == null && attribute.sinceVersion != null)
            {
                return -1;
            }
            else if (sinceVersion != null && attribute.sinceVersion != null)
            {
                return (new Version(sinceVersion)).CompareTo(new Version(attribute.sinceVersion));
            }

            return 0;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class OldStandardPropertyAttribute : OldStandardVersionedAttribute
    {
        public OldStandardPropertyAttribute(string oldName)
            : base(oldName)
        {
        }

        public OldStandardPropertyAttribute(string oldName, string version)
            : base(oldName, version)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class OldStandardArgumentAttribute : OldStandardVersionedAttribute
    {
        public OldStandardArgumentAttribute(string newName, string oldName)
            : base(oldName)
        {
            this.newName = newName;
        }

        public OldStandardArgumentAttribute(string newName, string oldName, string version) :
            base(oldName, version)
        {
            this.newName = newName;
        }

        public string newName { get; set; }

        public override int Compare(OldStandardVersionedAttribute attribute)
        {
            int compare = base.Compare(attribute);
            if (compare != 0)
            {
                return compare;
            }

            return newName.CompareTo((attribute as OldStandardArgumentAttribute).newName);
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