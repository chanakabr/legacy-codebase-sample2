using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Models.Users;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("user_login_pin")]
    public class UserLoginPinController : ApiController
    {
        /// <summary>
        /// Generates a temporarily PIN that can allow a user to log-in.
        /// </summary>        
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="user_id">User Identifier</param>
        /// <param name="secret">Additional security parameter for optional enhanced security</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, User doesn't exist = 2000, User suspended = 2001
        /// </remarks>
        [Route("{user_id}/pin/generate"), HttpPost]
        public KalturaLoginPin Add([FromUri] string partner_id, [FromUri] string user_id, [FromUri] string secret = null)
        {
            KalturaLoginPin response = null;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                response = ClientsManager.UsersClient().GenerateLoginPin(groupId, user_id, secret);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Set a temporarily PIN that can allow a user to log-in.        
        /// </summary>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="user_id">User Identifier</param>
        /// <param name="pin">Device Identifier</param>
        /// <param name="secret">Additional security parameter to validate the login</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, MissingSecurityParameter = 2007, LoginViaPinNotAllowed = 2009, PinNotInTheRightLength = 2010,PinExists = 2011
        /// </remarks>
        [Route("{user_id}/pin"), HttpPost]
        public void Update([FromUri] string partner_id, [FromUri] string user_id, [FromUri] string pin, [FromUri] string secret = null)
        {
            int groupId = int.Parse(partner_id);

            if (string.IsNullOrEmpty(pin))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "pin cannot be empty");
            }

            try
            {
                // call client
                ClientsManager.UsersClient().SetLoginPin(groupId, user_id, pin, secret);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
        }

        /// <summary>
        /// Immediately expires a pre-set login-PIN.
        /// </summary>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="user_id">User Identifier</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008</remarks>
        [Route("{user_id}/pin"), HttpDelete]
        public void Delete([FromUri] string partner_id, [FromUri] string user_id)
        {
            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                ClientsManager.UsersClient().ClearLoginPIN(groupId, user_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
        }
    }
}