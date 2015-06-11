using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Net.Http;
using System.Web.Http.Description;
using System.Web.Routing;
using WebAPI.Exceptions;
using WebAPI.ClientManagers;
using WebAPI.ClientManagers.Client;
using WebAPI.Utils;
using WebAPI.Models.Users;
using WebAPI.Models.General;
using WebAPI.Models.Catalog;
using WebAPI.Models.API;

namespace WebAPI.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [RoutePrefix("users")]
    public class UsersController : ApiController
    {
        /// <summary>
        /// Generates a temporarily PIN that can allow a user to log-in.<br />
        /// Possible status codes: BadCredentials = 500000, InternalConnectionIssue = 500001, Timeout = 500002, BadRequest = 500003, UserNotExists = 2000, UserSuspended = 2001
        /// </summary>        
        /// <param name="group_id">Group Identifier</param>
        /// <param name="user_id">User Identifier</param>
        /// <param name="secret">Additional security parameter for optional enhanced security</param>
        /// <remarks></remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [Route("{user_id}/pin"), HttpPost]
        public LoginPin GenerateLoginPin([FromUri] string group_id, [FromUri] string user_id, [FromUri] string secret = null)
        {
            LoginPin response = null;

            // parameters validation
            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be an integer");
            }

            if (string.IsNullOrEmpty(user_id))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "user_id cannot be empty");
            }

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
        public LoginPin GetGenerateLoginPin(string group_id, string user_id, string secret = null)
        {
            return GenerateLoginPin(group_id, user_id, secret);
        }

        /// <summary>
        /// User sign-in via a time-expired sign-in PIN.
        /// Possible status codes: BadCredentials = 500000, InternalConnectionIssue = 500001, Timeout = 500002, BadRequest = 500003,
        /// UserNotInDomain = 1005, WrongPasswordOrUserName = 1011, PinNotExists = 2003, PinExpired = 2004, ValidPin = 2005, NoValidPin = 2006, SecretIsWrong = 2008, 
        /// LoginViaPinNotAllowed = 2009, UserSuspended = 2001, InsideLockTime = 2015, UserNotActivated = 2016, 
        /// UserAllreadyLoggedIn = 2017,UserDoubleLogIn = 2018, DeviceNotRegistered = 2019, ErrorOnInitUser = 2021,UserNotMasterApproved = 2023, UserWIthNoDomain = 2024, UserDoesNotExist = 2025
        /// </summary>
        /// <param name="group_id">Group Identifier</param>
        /// <param name="pin">pin code</param>
        /// <param name="user_id">User Identifier</param>
        /// <param name="secret">Additional security parameter to validate the login</param>
        /// <param name="device_id">Device Identifier</param>
        /// <remarks></remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [Route("login/pin"), HttpPost]
        public User LogInWithPin([FromUri] string group_id, [FromUri] string pin, [FromUri] string device_id = null, [FromUri] string secret = null)
        {
            User response = null;

            // parameters validation
            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be an integer");
            }

            if (string.IsNullOrEmpty(pin))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "pin cannot be empty");
            }

            try
            {
                // call client
                response = ClientsManager.UsersClient().LoginWithPin(groupId, device_id, pin, secret);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("login/pin"), HttpGet]
        public User GetLogInWithPin(string group_id, string pin, string device_id, string secret = null)
        {
            return LogInWithPin(group_id, pin, device_id, secret);
        }

        /// <summary>
        /// Set a temporarily PIN that can allow a user to log-in.
        /// Possible status codes: MissingSecurityParameter = 2007, LoginViaPinNotAllowed = 2009, PinNotInTheRightLength = 2010,PinExists = 2011,PinMustBeDigitsOnly = 2012, PinCanNotStartWithZero = 2013
        /// </summary>
        /// <param name="group_id">Group Identifier</param>
        /// <param name="user_id">User Identifier</param>
        /// <param name="pin">Device Identifier</param>
        /// <param name="secret">Additional security parameter to validate the login</param>
        /// <remarks></remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [Route("{user_id}/pin/{pin}"), HttpPost]
        public bool SetLoginPin([FromUri] string group_id, [FromUri] string user_id, [FromUri] string pin, [FromUri] string secret = null)
        {
            // parameters validation
            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be an integer");
            }

            if (string.IsNullOrEmpty(pin))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "pin cannot be empty");
            }

            if (string.IsNullOrEmpty(user_id))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "user_id cannot be empty");
            }

            try
            {
                // call client
                ClientsManager.UsersClient().SetLoginPin(groupId, user_id, pin, secret);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex, new List<int>() {2010, 2012, 2013});
            }

            return true;
        }

        /// <summary>
        /// Immediately expires a pre-set login-PIN.
        /// </summary>
        /// <param name="group_id">Group Identifier</param>
        /// <param name="user_id">User Identifier</param>
        /// <remarks></remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [Route("{user_id}/pin"), HttpDelete]
        public bool ClearLoginPin([FromUri] string group_id, [FromUri] string user_id)
        {
            // parameters validation
            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be an integer");
            }

            if (string.IsNullOrEmpty(user_id))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "user_id cannot be empty");
            }

            try
            {
                // call client
                ClientsManager.UsersClient().ClearLoginPIN(groupId, user_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return true;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("{user_id}/watch_history"), HttpPost]
        public WatchHistoryAssetWrapper PostWatchHistory(string group_id, string user_id, WatchHistory request, string language = null)
        {
            WatchHistoryAssetWrapper response = null;

            // parameters validation
            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be an integer");
            }

            // page size - 5 <= size <= 50
            if (request.page_size == null || request.page_size == 0)
            {
                request.page_size = 25;
            }
            else if (request.page_size > 50)
            {
                request.page_size = 50;
            }
            else if (request.page_size < 5)
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "page_size range can be between 5 and 50");
            }

            // days - default value 7
            if (request.days == 0)
                request.days = 7;
            try
            {
                // call client
                response = ClientsManager.CatalogClient().WatchHistory(groupId, user_id, language, request.page_index, request.page_size,
                                                                       request.filter_status, request.days, request.filter_types, request.with);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Get recently watched media for user, ordered by recently watched first.<br />
        /// Possible status codes: BadCredentials = 500000, InternalConnectionIssue = 500001, Timeout = 500002, BadRequest = 500003
        /// </summary>
        /// <param name="request">The search asset request parameter</param>
        /// <param name="group_id" >Group Identifier</param>
        /// <param name="user_id">User Identifier</param>
        /// <param name="language">Language Code</param>
        /// <remarks></remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [Route("{user_id}/watch_history"), HttpGet]
        public WatchHistoryAssetWrapper GetWatchHistory(string group_id, string user_id, [FromUri] WatchHistory request, string language = null)
        {
            return PostWatchHistory(group_id, user_id, request, language);
        }

        ///// <summary>
        ///// Retrieving users' data
        ///// </summary>
        ///// <param name="ids">Users IDs to retreive. Use ',' as a seperator between the IDs</param>
        ///// <remarks></remarks>
        ///// <returns>WebAPI.Models.User</returns>
        ///// <response code="200">OK</response>
        ///// <response code="400">Bad request</response>
        ///// <response code="500">Internal Server Error</response>
        //[Route("{ids}"), HttpGet]
        ////[ApiAuthorize()]
        //[ApiExplorerSettings(IgnoreApi = true)]
        //public List<ClientUser> GetUsersData(string ids)
        //{
        //    var c = new Users.UsersService();

        //    //XXX: Example of using the unmasking
        //    string[] unmaskedIds = null;
        //    try
        //    {
        //        unmaskedIds = ids.Split(',').Select(x => SerializationUtils.UnmaskSensitiveObject(x)).Distinct().ToArray();
        //    }
        //    catch
        //    {
        //        /*
        //         * We don't want to return 500 here, because if something went bad in the parameters, it means 400, but since
        //         * the model is valid (we can't really validate the unmasking thing on the model), we are doing it manually.
        //        */
        //        throw new BadRequestException();
        //    }

        //    var res = c.GetUsersData("users_215", "11111", unmaskedIds);
        //    List<ClientUser> dto = Mapper.Map<List<ClientUser>>(res);
        //    return dto;
        //}

        /// <summary>
        /// Create new user
        /// </summary>
        /// <param name="user">User details object</param>
        /// <remarks></remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [Route(""), HttpPost]
        [Authorize()]
        [ApiExplorerSettings(IgnoreApi = true)]
        public bool Post([FromBody]ClientUser user)
        {

            return true;
        }

        [Route("{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public void Put(int id, [FromBody]ClientUser value)
        {

        }

        [Route("{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public void Delete(int id)
        {

        }



        /// <summary>
        /// login with user name and password.<br />
        /// BadCredentials = 500000, InternalConnectionIssue = 500001, Timeout = 500002, BadRequest = 500003,
        /// UserNotInDomain = 1005, WrongPasswordOrUserName = 1011, UserSuspended = 2001, InsideLockTime = 2015, UserNotActivated = 2016, 
        /// UserAllreadyLoggedIn = 2017,UserDoubleLogIn = 2018, DeviceNotRegistered = 2019, ErrorOnInitUser = 2021,UserNotMasterApproved = 2023, UserDoesNotExist = 2025
        /// </summary>        
        /// <param name="group_id">Group ID</param>
        /// <param name="LogIn">LogIn Object</param>
        /// <param name="device_id">device identifier</param>
        /// <remarks></remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [Route("login"), HttpPost]
        public User Login([FromUri] string group_id, [FromBody] LogIn request, [FromUri] string device_id = null)
        {
            User response = null;

            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be int");
            }
            if (request == null)
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "SignIn is null");
            }
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "username or password empty");
            }
            try
            {
                // call client
                response = ClientsManager.UsersClient().Login(groupId, request.Username, request.Password, device_id, request.keyValues);
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
        /// SignUp (for new user).<br />
        /// BadCredentials = 500000, InternalConnectionIssue = 500001, Timeout = 500002, BadRequest = 500003,
        /// UserNotInDomain = 1005, WrongPasswordOrUserName = 1011, UserSuspended = 2001, InsideLockTime = 2015, UserNotActivated = 2016, 
        /// UserAllreadyLoggedIn = 2017,UserDoubleLogIn = 2018, DeviceNotRegistered = 2019, ErrorOnInitUser = 2021,UserNotMasterApproved = 2023, UserDoesNotExist = 2025
        /// </summary>        
        /// <param name="group_id">Group ID</param>
        /// <param name="sign_up">SignUp Object</param>
        /// <remarks></remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [Route("signup"), HttpPost]
        public User SignUp([FromUri] string group_id, [FromBody] SignUp sign_up)
        {
            User response = null;

            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be int");
            }
            if (sign_up == null || sign_up.userBasicData == null)
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "SignUp or UserBasicData is null");
            }
            if (string.IsNullOrEmpty(sign_up.userBasicData.Username) || string.IsNullOrEmpty(sign_up.password))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "username or password empty");
            }
            try
            {
                // call client
                response = ClientsManager.UsersClient().SignUp(groupId, sign_up.userBasicData, sign_up.userDynamicData, sign_up.password, sign_up.affiliateCode);
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
        /// SendNewPassword by user name.<br />
        /// Possible status codes: BadCredentials = 500000, InternalConnectionIssue = 500001, Timeout = 500002, BadRequest = 500003
        /// </summary>        
        /// <param name="group_id">Group ID</param>
        /// <param name="user_name">user name</param>
        /// <remarks></remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [Route("send_password"), HttpPost]
        public bool SendNewPassword([FromUri] string group_id, [FromUri] string user_name)
        {
            bool response = false;

            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be int");
            }
            if (string.IsNullOrEmpty(user_name))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "user name is empty");
            }          
            try
            {
                // call client
                response = ClientsManager.UsersClient().SendNewPassword(groupId, user_name);
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
        /// RenewPassword get user name and new password.<br />
        /// Possible status codes: BadCredentials = 500000, InternalConnectionIssue = 500001, Timeout = 500002, BadRequest = 500003, UserDoesNotExist = 2025, WrongPasswordOrUserName = 1011,
        /// </summary>        
        /// <param name="group_id">Group ID</param>
        /// <param name="user_name">user name</param>
        /// <param name="password">new password</param>
        /// <remarks></remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [Route("reset_password"), HttpPost]
        public bool RenewPassword([FromUri] string group_id, [FromUri] string user_name, [FromUri] string password)
        {
            bool response = false;

            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be int");
            }
            if (string.IsNullOrEmpty(user_name) || string.IsNullOrEmpty(password))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "user name is empty");
            }
            try
            {
                // call client
                response = ClientsManager.UsersClient().RenewPassword(groupId, user_name, password);
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
        /// CheckPasswordToken .<br />
        /// Possible status codes: BadCredentials = 500000, InternalConnectionIssue = 500001, Timeout = 500002, BadRequest = 500003
        /// </summary>        
        /// <param name="group_id">Group ID</param>
        /// <param name="token">token</param>
        /// <remarks></remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [Route("toke/{token}"), HttpGet]
        public User CheckPasswordToken([FromUri] string group_id, [FromUri] string token)
        {
            User response = null;

            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be int");
            }
            if (string.IsNullOrEmpty(token))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "token is empty");
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
        /// ChangeUserPassword chnage old password with new one for get user name.<br />
        /// Possible status codes: BadCredentials = 500000, InternalConnectionIssue = 500001, Timeout = 500002, BadRequest = 500003
        /// </summary>        
        /// <param name="group_id">Group ID</param>
        /// <param name="user_name">user name</param>
        /// <param name="old_password">old password</param>
        /// <param name="new_password">new password</param>
        /// <remarks></remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [Route("password"), HttpPut]
        public bool ChangeUserPassword([FromUri] string group_id, [FromUri] string user_name, [FromUri] string old_password, [FromUri] string new_password)
        {
            bool response = false;

            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be int");
            }
            if (string.IsNullOrEmpty(user_name) || string.IsNullOrEmpty(old_password) || string.IsNullOrEmpty(new_password))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "user name or password  is empty");
            }
            try
            {
                // call client
                response = ClientsManager.UsersClient().ChangeUserPassword(groupId, user_name, old_password, new_password);
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
        /// <param name="group_id">Group ID</param>
        /// <param name="ids">Users IDs to retreive. Use ',' as a seperator between the IDs</param>
        /// <remarks></remarks>
        /// <returns>List<WebAPI.Models.User></returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [Route("data/{ids}"), HttpGet]
        //[ApiAuthorize()]       
        public List<User> GetUsersData([FromUri] string group_id, string ids)
        {
            List<int> usersIds;
            try
            {
                usersIds = ids.Split(',').Select(x => int.Parse(x)).Distinct().ToList();
            }
            catch
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "each user id must be int");
            }

            List<User> response = null;
            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be int");
            }
            if (usersIds == null || usersIds.Count == 0)
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "no user id in list");
            }            
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
            return response;
          
        }


        /// <summary>SetUserData</summary>
        /// <param name="group_id">Group ID</param>
        /// <param name="user_data"> UserData Object (include basic and dynamic data)</param>
        /// <remarks></remarks>
        /// <returns>WebAPI.Models.User</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [Route(""), HttpPut]
        //[ApiAuthorize()]       
        public User SetUserData([FromUri] string group_id, UserData user_data)
        {           
            User response = null;
            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be int");
            }
            if (user_data == null || (user_data.userBasicData == null && (user_data.userDynamicData == null || user_data.userDynamicData.Count == 0)))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "no data to set");
            }
            if (string.IsNullOrEmpty(user_data.siteGuid))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "no siteGuid");
            }
            try
            {
                // call client
                response = ClientsManager.UsersClient().SetUserData(groupId, user_data.siteGuid ,user_data.userBasicData, user_data.userDynamicData);
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
        /// Return the parental rules that applies to the user. Can include rules that have been associated in account, domain, or user level.
        /// </summary>
        /// <param name="user_id">User Identifier</param>
        /// <param name="group_id">Partner identifier</param>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        /// <returns>List of parental rules applied to the user</returns>
        [Route("{user_id}/parental_rules"), HttpGet]
        public List<ParentalRule> GetParentalRules([FromUri] string group_id, [FromUri] string user_id)
        {
            List<ParentalRule> response = null;

            // parameters validation
            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be an integer");
            }

            if (string.IsNullOrEmpty(user_id))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "user_id cannot be empty");
            }

            try
            {
                // call client
                response = ClientsManager.ApiClient().GetUserParentalRules(groupId, user_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Enabled a parental rule for a specific user
        /// </summary>
        /// <param name="user_id">User Identifier</param>
        /// <param name="rule_id">Rule Identifier</param>
        /// <param name="group_id">Partner identifier</param>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        /// <returns>Success or failure and reason</returns>
        [Route("{user_id}/parental_rules/{rule_id}"), HttpPost]
        public bool EnableParentalRule([FromUri] string group_id, [FromUri] string user_id, [FromUri] long rule_id)
        {
            bool success = false;

            // parameters validation
            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be an integer");
            }

            if (string.IsNullOrEmpty(user_id))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "user_id cannot be empty");
            }

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
        /// Disables a parental rule for a specific user
        /// </summary>
        /// <param name="user_id">User Identifier</param>
        /// <param name="rule_id">Rule Identifier</param>
        /// <param name="group_id">Partner identifier</param>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        /// <returns>Success or failure and reason</returns>
        [Route("{user_id}/parental_rules/{rule_id}"), HttpDelete]
        public bool DisableParentalRule([FromUri] string group_id, [FromUri] string user_id, [FromUri] long rule_id)
        {
            bool success = false;

            // parameters validation
            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be an integer");
            }

            if (string.IsNullOrEmpty(user_id))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "user_id cannot be empty");
            }

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
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        /// <param name="group_id">Partner Identifier</param>
        /// <param name="user_id">User identifier</param>
        /// <returns>The PIN that applies for the user</returns>
        [Route("{user_id}/parental_pin/"), HttpGet]
        public PinResponse GetParentalPIN([FromUri] string group_id, [FromUri] string user_id)
        {
            PinResponse pinResponse = null;

            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be an integer");
            }

            if (string.IsNullOrEmpty(user_id))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "user_id cannot be empty");
            }

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
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        /// <param name="group_id">Partner Identifier</param>
        /// <param name="user_id">User identifier</param>
        /// <param name="pin">New PIN to set</param>
        /// <returns>Success / Fail</returns>
        [Route("{user_id}/parental_pin/"), HttpPost]
        public bool SetParentalPIN([FromUri] string group_id, [FromUri] string user_id, string pin)
        {
            bool success = false;

            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be an integer");
            }

            if (string.IsNullOrEmpty(user_id))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "user_id cannot be empty");
            }

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
        /// Retrieve the purchase settings that applies for the user.
        /// Possible status codes:
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        /// <param name="group_id">Partner Identifier</param>
        /// <param name="user_id">User identifier</param>
        /// <returns>The PIN that applies for the user</returns>
        [Route("{user_id}/purchase_settings/"), HttpGet]
        public PurchaseSettingsResponse GetPurchaseSettings([FromUri] string group_id, [FromUri] string user_id)
        {
            PurchaseSettingsResponse purchaseResponse = null;

            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be an integer");
            }

            if (string.IsNullOrEmpty(user_id))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "user_id cannot be empty");
            }

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
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        /// <param name="group_id">Partner Identifier</param>
        /// <param name="user_id">User identifier</param>
        /// <param name="setting">New settings to apply</param>
        /// <returns>Success / Fail</returns>
        [Route("{user_id}/purchase_setting/"), HttpPost]
        public bool SetPurchaseSettings([FromUri] string group_id, [FromUri] string user_id, int setting)
        {
            bool success = false;

            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be an integer");
            }

            if (string.IsNullOrEmpty(user_id))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "user_id cannot be empty");
            }

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
        /// Possible status codes: 5001 = No PIN defined
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        /// <param name="group_id">Partner Identifier</param>
        /// <param name="user_id">User identifier</param>
        /// <returns>The PIN that applies for the user</returns>
        [Route("{user_id}/purchase_pin/"), HttpGet]
        public PurchaseSettingsResponse GetPurchasePIN([FromUri] string group_id, [FromUri] string user_id)
        {
            PurchaseSettingsResponse pinResponse = null;

            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be an integer");
            }

            if (string.IsNullOrEmpty(user_id))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "user_id cannot be empty");
            }

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
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        /// <param name="group_id">Partner Identifier</param>
        /// <param name="user_id">User identifier</param>
        /// <param name="pin">New PIN to apply</param>
        /// <returns>Success / Fail</returns>
        [Route("{user_id}/purchase_pin/"), HttpPost]
        public bool SetPurchasePIN([FromUri] string group_id, [FromUri] string user_id, string pin)
        {
            bool success = false;

            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be an integer");
            }

            if (string.IsNullOrEmpty(user_id))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "user_id cannot be empty");
            }

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
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        /// <param name="group_id">Partner Identifier</param>
        /// <param name="user_id">User identifier</param>
        /// <param name="media_id">Media identifier</param>
        /// <returns>All the parental rules that applies for a specific media and a specific user according to the user parental settings.</returns>
        [Route("{user_id}/parental_rules/media/{media_id}"), HttpGet]
        public List<ParentalRule> GetParentalMediaRules([FromUri] string group_id, [FromUri] string user_id, [FromUri] long media_id)
        {
            List<ParentalRule> response = null;

            // parameters validation
            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be an integer");
            }

            if (string.IsNullOrEmpty(user_id))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "user_id cannot be empty");
            }

            if (media_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "media_id cannot be empty");
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

            return response;
        }

        /// <summary>
        /// Retrieve all the parental rules that applies for a specific EPG and a specific user according to the user parental settings.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        /// <param name="group_id">Partner Identifier</param>
        /// <param name="user_id">User identifier</param>
        /// <param name="epg_id">EPG identifier</param>
        /// <returns>All the parental rules that applies for a specific EPG and a specific user according to the user parental settings.</returns>
        [Route("{user_id}/parental_rules/epg/{epg_id}"), HttpGet]
        public List<ParentalRule> GetParentalEPGRules([FromUri] string group_id, [FromUri] string user_id, [FromUri] long epg_id)
        {
            List<ParentalRule> response = null;

            // parameters validation
            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be an integer");
            }

            if (string.IsNullOrEmpty(user_id))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "user_id cannot be empty");
            }
            if (epg_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "epg_id cannot be empty");
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

            return response;
        }

        /// <summary>
        /// Validate that a given parental PIN for a user is valid.
        /// Possible status codes: 5001 = No PIN defined, 5002 = PIN mismatch
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        /// <param name="group_id">Partner Identifier</param>
        /// <param name="user_id">User identifier</param>
        /// <param name="pin">PIN to validate</param>
        /// <returns>Success / fail</returns>
        [Route("{user_id}/parental_pin/{pin}"), HttpGet]
        public bool ValidateParentalPIN([FromUri] string group_id, [FromUri] string user_id, [FromUri] string pin)
        {
            bool success = false;

            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be an integer");
            }

            if (string.IsNullOrEmpty(user_id))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "user_id cannot be empty");
            }

            if (string.IsNullOrEmpty(pin))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "pin cannot be empty");
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
        /// Possible status codes: 5001 = No PIN defined, 5002 = PIN mismatch
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        /// <param name="group_id">Partner Identifier</param>
        /// <param name="user_id">User identifier</param>
        /// <param name="pin">PIN to validate</param>
        /// <returns>Success / fail</returns>
        [Route("{user_id}/purchase_pin/{pin}"), HttpGet]
        public bool ValidatePurchasePIN([FromUri] string group_id, [FromUri] string user_id, [FromUri] string pin)
        {
            bool success = false;

            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be an integer");
            }

            if (string.IsNullOrEmpty(user_id))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "user_id cannot be empty");
            }

            if (string.IsNullOrEmpty(pin))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "pin cannot be empty");
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

        #endregion

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="request">Credentials</param>
        ///// <param name="group_id">Group ID</param>
        //[Route("sign_in"), HttpPost]
        //[ApiExplorerSettings(IgnoreApi = true)]
        //public string SignIn([FromUri] string group_id, [FromBody] SignIn request)
        //{
        //    //TODO: add parameters

        //    // TODO: change to something later
        //    string data = string.Empty;

        //    int groupId;
        //    if (!int.TryParse(group_id, out groupId))
        //    {
        //        throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be int");
        //    }

        //    WebAPI.Models.Users.ClientUser user = ClientsManager.UsersClient().SignIn(groupId, request.Username, request.Password);

        //    if (user == null)
        //    {
        //        throw new InternalServerErrorException();
        //    }

        //    string userSecret = GroupsManager.GetGroup(groupId).UserSecret;

        //    //TODO: get real value
        //    int expiration = 1462543601;

        //    return new KS(userSecret, group_id, user.ID, expiration, KS.eUserType.USER, data, string.Empty).ToString();
        //}
    }
}
