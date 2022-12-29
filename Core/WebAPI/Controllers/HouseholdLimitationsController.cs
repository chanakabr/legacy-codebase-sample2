using ApiObjects.Response;
using TVinciShared;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Domains;
using WebAPI.ModelsValidators;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("householdLimitations")]
    public class HouseholdLimitationsController : IKalturaController
    {
        /// <summary>
        /// Add household limitation
        /// </summary>
        /// <param name="householdLimitations">Household limitations</param>
        /// <returns></returns>
        [Action("add")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public static KalturaHouseholdLimitations Add(KalturaHouseholdLimitations householdLimitations) 
        {
            KalturaHouseholdLimitations response = null;
            var groupId = KS.GetFromRequest().GroupId;
            var userId = Utils.Utils.GetUserIdFromKs();
            householdLimitations.ValidateAssociatedDevices();

            try
            {
                response = ClientsManager.DomainsClient().AddDomainLimitationModule(groupId, householdLimitations, userId);
            }
            catch (ClientException e)
            {
                ErrorUtils.HandleClientException(e);
            }

            return response;
        }
        
        /// <summary>
        /// Get the limitation module by id
        /// </summary>
        /// <param name="id">Household limitations module identifier</param>
        /// <returns></returns>
        [Action("get")]
        [ApiAuthorize]
        [Throws(eResponseStatus.DlmNotExist)]
        [Throws(eResponseStatus.WrongPasswordOrUserName)]
        static public KalturaHouseholdLimitations Get(int id)
        {
            KalturaHouseholdLimitations response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                response = ClientsManager.DomainsClient().GetDomainLimitationModule(groupId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Get the list of PartnerConfiguration
        /// </summary>
        /// <param name="filter">filter by PartnerConfiguration type</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaHouseholdLimitationsListResponse List()
        {
            KalturaHouseholdLimitationsListResponse response = null;
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                response = ClientsManager.DomainsClient().GetDomainLimitationModule(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Delete household limitation
        /// </summary>
        /// <param name="householdLimitationsId">Id of household limitation</param>
        /// <returns>true if success</returns>
        [Action("delete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.DlmNotExist)]
        public static bool Delete(int householdLimitationsId)
        {
            try
            {
                var userId = Utils.Utils.GetUserIdFromKs();
                var groupId = KS.GetFromRequest().GroupId;
                var result = ClientsManager.DomainsClient().DeleteDomainLimitationModule(groupId, householdLimitationsId, userId);

                return result;
            }
            catch (ClientException e)
            {
                ErrorUtils.HandleClientException(e);
            }

            return false;
        }

        /// <summary>
        /// Updates household limitation
        /// </summary>
        /// <param name="dlmId">Id of household limitation</param>
        /// <param name="householdLimitation"> household limitation</param>
        /// <returns>true if success</returns>
        [Action("update")]
        [ApiAuthorize]
        [Throws(eResponseStatus.DlmNotExist)]
        public static KalturaHouseholdLimitations Update(int dlmId, KalturaHouseholdLimitations householdLimitation)
        {
            KalturaHouseholdLimitations response = null;
            householdLimitation.ValidateUpdate();

            try
            {
                var userId = Utils.Utils.GetUserIdFromKs();
                var groupId = KS.GetFromRequest().GroupId;

                response = ClientsManager.DomainsClient().UpdateDLM(dlmId, groupId, householdLimitation, userId);
            }
            catch (ClientException e)
            {
                ErrorUtils.HandleClientException(e);
            }

            return response;
        }

        /// <summary>
        /// Checks if the DLM is used
        /// </summary>
        /// <param name="dlmId">Household limitations module identifier</param>
        /// <returns></returns>
        [Action("isUsed")]
        [ApiAuthorize]
        [Throws(eResponseStatus.DlmNotExist)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        static public bool IsUsed(int dlmId)
        {
            bool response = true;

            int groupId = KS.GetFromRequest().GroupId;

            try 
            {
                return ClientsManager.DomainsClient().IsDLMInUse(groupId, dlmId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }
    }
}
