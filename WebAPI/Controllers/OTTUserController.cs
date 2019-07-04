using ApiObjects.Response;
using ConfigurationManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.ModelBinding;
using System.Web.Routing;
using WebAPI.ClientManagers;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.Domains;
using WebAPI.Models.General;
using WebAPI.Models.Users;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Service("ottUser")]
    public class OttUserController : IKalturaController
    {
        /// <summary>
        /// Returns tokens (KS and refresh token) for anonymous access
        /// </summary>
        /// <param name="partnerId">The partner ID</param>
        /// <param name="udid">The caller device's UDID</param>
        /// <returns>KalturaLoginResponse</returns>
        [Action("anonymousLogin")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        static public KalturaLoginSession AnonymousLogin(int partnerId, string udid = null)
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
        /// UserNotInDomain = 1005, Wrong username or password = 1011, PinNotExists = 2003, PinExpired = 2004, NoValidPin = 2006, SecretIsWrong = 2008, 
        /// LoginViaPinNotAllowed = 2009, User suspended = 2001, InsideLockTime = 2015, UserNotActivated = 2016, 
        /// UserAllreadyLoggedIn = 2017,UserDoubleLogIn = 2018, DeviceNotRegistered = 2019, ErrorOnInitUser = 2021,UserNotMasterApproved = 2023, UserWithNoDomain = 2024, User does not exist = 2000
        /// </remarks>
        [Action("loginWithPin")]
        [BlockHttpMethods("GET")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.UserNotInDomain)]
        [Throws(eResponseStatus.WrongPasswordOrUserName)]
        [Throws(eResponseStatus.PinNotExists)]
        [Throws(eResponseStatus.PinExpired)]
        [Throws(eResponseStatus.NoValidPin)]
        [Throws(eResponseStatus.SecretIsWrong)]
        [Throws(eResponseStatus.LoginViaPinNotAllowed)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.InsideLockTime)]
        [Throws(eResponseStatus.UserNotActivated)]
        [Throws(eResponseStatus.UserAllreadyLoggedIn)]
        [Throws(eResponseStatus.UserDoubleLogIn)]
        [Throws(eResponseStatus.DeviceNotRegistered)]
        [Throws(eResponseStatus.ErrorOnInitUser)]
        [Throws(eResponseStatus.UserNotMasterApproved)]
        [Throws(eResponseStatus.UserWithNoDomain)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        static public KalturaLoginResponse LoginWithPin(int partnerId, string pin, string udid = null, string secret = null)
        {
            KalturaOTTUser response = null;

            if (string.IsNullOrEmpty(pin))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "pin");
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

            Dictionary<string, string> priviliges = null;
            var tmp = HttpContext.Current.Items[KLogMonitor.Constants.PRIVILIGES];
            if (tmp != null)
            {
                priviliges = (Dictionary<string, string>)tmp;
                HttpContext.Current.Items.Remove(KLogMonitor.Constants.PRIVILIGES);
            }

            return new KalturaLoginResponse() { LoginSession = AuthorizationManager.GenerateSession(response.Id.ToString(), partnerId, false, true, udid, priviliges), User = response };
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
        /// UserNotInDomain = 1005, Wrong username or password = 1011, User suspended = 2001, InsideLockTime = 2015, UserNotActivated = 2016, 
        /// UserAllreadyLoggedIn = 2017,UserDoubleLogIn = 2018, DeviceNotRegistered = 2019, ErrorOnInitUser = 2021,UserNotMasterApproved = 2023, User does not exist = 2000
        /// </remarks>
        [Action("login")]
        [BlockHttpMethods("GET")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [OldStandardArgument("extraParams", "extra_params")]
        [Throws(eResponseStatus.UserNotInDomain)]
        [Throws(eResponseStatus.WrongPasswordOrUserName)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.InsideLockTime)]
        [Throws(eResponseStatus.UserNotActivated)]
        [Throws(eResponseStatus.UserAllreadyLoggedIn)]
        [Throws(eResponseStatus.UserDoubleLogIn)]
        [Throws(eResponseStatus.DeviceNotRegistered)]
        [Throws(eResponseStatus.ErrorOnInitUser)]
        [Throws(eResponseStatus.UserNotMasterApproved)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserExternalError)]
        static public KalturaLoginResponse Login(int partnerId, string username = null, string password = null, SerializableDictionary<string, KalturaStringValue> extraParams = null,
            string udid = null)
        {
            KalturaOTTUser response = null;

            if ((string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) && !string.IsNullOrEmpty(udid))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "username or password");
            }

            Group group = GroupsManager.GetGroup(partnerId);
            try
            {
                // call client
                // add header. if key exists use extraParams
                response = ClientsManager.UsersClient().Login(partnerId, username, password, udid, extraParams, System.Web.HttpContext.Current.Request.Headers, group.ShouldSupportSingleLogin);
            }
            catch (ClientExternalException ex)
            {
                ErrorUtils.HandleClientExternalException(ex);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            if (response == null)
            {
                throw new InternalServerErrorException();
            }

            Dictionary<string, string> priviliges = null;
            var tmp = HttpContext.Current.Items[KLogMonitor.Constants.PRIVILIGES];
            if (tmp != null)
            {
                priviliges = (Dictionary<string, string>)tmp;
                HttpContext.Current.Items.Remove(KLogMonitor.Constants.PRIVILIGES);
            }

            return new KalturaLoginResponse() { LoginSession = AuthorizationManager.GenerateSession(response.Id.ToString(), partnerId, false, false, udid, priviliges), User = response };
        }

        /// <summary>
        /// Returns new Kaltura session (ks) for the user, using the supplied refresh_token (only if it's valid and not expired)
        /// </summary>
        /// <param name="refreshToken">Refresh token</param>
        /// <param name="udid">Device UDID</param>
        /// <returns></returns>
        [Action("refreshSession")]
        [ApiAuthorize(true)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [OldStandardArgument("refreshToken", "refresh_token")]
        [Throws(WebAPI.Managers.Models.StatusCode.InvalidRefreshToken)]
        [Throws(WebAPI.Managers.Models.StatusCode.InvalidKS)]
        [Throws(WebAPI.Managers.Models.StatusCode.RefreshTokenFailed)]
        [Obsolete]
        static public KalturaLoginSession RefreshSession(string refreshToken, string udid = null)
        {
            KalturaLoginSession response = null;

            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "refreshToken");
            }
            try
            {
                // call client
                response = AuthorizationManager.RefreshSession(refreshToken, udid);
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
        [Action("facebookLogin")]
        [Obsolete]
        [Throws(eResponseStatus.UserDoesNotExist)]
        static public KalturaLoginResponse FacebookLogin(int partnerId, string token, string udid = null)
        {
            KalturaOTTUser response = null;

            if (string.IsNullOrEmpty(token))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "token");
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
        [Action("register")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [OldStandardAction("add")]
        [Throws(eResponseStatus.WrongPasswordOrUserName)]
        [Throws(eResponseStatus.UserExists)]
        [Throws(eResponseStatus.ExternalIdAlreadyExists)]
        [Throws(eResponseStatus.UserExternalError)]
        [SchemeArgument("password", MaxLength = 128)]
        static public KalturaOTTUser Register(int partnerId, KalturaOTTUser user, string password)
        {
            KalturaOTTUser response = null;

            if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(password))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "username or password");
            }

            try
            {
                if (!string.IsNullOrEmpty(user.RoleIds) && !RolesManager.IsManagerAllowedAction(user.GetRoleIds()))
                {
                    throw new UnauthorizedException(UnauthorizedException.PROPERTY_ACTION_FORBIDDEN,
                                                       Enum.GetName(typeof(WebAPI.RequestType), WebAPI.RequestType.ALL),
                                                       "KalturaOTTUser",
                                                       "roleIds");
                }

                response = ClientsManager.UsersClient().SignUp(partnerId, user, password);
            }
            catch (ClientExternalException ex)
            {
                ErrorUtils.HandleClientExternalException(ex);
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
        /// <param name="templateName">Template name for reset password</param>
        /// <returns></returns>
        [Action("resetPassword")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [OldStandardAction("sendPassword")]
        [SchemeArgument("templateName", RequiresPermission = true)]
        static public bool resetPassword(int partnerId, string username, string templateName = null)
        {
            bool response = false;

            if (string.IsNullOrEmpty(username))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "username");
            }

            try
            {
                // call client
                response = ClientsManager.UsersClient().SendNewPassword(partnerId, username, templateName);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
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
        [Action("setInitialPassword")]
        [BlockHttpMethods("GET")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        static public KalturaOTTUser setInitialPassword(int partnerId, string token, string password)
        {
            KalturaOTTUser response = null;

            if (string.IsNullOrEmpty(token))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "token");
            }
            try
            {
                // call client
                response = ClientsManager.UsersClient().CheckPasswordToken(partnerId, token);
                ClientsManager.UsersClient().RenewPassword(partnerId, response.Username, password);
                AuthorizationManager.RevokeSessions(partnerId, response.Id);
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
        [Action("setPassword")]
        [Obsolete("Please use setInitialPassword instead")]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [OldStandardAction("resetPassword")]
        static public bool setPassword(int partnerId, string username, string password)
        {
            bool response = false;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "username");
            }
            try
            {
                // call client
                response = ClientsManager.UsersClient().RenewPassword(partnerId, username, password);
                var usersList = ClientsManager.UsersClient().GetUserByName(partnerId, username);
                if (usersList != null && usersList.Users != null && usersList.Users.Count > 0)
                    AuthorizationManager.RevokeSessions(partnerId, usersList.Users[0].Id);
                else
                    throw new InternalServerErrorException();

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
        /// Update the user's existing password.
        /// </summary>        
        /// <param name="userId">User Identifier</param>        
        /// <param name="password">new password</param>
        /// <remarks>Possible status codes: User does not exist = 2000</remarks>
        [Action("updatePassword")]
        [ApiAuthorize(true)]
        [BlockHttpMethods("GET")]
        [WebAPI.Managers.Scheme.ValidationException(WebAPI.Managers.Scheme.SchemeValidationType.ACTION_NAME)]
        static public void updatePassword(int userId, string password)
        {
            int groupId = KS.GetFromRequest().GroupId;

            if (userId <= 0)
            {
                throw new BadRequestException(BadRequestException.INVALID_USER_ID, "userId");
            }
            try
            {
                // call client
                ClientsManager.UsersClient().UpdateUserPassword(groupId, userId, password);
                AuthorizationManager.RevokeSessions(groupId, userId.ToString());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
        }

        /// <summary>
        /// Returns the user associated with a temporary reset token.
        /// </summary>        
        /// <param name="partnerId">Partner Identifier</param>
        /// <param name="token">token</param>
        /// <remarks>Possible status codes: 2000 = User does not exist</remarks>
        [Action("validateToken")]
        [Obsolete("Please use setInitialPassword instead")]
        [Throws(eResponseStatus.UserDoesNotExist)]
        static public KalturaOTTUser validateToken(int partnerId, string token)
        {
            KalturaOTTUser response = null;

            if (string.IsNullOrEmpty(token))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "token");
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
        [Throws(eResponseStatus.WrongPasswordOrUserName)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.InsideLockTime)]
        [Throws(eResponseStatus.UserAllreadyLoggedIn)]
        [Action("updateLoginData")]
        [BlockHttpMethods("GET")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [OldStandardArgument("oldPassword", "old_password")]
        [OldStandardArgument("newPassword", "new_password")]
        [OldStandardAction("changePassword")]
        static public bool UpdateLoginData(string username, string oldPassword, string newPassword)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            if (string.IsNullOrEmpty(username))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "username");
            }
            if (string.IsNullOrEmpty(oldPassword))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "oldPassword");
            }
            if (string.IsNullOrEmpty(newPassword))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "newPassword");
            }
            try
            {
                //TODO: get username by user id
                // call client
                response = ClientsManager.UsersClient().ChangeUserPassword(groupId, username, oldPassword, newPassword);
                var usersList = ClientsManager.UsersClient().GetUserByName(groupId, username);
                if (usersList != null && usersList.Users != null && usersList.Users.Count > 0)
                    AuthorizationManager.RevokeSessions(groupId, usersList.Users[0].Id);
                else
                    throw new InternalServerErrorException();
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
        [Action("get")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        static public KalturaOTTUser Get()
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

            if (response != null && response.Count > 0)
                return response.FirstOrDefault();
            else
                return null;
        }

        /// <summary>
        /// Retrieving users' data
        /// </summary>        
        /// <remarks></remarks>
        /// <remarks></remarks>        
        [Action("getOldStandard")]
        [OldStandardAction("get")]
        [ApiAuthorize]
        [Obsolete]
        static public KalturaOTTUserListResponse GetOldStandard()
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
        /// <param name="user"> User data (includes basic and dynamic data)</param>
        /// <param name="id">User ID</param>
        /// <remarks>         User suspended = 2001, User does not exist = 2000
        /// </remarks>
        [Action("update")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserExists)]
        [Throws(eResponseStatus.ExternalIdAlreadyExists)]
        [SchemeArgument("id", RequiresPermission = true)]
        static public KalturaOTTUser Update(KalturaOTTUser user, string id = null)
        {
            KalturaOTTUser response = null;

            int groupId = KS.GetFromRequest().GroupId;
            if (string.IsNullOrEmpty(id))
            {
                id = KS.GetFromRequest().UserId;
            }

            try
            {
                if (!RolesManager.IsManagerAllowedUpdateAction(id, user.GetRoleIds()))
                {
                    throw new UnauthorizedException(UnauthorizedException.PROPERTY_ACTION_FORBIDDEN,
                                                       Enum.GetName(typeof(WebAPI.RequestType), WebAPI.RequestType.ALL),
                                                       "KalturaOTTUser",
                                                       "roleIds");
                }

                response = ClientsManager.UsersClient().SetUserData(groupId, id, user);
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
        /// Deprecate - use Register or Update actions instead by setting user.roleIds parameter
        /// </summary>
        /// <param name="roleId"> The role identifier to add</param>
        /// <remarks>
        /// </remarks>
        [Action("addRole")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [OldStandardArgument("roleId", "role_id")]
        static public bool AddRole(long roleId)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;
            string userId = KS.GetFromRequest().UserId;

            try
            {
                List<long> roleToAdd = new List<long>();
                roleToAdd.Add(roleId);
                
                if (!RolesManager.IsManagerAllowedUpdateAction(userId, roleToAdd))
                {
                    throw new UnauthorizedException(UnauthorizedException.PROPERTY_ACTION_FORBIDDEN,
                                                       Enum.GetName(typeof(WebAPI.RequestType), WebAPI.RequestType.ALL),
                                                       "KalturaOTTUser",
                                                       "roleId");
                }

                // call client
                response = ClientsManager.UsersClient().AddRoleToUser(groupId, userId, roleId);
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
        [Action("delete")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [Throws(eResponseStatus.DomainSuspended)]
        [Throws(eResponseStatus.LimitationPeriod)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.DefaultUserCannotBeDeleted)]
        [Throws(eResponseStatus.ExclusiveMasterUserCannotBeDeleted)]
        static public bool Delete()
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;
            int userId = int.Parse(KS.GetFromRequest().UserId);

            try
            {
                if (!RolesManager.IsAllowedDeleteAction())
                {
                    throw new UnauthorizedException(UnauthorizedException.SERVICE_FORBIDDEN);
                }

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
        [Action("logout")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        static public bool Logout()
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
                if (response)
                {
                    response = AuthorizationManager.LogOut(ks);
                }

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
        [Action("activate")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [OldStandardArgument("activationToken", "activation_token")]
        static public Models.Users.KalturaOTTUser Activate(int partnerId, string username, string activationToken)
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
        [Action("resendActivationToken")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        static public bool ResendActivationToken(int partnerId, string username)
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
        /// Returns the identifier of the user encrypted with SHA1 using configured key
        /// </summary>
        /// <returns></returns>
        [Action("getEncryptedUserId")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        static public KalturaStringValue GetEncryptedUserId()
        {
            KalturaStringValue response = null;

            try
            {
                string userId = KS.GetFromRequest().UserId;
                string key = ApplicationConfiguration.OTTUserControllerConfiguration.UserIdEncryptionKey.Value;
                string iv = ApplicationConfiguration.OTTUserControllerConfiguration.UserIdEncryptionIV.Value;

                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(iv))
                {
                    throw new InternalServerErrorException(InternalServerErrorException.MISSING_CONFIGURATION, "Encryption");
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

        /// <summary>
        /// Returns list of OTTUser (limited to 500 items). Filters by username/external identifier/idIn or roleIdIn
        /// </summary>
        /// <param name="filter">Filter request</param>
        /// <remarks>Possible status codes: 
        /// UserDoesNotExist = 2000 </remarks>
        /// <returns>List of OTTUser limited in 500 items</returns>
        [Action("list")]
        [ApiAuthorize]
        [Throws(eResponseStatus.UserDoesNotExist)]
        static public KalturaOTTUserListResponse List(KalturaOTTUserFilter filter = null)
        {
            KalturaOTTUserListResponse response = null;

            var ks = KS.GetFromRequest();
            int groupId = ks.GroupId;

            if (filter == null)
            {
                filter = new KalturaOTTUserFilter();
            }

            try
            {
                bool isOperatorOrAbove = RolesManager.GetRoleIds(ks).
                    Any(ur => ur == RolesManager.OPERATOR_ROLE_ID || ur == RolesManager.MANAGER_ROLE_ID || ur == RolesManager.ADMINISTRATOR_ROLE_ID);

                filter.Validate(isOperatorOrAbove);

                if (!string.IsNullOrEmpty(filter.UsernameEqual))
                {
                    response = ClientsManager.UsersClient().GetUserByName(groupId, filter.UsernameEqual);
                }
                else if (!string.IsNullOrEmpty(filter.ExternalIdEqual))
                {
                    response = ClientsManager.UsersClient().GetUserByExternalID(groupId, filter.ExternalIdEqual);
                }
                else if (!string.IsNullOrEmpty(filter.IdIn)) 
                {
                    List<string> usersToGet = null;
                    KalturaHousehold household = HouseholdUtils.GetHouseholdFromRequest();

                    // get users only from my domain
                    if (household != null && !isOperatorOrAbove)
                    {
                        var householdUsers = HouseholdUtils.GetHouseholdUserIds(groupId);
                        if (householdUsers != null && householdUsers.Count > 0)
                        {
                            usersToGet = new List<string>(filter.GetIdIn().Where(userId => householdUsers.Contains(userId)));
                        }
                    }
                    // get users from idIn
                    else if (isOperatorOrAbove)
                    {
                        usersToGet = filter.GetIdIn();
                    }
                    else // no household and less then Operator
                    {
                        throw new UnauthorizedException(UnauthorizedException.PROPERTY_ACTION_FORBIDDEN, 
                                                        Enum.GetName(typeof(WebAPI.RequestType), WebAPI.RequestType.READ),
                                                        "KalturaOTTUserFilter", 
                                                        "idIn");
                    }

                    response = GetUsersData(groupId, usersToGet);
                }
                else // empty filter (can have roleIds)
                {
                    response = new KalturaOTTUserListResponse();
                    string userId = KS.GetFromRequest().UserId;
                    string originalUserId = KS.GetFromRequest().OriginalUserId;
                    if (string.IsNullOrEmpty(originalUserId))
                    {
                        originalUserId = userId;
                    }

                    // get all users of the master / itself                    
                    List<string> householdUserIds = null;
                    if (!userId.Equals(originalUserId) || !isOperatorOrAbove)
                    {
                        householdUserIds = new List<string>();
                        if (HouseholdUtils.GetHouseholdFromRequest() != null)
                        {
                            householdUserIds.AddRange(HouseholdUtils.GetHouseholdUserIds(groupId).Distinct());
                        }
                        else
                        {
                            householdUserIds.Add(userId);
                        }
                    }

                    response = GetUsersData(groupId, householdUserIds, filter.GetRoleIdsIn());
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        private static KalturaOTTUserListResponse GetUsersData(int groupId, List<string> usersToGet, HashSet<long> roleIdsIn = null)
        {
            KalturaOTTUserListResponse response = new KalturaOTTUserListResponse();
            if ((usersToGet != null && usersToGet.Count > 0) || (roleIdsIn != null && roleIdsIn.Count > 0))
            {
                response.Users = ClientsManager.UsersClient().GetUsersData(groupId, usersToGet, roleIdsIn);
            }

            if (response.Users != null)
            {
                response.TotalCount = response.Users.Count;
            }

            return response;
        }

        /// <summary>
        /// Update user dynamic data
        /// </summary>
        /// <param name="key">Type of dynamicData </param>
        /// <param name="value">Value of dynamicData </param>
        /// <param name="userId">User identifier</param>
        /// <returns></returns>
        [Action("updateDynamicData")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        static public KalturaOTTUserDynamicData UpdateDynamicData(string key, KalturaStringValue value)
        {
            KalturaOTTUserDynamicData response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                response = ClientsManager.UsersClient().SetUserDynamicData(groupId, KS.GetFromRequest().UserId, key, value);
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
