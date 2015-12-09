using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Domains;

namespace WebAPI.Utils
{
    public class HouseholdUtils
    {
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
                ErrorUtils.HandleClientException(ex);
            }

            if (domain == null)
                return 0;

            return domain.Id;
        }
    }
}