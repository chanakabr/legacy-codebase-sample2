using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Domains;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/householdUser/action")]
    [OldStandardAction("addOldStandard", "add")]
    public class HouseholdUserController : ApiController
    {
        /// <summary>
        /// Removes a user from household   
        /// </summary>                
        /// <param name="userId">The identifier of the user to delete</param>
        /// <remarks>Possible status codes: 
        /// Household does not exists = 1006, Limitation period = 1014, User not exists in household = 1020, Invalid user = 1026, 
        /// Household suspended = 1009, No users in household = 1017, User not allowed = 1027</remarks>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        [OldStandard("userId", "user_id_to_delete")]
        [Throws(eResponseStatus.HouseholdNotExists)]
        [Throws(eResponseStatus.LimitationPeriod)]
        [Throws(eResponseStatus.UserNotExistsInHousehold)]
        [Throws(eResponseStatus.InvalidUser)]
        [Throws(eResponseStatus.HouseholdSuspended)]
        [Throws(eResponseStatus.NoUsersInHousehold)]
        [Throws(eResponseStatus.UserNotAllowed)]
        public bool Delete(string userId)
        {
            int groupId = KS.GetFromRequest().GroupId;
            string masterUserId = KS.GetFromRequest().UserId;

            try
            {                
                int household_id = 0;
                string requestUserId = KS.GetFromRequest().UserId;

                if (requestUserId != "0")
                {
                    var domain = ClientsManager.DomainsClient().GetDomainByUser(groupId, requestUserId);
                    household_id = (int) domain.Id;
                }

                // call client
                return ClientsManager.DomainsClient().RemoveUserFromDomain(groupId, household_id, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return true;
        }

        /// <summary>
        /// Adds a user to household       
        /// </summary>                
        /// <param name="householdUser">User details to add</param>
        /// <remarks>Possible status codes: 
        /// Household suspended = 1009, Invalid user = 1026, User Already In household = 1029, User not allowed = 1027, Household suspended = 1009, 
        /// Action user is not master = 1021, No users in household = 1017, User exists in other households = 1018, User not allowed = 1027, Exceeded user limit = 1022, Request failed = 1025, 
        /// </remarks>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.HouseholdSuspended)]
        [Throws(eResponseStatus.InvalidUser)]
        [Throws(eResponseStatus.UserAlreadyInHousehold)]
        [Throws(eResponseStatus.UserNotAllowed)]
        [Throws(eResponseStatus.ActionUserNotMaster)]
        [Throws(eResponseStatus.NoUsersInHousehold)]
        [Throws(eResponseStatus.UserExistsInOtherHouseholds)]
        [Throws(eResponseStatus.ExceededUserLimit)]
        [Throws(eResponseStatus.RequestFailed)]
        public KalturaHouseholdUser Add(KalturaHouseholdUser householdUser)
        {
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // get domain id       
                int householdId = 0;
                string masterId = "";

                if (householdUser.HouseholdId.HasValue)
                {
                    householdId = householdUser.HouseholdId.Value;
                    KalturaHousehold household = ClientsManager.DomainsClient().GetDomainInfo(groupId, householdId);

                    // check if the user performing the action is domain master
                    if (household.MasterUsers.FirstOrDefault() != null)
                        masterId = household.MasterUsers.FirstOrDefault().Id;
                }
                else if (string.IsNullOrEmpty(householdUser.HouseholdMasterUsername))
                {
                    householdId = (int)HouseholdUtils.GetHouseholdIDByKS(groupId);
                    masterId = KS.GetFromRequest().UserId;
                }

                if (householdId > 0)
                {
                    ClientsManager.DomainsClient().AddUserToDomain(groupId, (int)householdId, householdUser.UserId, masterId, householdUser.getIsMaster());
                    householdUser.Status = KalturaHouseholdUserStatus.OK;
                    householdUser.HouseholdId = (int)householdId;
                }
                else if (!string.IsNullOrEmpty(householdUser.HouseholdMasterUsername))
                {
                    householdUser.UserId = KS.GetFromRequest().UserId;
                    var household = ClientsManager.DomainsClient().SubmitAddUserToDomainRequest(groupId, householdUser.UserId, householdUser.HouseholdMasterUsername);
                    householdUser.Status = KalturaHouseholdUserStatus.PENDING;
                    householdUser.HouseholdId = (int)household.Id;
                }
                else
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, "householdMasterUsername, householdId");
                }

                householdUser.IsMaster = householdUser.getIsMaster();
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return householdUser;
        }

        /// <summary>
        /// Adds a user to household       
        /// </summary>                
        /// <param name="user_id_to_add">The identifier of the user to add</param>
        /// <param name="is_master">True if the new user should be added as master user</param>
        /// <remarks>Possible status codes: 
        /// Household suspended = 1009, Invalid user = 1026, User Already In household = 1029
        /// </remarks>
        [Route("addOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        [Throws(eResponseStatus.HouseholdSuspended)]
        [Throws(eResponseStatus.UserAlreadyInHousehold)]
        public bool AddOldStandard(string user_id_to_add, bool is_master = false)
        {
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // get domain id       
                var domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);

                // call client
                return ClientsManager.DomainsClient().AddUserToDomain(groupId, (int)domainId, user_id_to_add, KS.GetFromRequest().UserId, is_master) != null;
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return true;
        }

        /// <summary>
        /// Allow the Operator to add a user to a household     
        /// </summary>                
        /// <param name="user_id_to_add">The identifier of the user to add</param>
        /// <param name="household_id">Household to add the user to</param>
        /// <param name="is_master">True if the new user should be added as master user</param>
        /// <remarks>Possible status codes: 
        /// Household suspended = 1009, Invalid user = 1026, User Already In household = 1029
        /// </remarks>
        [Route("addByOperator"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        [Throws(eResponseStatus.HouseholdSuspended)]
        [Throws(eResponseStatus.InvalidUser)]
        [Throws(eResponseStatus.UserAlreadyInHousehold)]
        public bool AddByOperator(string user_id_to_add, int household_id, bool is_master = false)
        {
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // get domain master id
                string masterId = "";
                var domain = ClientsManager.DomainsClient().GetDomainInfo(groupId, household_id);
                // check if the user performing the action is domain master
                if (domain.MasterUsers.FirstOrDefault() != null)
                    masterId = domain.MasterUsers.FirstOrDefault().Id;

                // call client
                return ClientsManager.DomainsClient().AddUserToDomain(groupId, household_id, user_id_to_add, masterId, is_master) != null;
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return true;
        }

        /// <summary>
        /// Returns the users within the household
        /// </summary>                
        /// <param name="filter">Household user filter</param>
        /// <remarks>Possible status codes: 
        /// Household does not exist = 1006, Household user failed = 1007  
        /// </remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.HouseholdNotExists)]
        [Throws(eResponseStatus.HouseholdUserFailed)]
        public KalturaHouseholdUserListResponse List(KalturaHouseholdUserFilter filter = null)
        {
            KalturaHouseholdUserListResponse response = new KalturaHouseholdUserListResponse(); 
            int groupId = KS.GetFromRequest().GroupId;
            try
            {
                KalturaHousehold household = null;
                if (filter != null && filter.HouseholdIdEqual.HasValue && filter.HouseholdIdEqual.Value > 0)
                {
                    household = ClientsManager.DomainsClient().GetDomainInfo(groupId, filter.HouseholdIdEqual.Value);
                }
                else
                {
                    household = HouseholdUtils.GetHouseholdFromRequest();
                }

                response.Objects = ClientsManager.DomainsClient().GetHouseholdUsers(groupId, household);
                response.TotalCount = response.Objects.Count;
                
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}