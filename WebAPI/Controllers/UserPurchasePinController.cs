using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.API;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/userPurchasePin/action")]
    public class UserPurchasePinController : ApiController
    {
        /// <summary>
        /// Retrieve the purchase PIN that applies for the user.        
        /// </summary>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// 5001 = No PIN defined, User does not exist = 2000, User with no household = 2024, User suspended = 2001</remarks>
        /// <returns>The PIN that applies for the user</returns>
        [Route("get"), HttpPost]
        [ApiAuthorize]
        public KalturaPurchaseSettingsResponse Get()
        {
            KalturaPurchaseSettingsResponse pinResponse = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                pinResponse = ClientsManager.ApiClient().GetUserPurchasePIN(groupId, KS.GetFromRequest().UserId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return pinResponse;
        }

        /// <summary>
        /// Set the purchase PIN that applies for the user.        
        /// </summary>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// User does not exist = 2000, User with no household = 2024, User suspended = 2001</remarks>
        /// <param name="pin">New PIN to apply</param>
        /// <returns>Success / Fail</returns>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        public bool Update(string pin)
        {
            bool success = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                success = ClientsManager.ApiClient().SetUserPurchasePIN(groupId, KS.GetFromRequest().UserId, pin);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return success;
        }

        /// <summary>
        /// Validate that a given purchase PIN for a user is valid.         
        /// </summary>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// No PIN defined = 5001, PIN mismatch = 5002, User does not exist = 2000, User with no household = 2024, User suspended = 2001</remarks>
        /// <param name="pin">PIN to validate</param>
        /// <returns>Success / fail</returns>
        [Route("validate"), HttpPost]
        [ApiAuthorize]
        public bool Validate(string pin)
        {
            bool success = false;

            int groupId = KS.GetFromRequest().GroupId;

            // parameters validation
            if (string.IsNullOrEmpty(pin))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "pin cannot be empty");
            }

            try
            {
                // call client
                success = ClientsManager.ApiClient().ValidatePurchasePIN(groupId, KS.GetFromRequest().UserId, pin);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return success;
        }
    }
}