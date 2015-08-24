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
        /// <param name="household_id">Household identifier</param>
        /// <param name="user_id">User identifier</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// Household does not exists = 1006, Limitation period = 1014, User not exists in household = 1020, Invalid user = 1026, 
        /// Household suspended = 1009, No users in household = 1017, User not allowed = 1027</remarks>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        public bool Delete(string user_id)
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
                return ClientsManager.DomainsClient().RemoveUserFromDomain(groupId, household_id, user_id);
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
        /// <param name="household_id">Household identifier</param>
        /// <param name="user_id">The identifier of the user to add</param>
        /// <param name="is_master">True if the new user should be added as master user</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// Household suspended = 1009, No users in household = 1017, Action user not master = 1021, User Already In household = 1029
        /// </remarks>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public bool Add(int household_id, string user_id, bool is_master = false)
        {
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                return ClientsManager.DomainsClient().AddUserToDomain(groupId, household_id, user_id, KS.GetFromRequest().UserId, is_master);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return true;
        }
    }
}