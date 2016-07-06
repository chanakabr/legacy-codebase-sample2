using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using WebAPI.Filters;

namespace WebAPI.Managers.Schema
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class OldStandardAttribute : Attribute
    {
        private const string version = "3.6.287.21521";

        public OldStandardAttribute(string newMember, string oldMember)
        {
            this.newMember = newMember;
            this.oldMember = oldMember;
        }

        public string newMember { get; set; }

        public string oldMember { get; set; }

        public static bool isCurrentRequestOldVersion()
        {
            if (string.IsNullOrEmpty((string)HttpContext.Current.Items[RequestParser.REQUEST_VERSION]))
                return true;


            Version old = new Version(version);
            Version current = new Version((string)HttpContext.Current.Items[RequestParser.REQUEST_VERSION]);

            return current.CompareTo(old) < 0;
        }

        public static Dictionary<string, string> getOldMembers(MethodInfo action)
        {
            if (!isCurrentRequestOldVersion())
                return null;

            Dictionary<string, string> map = new Dictionary<string, string>();
            OldStandardAttribute[] attributes = (OldStandardAttribute[])Attribute.GetCustomAttributes(action, typeof(OldStandardAttribute));
            foreach (OldStandardAttribute attribute in attributes)
            {
                map.Add(attribute.newMember, attribute.oldMember);
            }

            if (map.Count == 0)
                return null;

            return map;
        }

        public static Dictionary<string, string> getOldMembers(Type type)
        {
            if (!isCurrentRequestOldVersion())
                return null;

            Dictionary<string, string> map = new Dictionary<string, string>();
            OldStandardAttribute[] attributes = (OldStandardAttribute[])Attribute.GetCustomAttributes(type, typeof(OldStandardAttribute));
            foreach (OldStandardAttribute attribute in attributes)
            {
                map.Add(attribute.newMember, attribute.oldMember);
            }

            if (map.Count == 0)
                return null;

            return map;
        }
    }

    public class OldStandardActionAttribute : OldStandardAttribute
    {
        public OldStandardActionAttribute(string newMember, string oldMember) : base(newMember, oldMember.ToLower())
        {
        }
    }
}