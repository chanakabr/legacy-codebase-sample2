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
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Models.API;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.Models.Users;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [RoutePrefix("_service/OTTUser/action")]
    public class OTTUserController : ApiController
    {
        /// <summary>
        /// Returns tokens (KS and refresh token) for anonymous access
        /// </summary>
        /// <param name="partnerId">The partner ID</param>
        /// <param name="udid">The caller device's UDID</param>
        /// <returns>KalturaLoginResponse</returns>
        [Route("anonymousLogin"), HttpPost]
        public KalturaLoginSession AnonymousLogin(int partnerId, string udid = null)
        {
            return AuthorizationManager.GenerateSession("0", partnerId, false, false, udid);
        }

        /// <summary>
        /// User sign-in via a time-expired sign-in PIN.        
        /// </summary>
        /// <param name="partnerId">Partner Identifier</param>
        /// <param name="pin">pin code</param>
        /// <param name="secret">Additional security parameter to validate the login</param>
        /// <param name="udid">Device UDID</param>
        /// <remarks>Possible status codes: 
        /// UserNotInHousehold = 1005, Wrong username or password = 1011, PinNotExists = 2003, PinExpired = 2004, NoValidPin = 2006, SecretIsWrong = 2008, 
        /// LoginViaPinNotAllowed = 2009, User suspended = 2001, InsideLockTime = 2015, UserNotActivated = 2016, 
        /// UserAllreadyLoggedIn = 2017,UserDoubleLogIn = 2018, DeviceNotRegistered = 2019, ErrorOnInitUser = 2021,UserNotMasterApproved = 2023, UserWIthNoHousehold = 2024, User does not exist = 2000
        /// </remarks>
        [Route("loginWithPin"), HttpPost]
        public KalturaLoginResponse LoginWithPin(int partnerId, string pin, string udid = null, string secret = null)
        {
            KalturaOTTUser response = null;

            if (string.IsNullOrEmpty(pin))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "pin cannot be empty");
            }

            try
            {
                // call client
                response = ClientsManager.UsersClient().LoginWithPin(partnerId, udid, pin, secret);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaLoginResponse() { LoginSession = AuthorizationManager.GenerateSession(response.Id.ToString(), partnerId, false, true, udid), User = response };
        }

        /// <summary>
        /// login with user name and password.
        /// </summary>        
        /// <param name="partnerId">Partner identifier</param>
        /// <param name="username">user name</param>
        /// <param name="password">password</param>
        /// <param name="extra_params">extra params</param>
        /// <param name="udid">Device UDID</param>
        /// <remarks>        
        /// UserNotInHousehold = 1005, Wrong username or password = 1011, User suspended = 2001, InsideLockTime = 2015, UserNotActivated = 2016, 
        /// UserAllreadyLoggedIn = 2017,UserDoubleLogIn = 2018, DeviceNotRegistered = 2019, ErrorOnInitUser = 2021,UserNotMasterApproved = 2023, User does not exist = 2000
        /// </remarks>
        [Route("login"), HttpPost]
        public KalturaLoginResponse Login(int partnerId, string username, string password, SerializableDictionary<string, KalturaStringValue> extra_params = null,
            string udid = null)
        {
            KalturaOTTUser response = null;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "username or password empty");
            }
            try
            {
                // call client
                response = ClientsManager.UsersClient().Login(partnerId, username, password, udid, extra_params);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            if (response == null)
            {
                throw new InternalServerErrorException();
            }

            return new KalturaLoginResponse() { LoginSession = AuthorizationManager.GenerateSession(response.Id.ToString(), partnerId, false, false, udid), User = response };
        }

        /// <summary>
        /// Returns new Kaltura session (ks) for the user, using the supplied refresh_token (only if it's valid and not expired)
        /// </summary>
        /// <param name="refresh_token">Refresh token</param>
        /// <param name="udid">Device UDID</param>
        /// <returns></returns>
        [Route("refreshSession"), HttpPost]
        [ApiAuthorize(true, true)]
        public KalturaLoginSession RefreshSession(string refresh_token, string udid = null)
        {
            KalturaLoginSession response = null;

            if (string.IsNullOrEmpty(refresh_token))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "refresh_token cannot be empty");
            }
            try
            {
                // call client
                response = AuthorizationManager.RefreshSession(refresh_token, udid);
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
        /// login with facebook token.
        /// </summary>        
        /// <param name="partnerId">Partner identifier</param>
        /// <param name="token">Facebook token</param>
        /// <param name="udid">Device UDID</param>
        /// <remarks>        
        /// User does not exist = 2000
        /// </remarks>
        [Route("facebookLogin"), HttpPost]
        public KalturaLoginResponse FacebookLogin(int partnerId, string token, string udid = null)
        {
            KalturaOTTUser response = null;

            if (string.IsNullOrEmpty(token))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "token cannot be empty");
            }
            try
            {
                // call client
                response = ClientsManager.SocialClient().FBUserSignin(partnerId, token, udid);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            if (response == null)
            {
                throw new InternalServerErrorException();
            }

            return new KalturaLoginResponse() { LoginSession = AuthorizationManager.GenerateSession(response.Id.ToString(), partnerId, false, false, udid), User = response };
        }

        /// <summary>
        /// Sign up a new user.      
        /// </summary>        
        /// <param name="partnerId">Partner identifier</param>        
        /// <param name="password">password</param>
        /// <param name="user">The user model to add</param>
        /// <remarks>        
        /// Wrong username or password = 1011, User exists = 2014
        /// </remarks>
        [Route("add"), HttpPost]
        public KalturaOTTUser Add(int partnerId, KalturaOTTUser user, string password)
        {
            KalturaOTTUser response = null;

            if (user == null)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "user_date cannot be null");
            }

            if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(password))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "username and password cannot be empty");
            }
            try
            {
                response = ClientsManager.UsersClient().SignUp(partnerId, user, password);
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
        /// <param name="partnerId">Partner Identifier</param>
        /// <param name="username">user name</param>
        /// <remarks></remarks>
        [Route("sendPassword"), HttpPost]
        public bool sendPassword(int partnerId, string username)
        {
            bool response = false;

            if (string.IsNullOrEmpty(username))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "user name is empty");
            }
            try
            {
                // call client
                response = ClientsManager.UsersClient().SendNewPassword(partnerId, username);
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
        /// <param name="partnerId">Partner Identifier</param>
        /// <param name="username">user name</param>
        /// <param name="password">new password</param>
        /// <remarks>Possible status codes: User does not exist = 2000</remarks>
        [Route("resetPassword"), HttpPost]
        public bool resetPassword(int partnerId, string username, string password)
        {
            bool response = false;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "user name is empty");
            }
            try
            {
                // call client
                response = ClientsManager.UsersClient().RenewPassword(partnerId, username, password);
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
        /// Returns the user associated with a temporary reset token.
        /// </summary>        
        /// <param name="partnerId">Partner Identifier</param>
        /// <param name="token">token</param>
        /// <remarks>Possible status codes: 2000 = User does not exist</remarks>
        [Route("validateToken"), HttpPost]
        public KalturaOTTUser validateToken(int partnerId, string token)
        {
            KalturaOTTUser response = null;

            if (string.IsNullOrEmpty(token))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "token is empty");
            }
            try
            {
                // call client
                response = ClientsManager.UsersClient().CheckPasswordToken(partnerId, token);
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
        /// <param name="username">user name</param>
        /// <param name="old_password">old password</param>
        /// <param name="new_password">new password</param>
        /// <remarks>Possible status codes: Wrong username or password = 1011, User does not exist = 2000, Inside lock time = 2015, User already logged in = 2017</remarks>
        [Route("changePassword"), HttpPost]
        [ApiAuthorize]
        public bool ChangePassword(string username, string old_password, string new_password)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(old_password) || string.IsNullOrEmpty(new_password))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "user name or password is empty");
            }
            try
            {
                //TODO: get username by user id
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
        /// <param name="filter">Filter object to filter relevant users in the account</param>
        /// <remarks></remarks>
        /// <remarks></remarks>        
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaOTTUserListResponse List(KalturaOTTUserFilter filter)
        {
            List<KalturaOTTUser> response = null;

            List<string> usersIds;
            try
            {
                usersIds = filter.UserIDs.Select(x => x.value).Distinct().ToList();
            }
            catch
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "each user id must be int");
            }
            if (usersIds == null || usersIds.Count == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "no user id in list");
            }

            int groupId = KS.GetFromRequest().GroupId;

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

            return new KalturaOTTUserListResponse() { Users = response, TotalCount = response.Count };
        }

        /// <summary>Edit user details.        
        /// </summary>
        /// <param name="user"> UserData Object (include basic and dynamic data)</param>
        /// <remarks>         User suspended = 2001, User does not exist = 2000
        /// </remarks>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        public KalturaOTTUser Update(KalturaOTTUser user)
        {
            KalturaOTTUser response = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (user == null)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "user cannot be empty");
            }

            try
            {
                // call client
                response = ClientsManager.UsersClient().SetUserData(groupId, KS.GetFromRequest().UserId, user);
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
    }
}
