using ApiObjects.Response;
using System;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Users;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("userLoginPin")]
    public class UserLoginPinController : IKalturaController
    {
        /// <summary>
        /// Generate a time and usage expiry login-PIN that can allow a single/multiple login/s per PIN. 
        /// If an active login-PIN already exists. Calling this API again for same user will add another login-PIN 
        /// </summary>        
        /// <param name="secret">Additional security parameter for optional enhanced security</param>
        /// <param name="pinUsages">Optional number of pin usages</param>
        /// <param name="pinDuration">Optional duration in minutes of the pin</param>
        /// <remarks>Possible status codes: User doesn't exist = 2000, User suspended = 2001, InvalidParameters = 7010
        /// </remarks>
        [Action("add")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.InvalidParameters)]
        static public KalturaUserLoginPin Add(string secret = null, int? pinUsages = null, int? pinDuration = null)
        {
            KalturaUserLoginPin response = null;
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                ClientsManager.UsersClient().ValidatePinEnhancements(pinUsages, pinDuration);
                // call client
                response = ClientsManager.UsersClient().GenerateLoginPin(groupId, KS.GetFromRequest().UserId, secret, pinUsages, pinDuration);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Set a time and usage expiry login-PIN that can allow a single login per PIN. If an active login-PIN already exists. Calling this API again for same user will add another login-PIN 
        /// </summary>
        /// <param name="pinCode">Device Identifier</param>
        /// <param name="secret">Additional security parameter to validate the login</param>
        /// <param name="pinUsages">Optional number of pin usages</param>
        /// <param name="pinDuration">Optional duration in seconds of the pin</param>
        /// <remarks>Possible status codes: MissingSecurityParameter = 2007, LoginViaPinNotAllowed = 2009, PinNotInTheRightLength = 2010,PinExists = 2011, InvalidParameters = 7010
        /// </remarks>
        [Action("update")]
        [ApiAuthorize]
        [OldStandardArgument("pinCode", "pin_code")]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [Throws(eResponseStatus.MissingSecurityParameter)]
        [Throws(eResponseStatus.LoginViaPinNotAllowed)]
        [Throws(eResponseStatus.PinNotInTheRightLength)]
        [Throws(eResponseStatus.PinAlreadyExists)]
        [Throws(eResponseStatus.InvalidParameters)]
        static public KalturaUserLoginPin Update(string pinCode, string secret = null, int? pinUsages = null, int? pinDuration = null)
        {
            KalturaUserLoginPin res = null;
            int groupId = KS.GetFromRequest().GroupId;

            if (string.IsNullOrEmpty(pinCode))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "pinCode");
            }

            try
            {
                ClientsManager.UsersClient().ValidatePinEnhancements(pinUsages, pinDuration);
                // call client
                res = ClientsManager.UsersClient().SetLoginPin(groupId, KS.GetFromRequest().UserId, pinCode, secret, pinUsages, pinDuration);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return res;
        }

        /// <summary>
        /// Immediately expire all active login-PINs for a user
        /// </summary>
        /// <remarks></remarks>
        [Action("deleteAll")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        static public bool DeleteAll()
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
        /// <param name="pinCode">Login pin code to expire</param>
        /// <remarks></remarks>
        [Action("delete")]
        [ApiAuthorize]
        [OldStandardArgument("pinCode", "pin_code")]
        static public bool Delete(string pinCode)
        {
            bool res = false;
            int groupId = KS.GetFromRequest().GroupId;

            if (string.IsNullOrEmpty(pinCode))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "pinCode");
            }

            try
            {
                // call client
                res = ClientsManager.UsersClient().ClearLoginPIN(groupId, KS.GetFromRequest().UserId, pinCode);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return res;
        }
    }
}