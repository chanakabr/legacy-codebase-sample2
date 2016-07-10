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
using WebAPI.Managers.Schema;
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
    [RoutePrefix("_service/ottUser/action")]
    [OldStandardAction("register", "add")]
    [OldStandardAction("updateLoginData", "changePassword")]
    [OldStandardAction("setPassword", "resetPassword")]
    [OldStandardAction("resetPassword", "sendPassword")]
    [OldStandardAction("getOldStandard", "get")]
    public class OttUserController : ApiController
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
        [ValidationException(SchemaValidationType.ACTION_NAME)]
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
        /// <param name="extraParams">extra params</param>
        /// <param name="udid">Device UDID</param>
        /// <remarks>        
        /// UserNotInHousehold = 1005, Wrong username or password = 1011, User suspended = 2001, InsideLockTime = 2015, UserNotActivated = 2016, 
        /// UserAllreadyLoggedIn = 2017,UserDoubleLogIn = 2018, DeviceNotRegistered = 2019, ErrorOnInitUser = 2021,UserNotMasterApproved = 2023, User does not exist = 2000
        /// </remarks>
        [Route("login"), HttpPost]
        [ValidationException(SchemaValidationType.ACTION_NAME)]
        [OldStandard("extraParams", "extra_params")]
        public KalturaLoginResponse Login(int partnerId, string username = null, string password = null, SerializableDictionary<string, KalturaStringValue> extraParams = null,
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
                // add header. if key exists use extraParams
                response = ClientsManager.UsersClient().Login(partnerId, username, password, udid, extraParams, System.Web.HttpContext.Current.Request.Headers);
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
        [ApiAuthorize(true)]
        [ValidationException(SchemaValidationType.ACTION_NAME)]
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
        /// Login via Facebook credentials
        /// </summary>        
        /// <param name="partnerId">Partner identifier</param>
        /// <param name="token">Facebook token</param>
        /// <param name="udid">Device UDID</param>
        /// <remarks>        
        /// User does not exist = 2000
        /// </remarks>
        [Route("facebookLogin"), HttpPost]
        [Obsolete]
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
        [Route("register"), HttpPost]
        [ValidationException(SchemaValidationType.ACTION_NAME)]
        public KalturaOTTUser Register(int partnerId, KalturaOTTUser user, string password)
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
        /// Send an e-mail with URL to enable the user to set new password.
        /// </summary>        
        /// <param name="partnerId">Partner Identifier</param>
        /// <param name="username">user name</param>
        /// <remarks></remarks>
        [Route("resetPassword"), HttpPost]
        [ValidationException(SchemaValidationType.ACTION_NAME)]
        public bool resetPassword(int partnerId, string username)
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
        /// Renew the user's password after validating the token that sent as part of URL in e-mail.
        /// </summary>        
        /// <param name="partnerId">Partner Identifier</param>
        /// <param name="token">Token that sent by e-mail</param>
        /// <param name="password">New password</param>
        /// <remarks>Possible status codes: User does not exist = 2000</remarks>
        [Route("setInitialPassword"), HttpPost]
        [ValidationException(SchemaValidationType.ACTION_NAME)]
        public KalturaOTTUser setInitialPassword(int partnerId, string token, string password)
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
                ClientsManager.UsersClient().RenewPassword(partnerId, response.Username, password);
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
        /// Renew the user's password without validating the existing password, used internally after validating token that sent as part of URL in e-mail.
        /// </summary>        
        /// <param name="partnerId">Partner Identifier</param>
        /// <param name="username">user name</param>
        /// <param name="password">new password</param>
        /// <remarks>Possible status codes: User does not exist = 2000</remarks>
        [Route("setPassword"), HttpPost]
        [Obsolete("Please use setInitialPassword instead")]
        public bool setPassword(int partnerId, string username, string password)
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
        [Obsolete("Please use setInitialPassword instead")]
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
        /// <param name="oldPassword">old password</param>
        /// <param name="newPassword">new password</param>
        /// <remarks>Possible status codes: Wrong username or password = 1011, User does not exist = 2000, Inside lock time = 2015, User already logged in = 2017</remarks>
        [Route("updateLoginData"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemaValidationType.ACTION_NAME)]
        [OldStandard("oldPassword", "old_password")]
        [OldStandard("newPassword", "new_password")]
        public bool UpdateLoginData(string username, string oldPassword, string newPassword)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "user name or password is empty");
            }
            try
            {
                //TODO: get username by user id
                // call client
                response = ClientsManager.UsersClient().ChangeUserPassword(groupId, username, oldPassword, newPassword);
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
        /// <remarks></remarks>
        /// <remarks></remarks>        
        [Route("get"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemaValidationType.ACTION_ARGUMENTS)]
        public KalturaOTTUser Get()
        {
            List<KalturaOTTUser> response = null;

            string userId = KS.GetFromRequest().UserId;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.UsersClient().GetUsersData(groupId, new List<string>() { userId });
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            if (response == null)
            {
                throw new InternalServerErrorException();
            }

            return response.First();
        }

        /// <summary>
        /// Retrieving users' data
        /// </summary>        
        /// <remarks></remarks>
        /// <remarks></remarks>        
        [Route("getOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public KalturaOTTUserListResponse GetOldStandard()
        {
            List<KalturaOTTUser> response = null;

            string userId = KS.GetFromRequest().UserId;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.UsersClient().GetUsersData(groupId, new List<string>() { userId });
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

        /// <summary>Update user information      
        /// </summary>
        /// <param name="user"> UserData Object (include basic and dynamic data)</param>
        /// <remarks>         User suspended = 2001, User does not exist = 2000
        /// </remarks>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemaValidationType.ACTION_ARGUMENTS)]
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

        /// <summary>Edit user details.        
        /// </summary>
        /// <param name="role_id"> The role identifier to add</param>
        /// <remarks>
        /// </remarks>
        [Route("addRole"), HttpPost]
        [ApiAuthorize]
        public bool AddRole(long role_id)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;
            string userId = KS.GetFromRequest().UserId;

            try
            {
                // call client
                response = ClientsManager.UsersClient().AddRoleToUser(groupId, userId, role_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Permanently delete a user. User to delete cannot be an exclusive household master, and cannot be default user.
        /// </summary>        
        /// <remarks>        
        /// Possible status codes: 
        /// Household suspended = 1009, Limitation period = 1014,  User does not exist = 2000, Default user cannot be deleted = 2030, Exclusive master user cannot be deleted = 2031
        /// </remarks>        
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemaValidationType.ACTION_ARGUMENTS)]
        public bool Delete()
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;
            int userId = int.Parse(KS.GetFromRequest().UserId);

            try
            {
                // call client
                response = ClientsManager.UsersClient().DeleteUser(groupId, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Logout the calling user.
        /// </summary>
        /// <returns></returns>
        [Route("logout"), HttpPost]
        [ApiAuthorize]
        public bool Logout()
        {
            bool response = false;

            KS ks = KS.GetFromRequest();
            int groupId = ks.GroupId;
            int userId = int.Parse(ks.UserId);
            string udid = KSUtils.ExtractKSPayload().UDID;
            string ip = Utils.Utils.GetClientIP();

            try
            {
                response = ClientsManager.UsersClient().SignOut(groupId, userId, ip, udid);

            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Activate the account by activation token
        /// </summary>
        /// <param name="partnerId">The partner ID</param>
        /// <param name="activationToken">Activation token of the user</param>
        /// <param name="username">Username of the user to activate</param>
        /// <returns></returns>
        [Route("activate"), HttpPost]
        [ValidationException(SchemaValidationType.ACTION_NAME)]
        [OldStandard("activationToken", "activation_token")]
        public Models.Users.KalturaOTTUser Activate(int partnerId, string username, string activationToken)
        {
            Models.Users.KalturaOTTUser response = null;

            try
            {
                response = ClientsManager.UsersClient().ActivateAccount(partnerId, username, activationToken);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Resend the activation token to a user
        /// </summary>
        /// <param name="partnerId">The partner ID</param>
        /// <param name="username">Username of the user to activate</param>
        /// <returns></returns>
        [Route("resendActivationToken"), HttpPost]
        [ValidationException(SchemaValidationType.ACTION_NAME)]
        public bool ResendActivationToken(int partnerId, string username)
        {
            bool response = false;

            try
            {
                response = ClientsManager.UsersClient().ResendActivationToken(partnerId, username);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Resend the activation token to a user
        /// </summary>
        /// <returns></returns>
        [Route("getEncryptedUserId"), HttpPost]
        [ApiAuthorize]
        public KalturaStringValue GetEncryptedUserId()
        {
            KalturaStringValue response = null;
            
            try
            {
                string userId = KS.GetFromRequest().UserId;
                string key = TCMClient.Settings.Instance.GetValue<string>("user_id_encryption_key");
                string iv = TCMClient.Settings.Instance.GetValue<string>("user_id_encryption_iv");

                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(iv))
                {
                    throw new InternalServerErrorException((int)WebAPI.Managers.Models.StatusCode.MissingConfiguration, "Configuration for encryption was not found");
                }

                response = new KalturaStringValue()
                {
                    value = Convert.ToBase64String(Utils.EncryptionUtils.AesEncrypt(userId, iv, key))
                };
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}
