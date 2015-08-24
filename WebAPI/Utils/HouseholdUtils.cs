using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.ClientManagers.Client;
using WebAPI.Managers.Models;
using WebAPI.Models.Domains;

namespace WebAPI.Utils
{
    public class HouseholdUtils
    {
        public static long getHouseholdIDByKS(int groupID)
        {
            var ks = KS.GetFromRequest();

            if (ks == null)
                return 0;

            string userID = ks.UserId;

            if (userID == "0")
                return 0;

            var domain = ClientsManager.DomainsClient().GetDomainByUser(groupID, userID);

            if (domain == null)
                return 0;

            return domain.Id;
        }
    }
}