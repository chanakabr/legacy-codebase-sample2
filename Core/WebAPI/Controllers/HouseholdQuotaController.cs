using ApiObjects.Response;
using System;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
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
        [Throws(eResponseStatus.AccountCdvrNotEnabled)]
        [Throws(eResponseStatus.ServiceNotAllowed)]
        [Throws(eResponseStatus.UserWithNoDomain)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserNotInDomain)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.DomainNotExists)]
        [Throws(eResponseStatus.HouseholdUserFailed)]
        static public KalturaHouseholdQuota Get()
        {
            KalturaHouseholdQuota response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                long domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);
                string userId = KS.GetFromRequest().UserId;

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