using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.ModelBinding;
using System.Web.Routing;
using WebAPI.ClientManagers;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.API;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.Users;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [RoutePrefix("_service/user/action")]
    public class UserController : ApiController
    {
        /// <summary>
        /// Returns tokens (KS and refresh token) for anonymous access
        /// </summary>
        /// <param name="partner_id">The partner ID</param>
        /// <param name="udid">The caller device's UDID</param>
        /// <returns>KalturaLoginResponse</returns>
        [Route("anonymousLogin"), HttpPost]
        public KalturaLoginResponse AnonymousLogin(string partner_id, string udid = null)
        {
            int partnerID = int.Parse(partner_id);

            string userSecret = GroupsManager.GetGroup(partnerID).UserSecret;
            var l = new List<KeyValuePair<string, string>>();
            l.Add(new KeyValuePair<string, string>(KS.PAYLOAD_UDID, udid));
            string payload = KS.preparePayloadData(l);

            KS ks = new KS(userSecret, partner_id, "0", Int32.MaxValue, KS.eUserType.USER, payload, string.Empty);

            return new KalturaLoginResponse() { KS = ks.ToString(), RefreshToken = Guid.NewGuid().ToString(), User = null };
        }

        /// <summary>
        /// User sign-in via a time-expired sign-in PIN.        
        /// </summary>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="pin">pin code</param>
        /// <param name="secret">Additional security parameter to validate the login</param>
        /// <param name="udid">Device UDID</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// UserNotInHousehold = 1005, Wrong username or password = 1011, PinNotExists = 2003, PinExpired = 2004, NoValidPin = 2006, SecretIsWrong = 2008, 
        /// LoginViaPinNotAllowed = 2009, User suspended = 2001, InsideLockTime = 2015, UserNotActivated = 2016, 
        /// UserAllreadyLoggedIn = 2017,UserDoubleLogIn = 2018, DeviceNotRegistered = 2019, ErrorOnInitUser = 2021,UserNotMasterApproved = 2023, UserWIthNoHousehold = 2024, User does not exist = 2000
        /// </remarks>
        [Route("login_with_pin"), HttpPost]
        public KalturaLoginResponse LogInWithPin([FromUri] string partner_id, [FromUri] string pin, [FromUri] string udid = null, [FromUri] string secret = null)
        {
            KalturaUser response = null;

            int groupId = int.Parse(partner_id);

            if (string.IsNullOrEmpty(pin))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "pin cannot be empty");
            }

            try
            {
                // call client
                response = ClientsManager.UsersClient().LoginWithPin(groupId, udid, pin, secret);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            string userSecret = GroupsManager.GetGroup(groupId).UserSecret;
            var l = new List<KeyValuePair<string, string>>();
            l.Add(new KeyValuePair<string, string>(KS.PAYLOAD_UDID, udid));
            string payload = KS.preparePayloadData(l);
            KS ks = new KS(userSecret, partner_id, response.Id.ToString(), 32982398, KS.eUserType.USER, payload, string.Empty);

            return new KalturaLoginResponse() { KS = ks.ToString(), RefreshToken = Guid.NewGuid().ToString(), User = response };
        }

        /// <summary>
        /// login with user name and password.
        /// </summary>        
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="request">User details parameters</param>
        /// <param name="udid">Device UDID</param>
        /// <remarks>Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// UserNotInHousehold = 1005, Wrong username or password = 1011, User suspended = 2001, InsideLockTime = 2015, UserNotActivated = 2016, 
        /// UserAllreadyLoggedIn = 2017,UserDoubleLogIn = 2018, DeviceNotRegistered = 2019, ErrorOnInitUser = 2021,UserNotMasterApproved = 2023, User does not exist = 2000
        /// </remarks>
        [Route("login"), HttpPost]
        public KalturaLoginResponse Login([FromUri] string partner_id, [FromBody] KalturaLogIn request, [FromUri] string udid = null)
        {
            KalturaUser response = null;

            int partnerID = int.Parse(partner_id);

            if (request == null)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Login details are null");
            }
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "username or password empty");
            }
            try
            {
                // call client
                response = ClientsManager.UsersClient().Login(partnerID, request.Username, request.Password, udid, request.ExtraParams);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            if (response == null)
            {
                throw new InternalServerErrorException();
            }

            string userSecret = GroupsManager.GetGroup(partnerID).UserSecret;
            var l = new List<KeyValuePair<string, string>>();
            l.Add(new KeyValuePair<string, string>(KS.PAYLOAD_UDID, udid));
            string payload = KS.preparePayloadData(l);
            KS ks = new KS(userSecret, partner_id, response.Id.ToString(), 32982398, KS.eUserType.USER, payload, string.Empty);

            return new KalturaLoginResponse() { KS = ks.ToString(), RefreshToken = Guid.NewGuid().ToString(), User = response };
        }

        /// <summary>
        /// Sign up a new user.      
        /// </summary>        
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="request">SignUp Object</param>
        /// <remarks>Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// UserNotInHousehold = 1005, Wrong username or password = 1011, User suspended = 2001, InsideLockTime = 2015, UserNotActivated = 2016, 
        /// UserAllreadyLoggedIn = 2017,UserDoubleLogIn = 2018, DeviceNotRegistered = 2019, ErrorOnInitUser = 2021,UserNotMasterApproved = 2023, User does not exist = 2000
        /// </remarks>
        [Route("add"), HttpPost]
        public KalturaUser Add([FromUri] string partner_id, [FromBody] KalturaSignUp request)
        {
            KalturaUser response = null;
            
            int groupId = int.Parse(partner_id);

            if (request == null || request.userBasicData == null)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "SignUp or UserBasicData is null");
            }
            if (string.IsNullOrEmpty(request.userBasicData.Username) || string.IsNullOrEmpty(request.password))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "username or password empty");
            }
            try
            {
                // call client
                response = ClientsManager.UsersClient().SignUp(groupId, request.userBasicData, request.userDynamicData, request.password, request.affiliateCode);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            if (response == null)
            {
                throw new InternalServerErrorException();
            }
            return response;
        }

        /// <summary>
        /// Send a new password by user name.        
        /// </summary>        
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="username">user name</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008</remarks>
        [Route("{username}/password/send"), HttpPost]
        public bool SendNewPassword([FromUri] string partner_id, [FromUri] string username)
        {
            bool response = false;

            int groupId = int.Parse(partner_id);

            if (string.IsNullOrEmpty(username))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "user name is empty");
            }
            try
            {
                // call client
                response = ClientsManager.UsersClient().SendNewPassword(groupId, username);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            if (response == false)
            {
                throw new InternalServerErrorException();
            }

            return response;
        }

        /// <summary>
        /// Renew the user's password without validating the existing password.
        /// </summary>        
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="username">user name</param>
        /// <param name="password">new password</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, User does not exist = 2000, Wrong username or password = 1011</remarks>
        [Route("{username}/password/reset"), HttpPost]
        public bool RenewPassword([FromUri] string partner_id, [FromUri] string username, [FromUri] string password)
        {
            bool response = false;

            int groupId = int.Parse(partner_id);

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "user name is empty");
            }
            try
            {
                // call client
                response = ClientsManager.UsersClient().RenewPassword(groupId, username, password);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            if (response == false)
            {
                throw new InternalServerErrorException();
            }
            return response;
        }

        /// <summary>
        /// Returns the user name associated with a temporary reset token.        
        /// </summary>        
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="token">token</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008</remarks>
        [Route("token/{token}"), HttpPost]
        public KalturaUser CheckPasswordToken([FromUri] string partner_id, [FromUri] string token)
        {
            KalturaUser response = null;

            int groupId = int.Parse(partner_id);

            if (string.IsNullOrEmpty(token))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "token is empty");
            }
            try
            {
                // call client
                response = ClientsManager.UsersClient().CheckPasswordToken(groupId, token);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            if (response == null)
            {
                throw new InternalServerErrorException();
            }

            return response;
        }

        /// <summary>
        /// Given a user name and existing password, change to a new password.        
        /// </summary>        
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="username">user name</param>
        /// <param name="old_password">old password</param>
        /// <param name="new_password">new password</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008</remarks>
        [Route("{username}/password"), HttpPost]
        public bool ChangeUserPassword([FromUri] string partner_id, [FromUri] string username, [FromUri] string old_password, [FromUri] string new_password)
        {
            bool response = false;

            int groupId = int.Parse(partner_id);

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(old_password) || string.IsNullOrEmpty(new_password))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "user name or password is empty");
            }
            try
            {
                // call client
                response = ClientsManager.UsersClient().ChangeUserPassword(groupId, username, old_password, new_password);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            if (response == false)
            {
                throw new InternalServerErrorException();
            }
            return response;
        }

        /// <summary>
        /// Retrieving users' data
        /// </summary>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="user_id">Users IDs to retreive. Use ',' as a seperator between the IDs</param>
        /// <remarks></remarks>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008</remarks>
        [ApiAuthorize]
        [Route("get"), HttpPost]
        public KalturaUsersList Get([FromUri] string partner_id, string user_id)
        {
            List<KalturaUser> response = null;

            List<int> usersIds;
            try
            {
                usersIds = user_id.Split(',').Select(x => int.Parse(x)).Distinct().ToList();
            }
            catch
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "each user id must be int");
            }
            if (usersIds == null || usersIds.Count == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "no user id in list");
            }
            
            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                response = ClientsManager.UsersClient().GetUsersData(groupId, usersIds);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            if (response == null)
            {
                throw new InternalServerErrorException();
            }

            return new KalturaUsersList() { Users = response };
        }

        /// <summary>Edit user details.        
        /// </summary>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="user_data"> UserData Object (include basic and dynamic data)</param>
        /// <param name="user_id"> User identifiers</param>
        /// <remarks>Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, User suspended = 2001, User does not exist = 2000
        /// </remarks>
        [Route("update"), HttpPost]
        public KalturaUser Update([FromUri] string partner_id, string user_id, KalturaUserData user_data)
        {
            KalturaUser response = null;
            
            int groupId = int.Parse(partner_id);

            if (user_data == null || (user_data.userBasicData == null && (user_data.userDynamicData == null || user_data.userDynamicData.Count == 0)))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "no data to set");
            }

            try
            {
                // call client
                response = ClientsManager.UsersClient().SetUserData(groupId, user_id, user_data.userBasicData, user_data.userDynamicData);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            if (response == null)
            {
                throw new InternalServerErrorException();
            }
            return response;

        }

        #region Parental Rules

        /// <summary>
        /// Retrieve all the parental rules that applies for a specific media and a specific user according to the user parental settings.        
        /// </summary>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// User does not exist = 2000, User with no household = 2024, User suspended = 2001</remarks>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="user_id">User identifier</param>
        /// <param name="media_id">Media identifier</param>
        /// <returns>All the parental rules that applies for a specific media and a specific user according to the user parental settings.</returns>
        [Route("{user_id}/parental/rules/media/{media_id}"), HttpPost]
        public KalturaParentalRulesList GetParentalMediaRules([FromUri] string partner_id, [FromUri] string user_id, [FromUri] long media_id)
        {
            List<KalturaParentalRule> response = null;

            
            
            int groupId = int.Parse(partner_id);
                

            // parameters validation
            if (media_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "media_id cannot be empty");
            }
            try
            {
                // call client
                response = ClientsManager.ApiClient().GetUserMediaParentalRules(groupId, user_id, media_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaParentalRulesList() { ParentalRules = response };
        }

        /// <summary>
        /// Retrieve all the parental rules that applies for a specific EPG and a specific user according to the user parental settings.        
        /// </summary>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// User does not exist = 2000, User with no household = 2024, User suspended = 2001</remarks>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="user_id">User identifier</param>
        /// <param name="epg_id">EPG identifier</param>
        /// <returns>All the parental rules that applies for a specific EPG and a specific user according to the user parental settings.</returns>
        [Route("{user_id}/parental/rules/epg/{epg_id}"), HttpPost]
        public KalturaParentalRulesList GetParentalEPGRules([FromUri] string partner_id, [FromUri] string user_id, [FromUri] long epg_id)
        {
            List<KalturaParentalRule> response = null;

            
            
            int groupId = int.Parse(partner_id);
                

            // parameters validation
            if (epg_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "epg_id cannot be empty");
            }

            try
            {
                // call client
                response = ClientsManager.ApiClient().GetUserEPGParentalRules(groupId, user_id, epg_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaParentalRulesList() { ParentalRules = response };
        }

        /// <summary>
        /// Disables the partner's default rule for this user        
        /// </summary>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// User does not exist = 2000, User with no household = 2024, User suspended = 2001</remarks>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="user_id">User identifier</param>
        /// <returns>Success / fail</returns>
        [Route("{user_id}/parental/rules/default"), HttpPost]
        public bool DisableDefaultParentalRule([FromUri] string partner_id, [FromUri] string user_id)
        {
            bool success = false;

            
            
            int groupId = int.Parse(partner_id);
                

            try
            {
                // call client
                success = ClientsManager.ApiClient().DisableUserDefaultParentalRule(groupId, user_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return success;
        }

        /// <summary>
        /// Retrieve all the rules (parental, geo, device or user-type) that applies for this user and media.        
        /// </summary>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// User does not exist = 2000, User with no household = 2024, User suspended = 2001, User not in household = 1005, Household does not exist = 1006</remarks>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="user_id">User identifier</param>
        /// <param name="media_id">Media identifier</param>
        /// <param name="household_id">Media identifier</param>
        /// <param name="udid">Device UDID</param>
        /// <returns>All the rules that applies for a specific media and a specific user according to the user parental and userType settings.</returns>
        [Route("{user_id}/rules/media/{media_id}"), HttpPost]
        public KalturaGenericRulesList GetMediaRules(string partner_id, string user_id, long media_id, string udid = null, int household_id = 0)
        {
            List<KalturaGenericRule> response = null;

            
            
            int groupId = int.Parse(partner_id);
                

            // parameters validation
            if (media_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "media_id cannot be empty");
            }
            try
            {
                // call client
                response = ClientsManager.ApiClient().GetMediaRules(groupId, user_id, media_id, household_id, udid);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaGenericRulesList() { GenericRules = response };
        }

        /// <summary>
        /// Retrieve all the rules (parental) that applies for this EPG program      
        /// </summary>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// User does not exist = 2000, User with no household = 2024, User suspended = 2001, User not in household = 1005, Household does not exist = 1006</remarks>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="user_id">User identifier</param>
        /// <param name="epg_id">EPG program identifier</param>
        /// <param name="household_id">Household identifier</param>        
        /// <param name="channel_media_id">Linear channel's media identifier</param>        
        /// <returns>All the rules that applies for a specific media and a specific user according to the user parental and userType settings.</returns>
        [Route("{user_id}/rules/epg/{epg_id}"), HttpPost]
        public KalturaGenericRulesList GetEpgRules(string partner_id, string user_id, [FromUri] long epg_id, long channel_media_id, int household_id = 0)
        {
            List<KalturaGenericRule> response = null;

            
            
            int groupId = int.Parse(partner_id);
                

            // parameters validation
            if (epg_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "epg_id cannot be empty");
            }
            try
            {
                // call client
                response = ClientsManager.ApiClient().GetEpgRules(groupId, user_id, epg_id, household_id, channel_media_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaGenericRulesList() { GenericRules = response };
        }

        #endregion
    }
}
