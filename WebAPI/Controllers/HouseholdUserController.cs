using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/householdUser/action")]
    public class HouseholdUserController : ApiController
    {
        /// <summary>
        /// Removes a user from household   
        /// </summary>                
        /// <param name="user_id_to_delete">The identifier of the user to delete</param>
        /// <remarks>Possible status codes: 
        /// Household does not exists = 1006, Limitation period = 1014, User not exists in household = 1020, Invalid user = 1026, 
        /// Household suspended = 1009, No users in household = 1017, User not allowed = 1027</remarks>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        public bool Delete(string user_id_to_delete)
        {
            int groupId = KS.GetFromRequest().GroupId;
            string masterUserId = KS.GetFromRequest().UserId;

            try
            {                
                int household_id = 0;
                string userID = KS.GetFromRequest().UserId;

                // get domain       
                var domain = ClientsManager.DomainsClient().GetDomainByUser(groupId, userID);
                if (userID != "0")
                    household_id = (int) domain.Id;
        
                // check if the user performing the action is domain master
                if (domain.MasterUsers.Where(u => u.Id == masterUserId).FirstOrDefault() == null)
                {
                    throw new ForbiddenException();
                }

                // call client
                return ClientsManager.DomainsClient().RemoveUserFromDomain(groupId, household_id, user_id_to_delete);
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
        /// <param name="user_id_to_add">The identifier of the user to add</param>
        /// <param name="is_master">True if the new user should be added as master user</param>
        /// <remarks>Possible status codes: 
        /// Household suspended = 1009, No users in household = 1017, Action user not master = 1021, Invalid user = 1026, User Already In household = 1029
        /// </remarks>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public bool Add(string user_id_to_add, bool is_master = false)
        {
            int groupId = KS.GetFromRequest().GroupId;
          
            try
            {
                // get domain id       
                var domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);

                // call client
                return ClientsManager.DomainsClient().AddUserToDomain(groupId, (int)domainId, user_id_to_add, KS.GetFromRequest().UserId, is_master);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return true;
        }

        /// <summary>
        /// Adds a user to a specific household       
        /// </summary>                
        /// <param name="user_id_to_add">The identifier of the user to add</param>
        /// <param name="household_id">Household to add the user to</param>
        /// <param name="is_master">True if the new user should be added as master user</param>
        /// <remarks>Possible status codes: 
        /// Household suspended = 1009, No users in household = 1017, Action user not master = 1021, Invalid user = 1026, User Already In household = 1029
        /// </remarks>
        [Route("addByOperator"), HttpPost]
        [ApiAuthorize]
        public bool AddByOperator(string user_id_to_add, int household_id, bool is_master = false)
        {
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                return ClientsManager.DomainsClient().AddUserToDomain(groupId, household_id, user_id_to_add, KS.GetFromRequest().UserId, is_master);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return true;
        }
    }
}