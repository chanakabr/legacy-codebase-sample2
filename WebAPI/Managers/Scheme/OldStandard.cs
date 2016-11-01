using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using WebAPI.Filters;
using WebAPI.Reflection;

namespace WebAPI.Managers.Scheme
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
            if (HttpContext.Current.Items[RequestParser.REQUEST_VERSION] == null)
                return true;


            Version old = new Version(version);
            Version current = (Version) HttpContext.Current.Items[RequestParser.REQUEST_VERSION];

            return current.CompareTo(old) < 0;
        }

        public static Dictionary<string, string> getOldMembers(MethodInfo action)
        {
            if (isCurrentRequestOldVersion())
                return DataModel.getOldMembers(action);
                
            return null;
        }

        public static Dictionary<string, string> getOldMembers(Type type)
        {
            if (isCurrentRequestOldVersion())
                return DataModel.getOldMembers(type);

            return null;
        }
    }

    public class OldStandardActionAttribute : OldStandardAttribute
    {
        public OldStandardActionAttribute(string newMember, string oldMember) : base(newMember, oldMember.ToLower())
        {
        }
    }
}