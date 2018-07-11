using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.Domains;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("householdPremiumService")]
    public class HouseholdPremiumServiceController : IKalturaController
    {
        /// <summary>
        /// Returns all the premium services allowed for the household
        /// </summary>
        /// <returns></returns>
        [Action("listOldStandard")]
        [OldStandardAction("list")]
        [ApiAuthorize]
        [Obsolete]
        static public List<KalturaPremiumService> ListOldStandard()
        {
            List<KalturaPremiumService> response = null;

            int groupId = KS.GetFromRequest().GroupId;
            long householdId = HouseholdUtils.GetHouseholdIDByKS(groupId);

            try
            {
                response = ClientsManager.ConditionalAccessClient().GetDomainServicesOldStandart(groupId, (int)householdId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Returns all the premium services allowed for the household
        /// </summary>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaHouseholdPremiumServiceListResponse List()
        {
            KalturaHouseholdPremiumServiceListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;
            long householdId = HouseholdUtils.GetHouseholdIDByKS(groupId);

            try
            {
                response = ClientsManager.ConditionalAccessClient().GetDomainServices(groupId, (int)householdId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}