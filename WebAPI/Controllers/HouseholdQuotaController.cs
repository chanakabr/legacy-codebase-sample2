using System;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Utils;


namespace WebAPI.Controllers
{
    [Service("householdQuota")]
    public class HouseholdQuotaController : IKalturaController
    {
        /// <summary>
        /// Returns the household's quota data
        /// </summary>
        /// <returns></returns>
        /// <remarks>Possible status codes: BadRequest = 500003</remarks>     
        [Action("get")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        static public KalturaHouseholdQuota Get()
        {
            KalturaHouseholdQuota response = null;

            try
            {
                var ks = KSManager.GetKSFromRequest();
                int groupId = ks.GroupId;
                long domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);
                string userId = ks.UserId;

                // call client                
                response = ClientsManager.ConditionalAccessClient().GetDomainQuota(groupId, userId, domainId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}