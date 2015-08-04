using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Models.API;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/userParentalPin/action")]
    public class UserParentalPinController : ApiController
    {
        /// <summary>
        /// Retrieve the parental PIN that applies for the user.        
        /// </summary>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// User does not exist = 2000, User with no household = 2024, User suspended = 2001</remarks>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="user_id">User identifier</param>
        /// <returns>The PIN that applies for the user</returns>
        [Route("get"), HttpPost]
        public KalturaPinResponse Get([FromUri] string partner_id, [FromUri] string user_id)
        {
            KalturaPinResponse pinResponse = null;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                pinResponse = ClientsManager.ApiClient().GetUserParentalPIN(groupId, user_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return pinResponse;
        }

        /// <summary>
        /// Set the parental PIN that applies for the user.        
        /// </summary>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// User does not exist = 2000, User with no household = 2024, User suspended = 2001</remarks>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="user_id">User identifier</param>
        /// <param name="pin">New PIN to set</param>
        /// <returns>Success / Fail</returns>
        [Route("update"), HttpPost]
        public bool Update([FromUri] string partner_id, [FromUri] string user_id, [FromUri] string pin)
        {
            bool success = false;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                success = ClientsManager.ApiClient().SetUserParentalPIN(groupId, user_id, pin);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return success;
        }

        /// <summary>
        /// Validate that a given parental PIN for a user is valid.        
        /// </summary>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// No PIN defined = 5001, PIN mismatch = 5002, User does not exist = 2000, User with no household = 2024, User suspended = 2001</remarks>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="user_id">User identifier</param>
        /// <param name="pin">PIN to validate</param>
        /// <returns>Success / fail</returns>
        [Route("validate"), HttpPost]
        public bool Validate([FromUri] string partner_id, [FromUri] string user_id, [FromUri] string pin)
        {
            bool success = false;
            
            int groupId = int.Parse(partner_id);

            // parameters validation
            if (string.IsNullOrEmpty(pin))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "pin cannot be empty");
            }

            try
            {
                // call client
                success = ClientsManager.ApiClient().ValidateParentalPIN(groupId, user_id, pin);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return success;
        }
    }
}