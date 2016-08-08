using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Domains;

namespace WebAPI.Utils
{
    public class HouseholdUtils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string HOUSEHOLd_KEY = "household";

        public static long GetHouseholdIDByKS(int groupID)
        {
            var ks = KS.GetFromRequest();

            if (ks == null)
                return 0;

            string userID = ks.UserId;

            if (userID == "0")
                return 0;

            KalturaHousehold domain = GetHouseholdFromRequest(groupID, userID);
            if (domain == null)
                return 0;

            return domain.getId();
        }

        public static List<string> GetHouseholdUserIds(int groupID, bool withPending = false)
        {
            var ks = KS.GetFromRequest();

            if (ks == null)
                return null;

            string userID = ks.UserId;

            if (userID == "0")
                return null;

            KalturaHousehold domain = GetHouseholdFromRequest(groupID, userID);
            if (domain == null)
                return null;

            List<string> userIds = new List<string>();
            
            if (domain.DefaultUsers != null && domain.DefaultUsers.Count > 0)
                userIds.AddRange(domain.DefaultUsers.Select(u => u.Id));

            if (domain.MasterUsers != null && domain.MasterUsers.Count > 0)
                userIds.AddRange(domain.MasterUsers.Select(u => u.Id));

            if (domain.Users != null && domain.Users.Count > 0)
                userIds.AddRange(domain.Users.Select(u => u.Id));

            if (withPending && domain.PendingUsers != null && domain.PendingUsers.Count > 0)
                userIds.AddRange(domain.PendingUsers.Select(u => u.Id));

            return userIds;
        }

        public static KalturaHousehold GetHouseholdFromRequest(int groupID, string userID)
        {
            KalturaHousehold domain = null;

            if (HttpContext.Current.Items.Contains(HOUSEHOLd_KEY))
            {
                domain = (KalturaHousehold)HttpContext.Current.Items[HOUSEHOLd_KEY];
            }
            else
            {
                try
                {
                    domain = ClientsManager.DomainsClient().GetDomainByUser(groupID, userID);
                }
                catch (ClientException ex)
                {
                    log.Error("GetHouseholdIDByKS: got ClientException for GetDomainByUser", ex);
                    domain = null;
                }
            }

            if (domain == null)
                return null;

            if (HttpContext.Current.Items.Contains(HOUSEHOLd_KEY))
            {
                HttpContext.Current.Items[HOUSEHOLd_KEY] = domain;
            }
            else
            {
                HttpContext.Current.Items.Add(HOUSEHOLd_KEY, domain);
            }

            return domain;
        }
    }
}