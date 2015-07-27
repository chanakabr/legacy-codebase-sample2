using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.ModelBinding;
using System.Web.Routing;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
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
    [RoutePrefix("user")]
    public class UserController : ApiController
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
        public KalturaLoginPin GenerateLoginPin([FromUri] string partner_id, [FromUri] string user_id, [FromUri] string secret = null)
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

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("{user_id}/pin"), HttpGet]
        public KalturaLoginPin GetGenerateLoginPin(string partner_id, string user_id, string secret = null)
        {
            return GenerateLoginPin(partner_id, user_id, secret);
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
        [Route("login/pin"), HttpPost]
        public KalturaUser LogInWithPin([FromUri] string partner_id, [FromUri] string pin, [FromUri] string udid = null, [FromUri] string secret = null)
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

            return response;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("login/pin"), HttpGet]
        public KalturaUser GetLogInWithPin(string partner_id, string pin, string udid, string secret = null)
        {
            return LogInWithPin(partner_id, pin, udid, secret);
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
        public void SetLoginPin([FromUri] string partner_id, [FromUri] string user_id, [FromUri] string pin, [FromUri] string secret = null)
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
        public void ClearLoginPin([FromUri] string partner_id, [FromUri] string user_id)
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
        [Route("Login"), HttpPost]
        public KalturaUser Login([FromUri] string partner_id, [FromBody] KalturaLogIn request, [FromUri] string udid = null)
        {
            KalturaUser response = null;

            int groupId = int.Parse(partner_id);

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
                response = ClientsManager.UsersClient().Login(groupId, request.Username, request.Password, udid, request.ExtraParams);
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
        /// Sign up a new user.      
        /// </summary>        
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="request">SignUp Object</param>
        /// <remarks>Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// UserNotInHousehold = 1005, Wrong username or password = 1011, User suspended = 2001, InsideLockTime = 2015, UserNotActivated = 2016, 
        /// UserAllreadyLoggedIn = 2017,UserDoubleLogIn = 2018, DeviceNotRegistered = 2019, ErrorOnInitUser = 2021,UserNotMasterApproved = 2023, User does not exist = 2000
        /// </remarks>
        [Route(""), HttpPost]
        public KalturaUser SignUp([FromUri] string partner_id, [FromBody] KalturaSignUp request)
        {
            KalturaUser response = null;

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
                response = ClientsManager.UsersClient().SignUp(int.Parse(partner_id), request.userBasicData, request.userDynamicData, request.password, request.affiliateCode);
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
        [Route("token/{token}"), HttpGet]
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
        [Route("{username}/password"), HttpPut]
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
        public KalturaUsersList GetUsersData([FromUri] string partner_id, string user_id)
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
        [Route("{user_id}"), HttpPut]
        public KalturaUser SetUserData([FromUri] string partner_id, string user_id, KalturaUserData user_data)
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
        /// Return the parental rules that applies to the user. Can include rules that have been associated in account, household, or user level.        
        /// </summary>
        /// <remarks>
        /// Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// User does not exist = 2000, User with no household = 2024, User suspended = 2001
        /// </remarks>
        /// <param name="user_id">User Identifier</param>
        /// <param name="partner_id">Partner identifier</param>
        /// <returns>List of parental rules applied to the user</returns>
        [Route("{user_id}/parental/rules"), HttpGet]
        public KalturaParentalRulesList GetParentalRules([FromUri] string partner_id, [FromUri] string user_id)
        {
            List<KalturaParentalRule> response = null;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                response = ClientsManager.ApiClient().GetUserParentalRules(groupId, user_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaParentalRulesList() { ParentalRules = response };
        }

        /// <summary>
        /// Enabled a parental rule for a specific user.        
        /// </summary>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// User does not exist = 2000, User with no household = 2024, User suspended = 2001, Invalid rule = 5003</remarks>
        /// <param name="user_id">User Identifier</param>
        /// <param name="rule_id">Rule Identifier</param>
        /// <param name="partner_id">Partner identifier</param>
        /// <returns>Success or failure and reason</returns>
        [Route("{user_id}/parental/rules/{rule_id}"), HttpPost]
        public bool EnableParentalRule([FromUri] string partner_id, [FromUri] string user_id, [FromUri] long rule_id)
        {
            bool success = false;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                success = ClientsManager.ApiClient().SetUserParentalRule(groupId, user_id, rule_id, 1);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return success;
        }

        /// <summary>
        /// Disables a parental rule for a specific user.        
        /// </summary>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// User does not exist = 2000, User with no household = 2024, User suspended = 2001, Invalid rule = 5003</remarks>
        /// <param name="user_id">User Identifier</param>
        /// <param name="rule_id">Rule Identifier</param>
        /// <param name="partner_id">Partner identifier</param>
        /// <returns>Success or failure and reason</returns>
        [Route("{user_id}/parental/rules/{rule_id}"), HttpDelete]
        public bool DisableParentalRule([FromUri] string partner_id, [FromUri] string user_id, [FromUri] long rule_id)
        {
            bool success = false;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                success = ClientsManager.ApiClient().SetUserParentalRule(groupId, user_id, rule_id, 0);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return success;
        }

        /// <summary>
        /// Retrieve the parental PIN that applies for the user.        
        /// </summary>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// User does not exist = 2000, User with no household = 2024, User suspended = 2001</remarks>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="user_id">User identifier</param>
        /// <returns>The PIN that applies for the user</returns>
        [Route("{user_id}/parental/pin"), HttpGet]
        public KalturaPinResponse GetParentalPIN([FromUri] string partner_id, [FromUri] string user_id)
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
        [Route("{user_id}/parental/pin"), HttpPost]
        public bool SetParentalPIN([FromUri] string partner_id, [FromUri] string user_id, [FromUri] string pin)
        {
            bool success = false;

            try
            {
                // call client
                success = ClientsManager.ApiClient().SetUserParentalPIN(int.Parse(partner_id), user_id, pin);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return success;
        }

        /// <summary>
        /// Retrieve the purchase settings that applies for the user.        
        /// </summary>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// User does not exist = 2000, User with no household = 2024, User suspended = 2001</remarks>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="user_id">User identifier</param>
        /// <returns>The PIN that applies for the user</returns>
        [Route("{user_id}/purchase/settings"), HttpGet]
        public KalturaPurchaseSettingsResponse GetPurchaseSettings([FromUri] string partner_id, [FromUri] string user_id)
        {
            KalturaPurchaseSettingsResponse purchaseResponse = null;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                purchaseResponse = ClientsManager.ApiClient().GetUserPurchaseSettings(groupId, user_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return purchaseResponse;
        }

        /// <summary>
        /// Set the purchase settings that applies for the user.        
        /// </summary>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// User does not exist = 2000, User with no household = 2024, User suspended = 2001</remarks>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="user_id">User identifier</param>
        /// <param name="setting">New settings to apply</param>
        /// <returns>Success / Fail</returns>
        [Route("{user_id}/purchase/settings"), HttpPost]
        public bool SetPurchaseSettings([FromUri] string partner_id, [FromUri] string user_id, [FromUri] int setting)
        {
            bool success = false;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                success = ClientsManager.ApiClient().SetUserPurchaseSettings(groupId, user_id, setting);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return success;
        }

        /// <summary>
        /// Retrieve the purchase PIN that applies for the user.        
        /// </summary>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// 5001 = No PIN defined, User does not exist = 2000, User with no household = 2024, User suspended = 2001</remarks>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="user_id">User identifier</param>
        /// <returns>The PIN that applies for the user</returns>
        [Route("{user_id}/purchase/pin"), HttpGet]
        public KalturaPurchaseSettingsResponse GetPurchasePIN([FromUri] string partner_id, [FromUri] string user_id)
        {
            KalturaPurchaseSettingsResponse pinResponse = null;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                pinResponse = ClientsManager.ApiClient().GetUserPurchasePIN(groupId, user_id);
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
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="user_id">User identifier</param>
        /// <param name="pin">New PIN to apply</param>
        /// <returns>Success / Fail</returns>
        [Route("{user_id}/purchase/pin"), HttpPost]
        public bool SetPurchasePIN([FromUri] string partner_id, [FromUri] string user_id, [FromUri] string pin)
        {
            bool success = false;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                success = ClientsManager.ApiClient().SetUserPurchasePIN(groupId, user_id, pin);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return success;
        }

        /// <summary>
        /// Retrieve all the parental rules that applies for a specific media and a specific user according to the user parental settings.        
        /// </summary>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// User does not exist = 2000, User with no household = 2024, User suspended = 2001</remarks>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="user_id">User identifier</param>
        /// <param name="media_id">Media identifier</param>
        /// <returns>All the parental rules that applies for a specific media and a specific user according to the user parental settings.</returns>
        [Route("{user_id}/parental/rules/media/{media_id}"), HttpGet]
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
        [Route("{user_id}/parental/rules/epg/{epg_id}"), HttpGet]
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
        /// Validate that a given parental PIN for a user is valid.        
        /// </summary>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// No PIN defined = 5001, PIN mismatch = 5002, User does not exist = 2000, User with no household = 2024, User suspended = 2001</remarks>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="user_id">User identifier</param>
        /// <param name="pin">PIN to validate</param>
        /// <returns>Success / fail</returns>
        [Route("{user_id}/parental/pin/validate"), HttpPost]
        public bool ValidateParentalPIN([FromUri] string partner_id, [FromUri] string user_id, [FromUri] string pin)
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

        /// <summary>
        /// Validate that a given purchase PIN for a user is valid.         
        /// </summary>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// No PIN defined = 5001, PIN mismatch = 5002, User does not exist = 2000, User with no household = 2024, User suspended = 2001</remarks>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="user_id">User identifier</param>
        /// <param name="pin">PIN to validate</param>
        /// <returns>Success / fail</returns>
        [Route("{user_id}/purchase/pin/validate"), HttpPost]
        public bool ValidatePurchasePIN([FromUri] string partner_id, [FromUri] string user_id, [FromUri] string pin)
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
                success = ClientsManager.ApiClient().ValidatePurchasePIN(groupId, user_id, pin);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return success;
        }

        /// <summary>
        /// Disables the partner's default rule for this user        
        /// </summary>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// User does not exist = 2000, User with no household = 2024, User suspended = 2001</remarks>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="user_id">User identifier</param>
        /// <returns>Success / fail</returns>
        [Route("{user_id}/parental/rules/default"), HttpDelete]
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
        [Route("{user_id}/rules/media/{media_id}"), HttpGet]
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
        [Route("{user_id}/rules/epg/{epg_id}"), HttpGet]
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

        #region ConditionalAccess

        /// <summary>
        /// Gets user transaction history.        
        /// </summary>        
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="user_id">User Id</param>
        /// <param name="page_number">page number</param>
        /// <param name="page_size">page size</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008</remarks>
        [Route("{user_id}/transactions"), HttpGet]
        public KalturaBillingTransactions GetUserTransactionHistory([FromUri] string partner_id, [FromUri] string user_id, [FromUri] int page_number, [FromUri] int page_size)
        {
            KalturaBillingTransactions response = new KalturaBillingTransactions();

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().GetUserTransactionHistory(groupId, user_id, page_number, page_size);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
        
        #endregion

    }
}
