using ApiObjects;
using ApiObjects.CDNAdapter;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;

namespace CachingHelpers
{
    public class Utils
    {

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        internal static MutexSecurity CreateMutex()
        {
            var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            MutexSecurity mutexSecurity = new MutexSecurity();
            mutexSecurity.AddAccessRule(new MutexAccessRule(sid, MutexRights.FullControl, AccessControlType.Allow));
            mutexSecurity.AddAccessRule(new MutexAccessRule(sid, MutexRights.ChangePermissions, AccessControlType.Deny));
            mutexSecurity.AddAccessRule(new MutexAccessRule(sid, MutexRights.Delete, AccessControlType.Deny));

            return mutexSecurity;
        }

        internal static Tuple<CDNAdapter, bool> GetCdnAdapter(Dictionary<string, object> funcParams)
        {
            bool res = false;
            CDNAdapter result = null;
            try
            {
                if (funcParams != null && funcParams.Count >= 1)
                {
                    bool shouldGetOnlyActive = true;
                    if (funcParams.ContainsKey("shouldGetOnlyActive"))
                    {
                        shouldGetOnlyActive = (bool)funcParams["shouldGetOnlyActive"];
                    }
                    if (funcParams.ContainsKey("adapterId"))
                    {
                        int? adapterId;
                        adapterId = funcParams["adapterId"] as int?;
                        result = DAL.ApiDAL.GetCDNAdapter(adapterId.Value, shouldGetOnlyActive);
                        res = true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetCdnAdapter failed params : {0}", string.Join(";", funcParams.Keys)), ex);
            }
            return new Tuple<CDNAdapter, bool>(result, res);
        }

        internal static Tuple<CDNPartnerSettings, bool> GetCdnSettings(Dictionary<string, object> funcParams)
        {
            bool res = false;
            CDNPartnerSettings result = null;
            try
            {
                if (funcParams != null && funcParams.Count == 1)
                {
                    if (funcParams.ContainsKey("groupId"))
                    {
                        int? groupId;
                        groupId = funcParams["groupId"] as int?;
                        result = DAL.ApiDAL.GetCdnSettings(groupId.Value);
                        res = true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetCdnSettings failed params : {0}", string.Join(";", funcParams.Keys)), ex);
            }
            return new Tuple<CDNPartnerSettings, bool>(result, res);
        }    

    }
}
