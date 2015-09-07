using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Users;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/userLoginPin/action")]
    public class UserLoginPinController : ApiController
    {
        /// <summary>
        /// Generates a temporarily PIN that can allow a user to log-in.
        /// </summary>        
        /// <param name="secret">Additional security parameter for optional enhanced security</param>
        /// <remarks>Possible status codes: User doesn't exist = 2000, User suspended = 2001
        /// </remarks>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public KalturaLoginPin Add(string secret = null)
        {
            KalturaLoginPin response = null;
            
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.UsersClient().GenerateLoginPin(groupId, KS.GetFromRequest().UserId, secret);
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
        /// <param name="pin_code">Device Identifier</param>
        /// <param name="secret">Additional security parameter to validate the login</param>
        /// <remarks>Possible status codes: MissingSecurityParameter = 2007, LoginViaPinNotAllowed = 2009, PinNotInTheRightLength = 2010,PinExists = 2011
        /// </remarks>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        public KalturaLoginPin Update(string pin_code, string secret = null)
        {
            KalturaLoginPin res = null;
            int groupId = KS.GetFromRequest().GroupId;

            if (string.IsNullOrEmpty(pin_code))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "pin cannot be empty");
            }

            try
            {
                // call client
                res = ClientsManager.UsersClient().SetLoginPin(groupId, KS.GetFromRequest().UserId, pin_code, secret);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return res;
        }

        /// <summary>
        /// Immediately deletes all pre set login pin codes for the user.
        /// </summary>
        /// <remarks></remarks>
        [Route("deleteAll"), HttpPost]
        [ApiAuthorize]
        public bool DeleteAll()
        {
            bool res = false;
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                res = ClientsManager.UsersClient().ClearLoginPIN(groupId, KS.GetFromRequest().UserId, null);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return res;
        }

        /// <summary>
        /// Immediately deletes a given pre set login pin code for the user.
        /// </summary>
        /// <param name="pin_code">Login pin code to expire</param>
        /// <remarks></remarks>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        public bool Delete(string pin_code)
        {
            bool res = false;
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                res = ClientsManager.UsersClient().ClearLoginPIN(groupId, KS.GetFromRequest().UserId, pin_code);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return res;
        }
    }
}