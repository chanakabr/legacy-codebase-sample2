using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Web.Script.Serialization;

namespace QueueWrapper
{
    public static class Utils
    {
        public static string GetConfigValue(string sKey)
        {
            string sValue = string.Empty;

            try
            {
                sValue = GetValue(sKey);
            }
            catch (Exception ex)
            {
                //Logger.Logger.Log("Catalog Url", "Cannot read catalog url", "Catalog Url");
            }

            return sValue;
        }

        private static string GetValue(string sKey)
        {
            string configuration = ConfigurationManager.AppSettings[sKey];

            if (configuration != null && !configuration.ToString().Equals(string.Empty))
                return configuration.ToString();

            return string.Empty;
        }

        internal static MutexSecurity CreateMutex()
        {
            var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            MutexSecurity mutexSecurity = new MutexSecurity();
            mutexSecurity.AddAccessRule(new MutexAccessRule(sid, MutexRights.FullControl, AccessControlType.Allow));
            mutexSecurity.AddAccessRule(new MutexAccessRule(sid, MutexRights.ChangePermissions, AccessControlType.Deny));
            mutexSecurity.AddAccessRule(new MutexAccessRule(sid, MutexRights.Delete, AccessControlType.Deny));

            return mutexSecurity;
        }

        internal static T JsonToObject<T>(string json)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            T obj = js.Deserialize<T>(json);
            return (T)obj;  
        }
    }
}
