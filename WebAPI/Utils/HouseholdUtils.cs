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

        public static long GetHouseholdIDByKS(int groupID)
        {
            var ks = KS.GetFromRequest();

            if (ks == null)
                return 0;

            string userID = ks.UserId;

            if (userID == "0")
                return 0;

            KalturaHousehold domain = null;

            try
            {
                domain = ClientsManager.DomainsClient().GetDomainByUser(groupID, userID);
            }
            catch (ClientException ex)
            {
                log.Error("GetHouseholdIDByKS: got ClientException for GetDomainByUser", ex);
                domain = null;
            }

            if (domain == null)
                return 0;

            return domain.Id;
        }
    }
}