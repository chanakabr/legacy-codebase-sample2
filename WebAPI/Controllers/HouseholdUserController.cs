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
        public bool Delete(int household_id, string user_id)
        {
            int groupId = KS.GetFromRequest().GroupId;                                   

            try
            {
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
        /// <param name="user_id">User identifier</param>
        /// <param name="master_user_id">Identifier of household master</param>
        /// <param name="is_master">True if the new user should be set to be master</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// Household suspended = 1009, No users in household = 1017, Action user not master = 1021, User Already In household = 1029
        /// </remarks>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public bool Add(int household_id, string user_id, string master_user_id, bool is_master = false)
        {
            int groupId = KS.GetFromRequest().GroupId;                                   

            try
            {
                // call client
                return ClientsManager.DomainsClient().AddUserToDomain(groupId, household_id, user_id, master_user_id, is_master);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return true;
        }
    }
}