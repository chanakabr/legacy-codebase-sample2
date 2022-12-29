using KalturaRequestContext;
using Phx.Lib.Log;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using TVinciShared;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Domains;
using WebAPI.ObjectsConvertor.Extensions;

namespace WebAPI.Utils
{
    public class HouseholdUtils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string HOUSEHOLD_KEY = "household";

        public static long GetHouseholdIDByKS(int groupId = 0)
        {
            var ks = KS.GetFromRequest();

            if (ks == null)
                return 0;

            string userID = ks.UserId;

            if (userID == "0")
                return 0;

            if (!RequestContextUtilsInstance.Get().IsPartnerRequest())
            {
                var domainId = KSUtils.ExtractKSPayload().DomainId;
                if (domainId > 0)
                    return domainId;
            }

            KalturaHousehold domain = GetHouseholdFromRequest();
            if (domain == null)
                return 0;

            return domain.getId();
        }

        public static List<string> GetHouseholdUserIds(int groupID, bool withPending = false, int householdId = 0)
        {
            KalturaHousehold domain = null;

            if (householdId == 0)
            {
                var ks = KS.GetFromRequest();
                if (ks == null)
                    return null;

                string userID = ks.UserId;

                if (userID == "0")
                    return null;

                domain = GetHouseholdFromRequest();
                if (domain == null)
                    return null;
            }
            else
            {
                try
                {
                    domain = ClientsManager.DomainsClient().GetDomainInfo(groupID, householdId);
                }
                catch (ClientException ex)
                {
                    log.Error("GetHouseholdIDByKS: got ClientException for GetDomainInfo", ex);
                    return null;
                }
            }

            List<string> userIds = new List<string>();
            
            if (domain.DefaultUsers != null && domain.DefaultUsers.Count > 0)
                userIds.AddRange(domain.DefaultUsers.Select(u => u.Id));

            if (domain.MasterUsers != null && domain.MasterUsers.Count > 0)
                userIds.AddRange(domain.MasterUsers.Select(u => u.Id));

            if (domain.Users != null && domain.Users.Count > 0)
                userIds.AddRange(domain.Users.Select(u => u.Id));

            if (withPending && domain.PendingUsers != null && domain.PendingUsers.Count > 0)
                userIds.AddRange(domain.PendingUsers.Select(u => u.Id));

            return userIds.Distinct().ToList();
        }

        public static KalturaHousehold GetHouseholdFromRequest()
        {
            KalturaHousehold domain = null;

            if (HttpContext.Current.Items.ContainsKey(HOUSEHOLD_KEY))
            {
                domain = (KalturaHousehold)HttpContext.Current.Items[HOUSEHOLD_KEY];
            }
            else
            {
                var ks = KS.GetFromRequest();

                if (ks == null)
                {
                    return null;
                }

                try
                {
                    domain = ClientsManager.DomainsClient().GetDomainByUser(ks.GroupId, ks.UserId);
                }
                catch (ClientException ex)
                {
                    if (!ks.OriginalUserId.IsNullOrEmpty() || !RequestContextUtilsInstance.Get().IsPartnerRequest())
                    {
                        log.Error($"GetHouseholdIDByKS: got ClientException for GetDomainByUser. userId = {ks.UserId} ex ={ex}");
                    }

                    domain = null;
                }
                
            }

            if (domain == null)
                return null;

            if (HttpContext.Current.Items.ContainsKey(HOUSEHOLD_KEY))
            {
                HttpContext.Current.Items[HOUSEHOLD_KEY] = domain;
            }
            else
            {
                HttpContext.Current.Items.Add(HOUSEHOLD_KEY, domain);
            }

            return domain;
        }

        public static bool IsUserMaster()
        {
            var ks = KS.GetFromRequest();

            if (ks == null)
                return false;

            string userID = ks.UserId;

            if (string.IsNullOrEmpty(userID) || userID == "0")
                return false;

            KalturaHousehold domain = GetHouseholdFromRequest();

            if (domain == null)
                return false;

            if (domain.MasterUsers != null && domain.MasterUsers.FirstOrDefault(u => u.Id == userID) != null)
            {
                return true;
            }

            return false;
        }

        public static List<string> GetHouseholdUdids(int groupID, bool withPending = false, int householdId = 0)
        {
            KalturaHousehold domain = null;

            if (householdId == 0)
            {
                var ks = KS.GetFromRequest();
                if (ks == null)
                    return null;

                string userID = ks.UserId;

                if (userID == "0")
                    return null;

                domain = GetHouseholdFromRequest();
                if (domain == null)
                    return null;
            }
            else
            {
                try
                {
                    domain = ClientsManager.DomainsClient().GetDomainInfo(groupID, householdId);
                }
                catch (ClientException ex)
                {
                    log.Error("GetHouseholdIDByKS: got ClientException for GetDomainInfo", ex);
                    return null;
                }
            }

            var udids = new List<string>();

            var devices = ClientsManager.DomainsClient().GetHouseholdDevices(groupID, domain, null, string.Empty);

            if (devices != null)
                udids = devices.Objects.Select(d => d.Udid).ToList();

            return udids;
        }
    }
}
