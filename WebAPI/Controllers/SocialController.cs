using ApiObjects;
using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Social;
using WebAPI.Models.Users;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("social")]
    public class SocialController : IKalturaController
    {
        /// <summary>
        /// Return the user object with social information according to a provided external social token
        /// </summary>
        /// <param name="partnerId">Partner identifier</param>
        /// <param name="token">Social token</param>
        /// <param name="type">Social network type</param>
        /// <remarks>Possible status codes: Conflict - 7000, MinFriendsLimitation - 7001, UserEmailIsMissing - 7017</remarks>
        [Action("getByToken")]
        [BlockHttpMethods("GET")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.Conflict)]
        [Throws(eResponseStatus.MinFriendsLimitation)]
        [Throws(eResponseStatus.UserEmailIsMissing)]
        static public KalturaSocial GetByToken(int partnerId, string token, KalturaSocialNetwork type)
        {
            if (string.IsNullOrEmpty(token))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "token");

            try
            {
                // call client
                switch (type)
                {
                    case KalturaSocialNetwork.facebook:
                        return ClientsManager.SocialClient().FBData(partnerId, token);
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return null;
        }

        /// <summary>
        /// Return the user object with social information according to a provided external social token
        /// </summary>
        /// <param name="partnerId">Partner identifier</param>
        /// <param name="token">Social token</param>
        /// <param name="type">Social network type</param>
        /// <remarks>Possible status codes: Conflict - 7000, MinFriendsLimitation - 7001, UserEmailIsMissing - 7017</remarks>
        [Action("getByTokenOldStandard")]
        [OldStandardAction("getByToken")]
        [ApiAuthorize]
        [BlockHttpMethods("GET")]
        [Obsolete]
        [Throws(eResponseStatus.Conflict)]
        [Throws(eResponseStatus.MinFriendsLimitation)]
        [Throws(eResponseStatus.UserEmailIsMissing)]

        static public KalturaSocialResponse GetByTokenOldStandard(int partnerId, string token, KalturaSocialNetwork type)
        {
            KalturaSocialResponse response = new KalturaSocialResponse();

            if (string.IsNullOrEmpty(token))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "token");

            try
            {
                // call client
                switch (type)
                {
                    case KalturaSocialNetwork.facebook:
                        response = ClientsManager.SocialClient().FBUserData(partnerId, token);
                        break;
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// List social accounts
        /// </summary>
        /// <param name="type">social type to get</param>
        /// <remarks></remarks>
        [Action("get")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        static public KalturaSocial Get(KalturaSocialNetwork type)
        {
            KalturaSocial response = null;

            string userId = KS.GetFromRequest().UserId;
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                switch (type)
                {
                    case KalturaSocialNetwork.facebook:
                        response = ClientsManager.SocialClient().FBUserDataByUserId(groupId, userId);
                        break;
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Login using social token
        /// </summary>        
        /// <param name="partnerId">Partner identifier</param>
        /// <param name="token">Social token</param>
        /// <param name="type">Social network</param>
        /// <param name="udid">Device UDID</param>
        /// <remarks>        
        /// User does not exist = 2000
        /// </remarks>
        [Action("login")]
        [BlockHttpMethods("GET")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        static public KalturaLoginResponse Login(int partnerId, string token, KalturaSocialNetwork type, string udid = null)
        {
            KalturaOTTUser response = null;

            if (string.IsNullOrEmpty(token))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "token");
            }
            try
            {
                // call client
                switch (type)
                {
                    case KalturaSocialNetwork.facebook:
                        response = ClientsManager.SocialClient().FBUserSignin(partnerId, token, udid);
                        break;
                }
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
        /// Create a new user in the system using a provided external social token
        /// </summary>
        /// <param name="token">social token</param>
        /// <param name="partnerId">Partner identifier</param>
        /// <param name="type">Social network type</param>
        /// <param name="email">User email</param>
        /// <remarks>Possible status codes: Conflict - 7000, MinFriendsLimitation - 7001</remarks>
        [Action("register")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.Conflict)]
        [Throws(eResponseStatus.MinFriendsLimitation)]
        [Throws(eResponseStatus.UserEmailIsMissing)]
        static public KalturaSocial Register(int partnerId, string token, KalturaSocialNetwork type, string email = null)
        {
            string ip = Utils.Utils.GetClientIP();

            // create extra parameters object
            var extraParameters = new List<KeyValuePair>() 
            {
                new KeyValuePair() 
                {
                    key = "news", 
                    value = "0" 
                },
                new KeyValuePair() 
                {
                    key = "domain", 
                    value = "0" 
                },
                new KeyValuePair() 
                {
                    key = "email", 
                    value = email 
                }  
            };

            if (string.IsNullOrEmpty(token))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "token");

            try
            {
                // call client
                switch (type)
                {
                    case KalturaSocialNetwork.facebook:
                        return ClientsManager.SocialClient().FBRegister(partnerId, token, extraParameters, ip);
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return null;
        }

        /// <summary>
        /// Create a new user in the system using a provided external social token
        /// </summary>
        /// <param name="token">social token</param>
        /// <param name="partnerId">Partner identifier</param>
        /// <param name="type">Social network type</param>
        /// <param name="should_create_domain">New domain is created upon registration</param>
        /// <param name="subscribe_newsletter">Subscribes to newsletter</param>
        /// <param name="email">User email</param>
        /// <remarks>Possible status codes: Conflict - 7000, MinFriendsLimitation - 7001, UserEmailIsMissing - 7017</remarks>
        [Action("registerOldStandard")]
        [OldStandardAction("register")]
        [ApiAuthorize]
        [Obsolete]
        [Throws(eResponseStatus.Conflict)]
        [Throws(eResponseStatus.MinFriendsLimitation)]
        [Throws(eResponseStatus.UserEmailIsMissing)]
        static public KalturaSocialResponse RegisterOldStandard(int partnerId, string token, bool should_create_domain, bool subscribe_newsletter, KalturaSocialNetwork type, string email = null)
        {
            KalturaSocialResponse response = new KalturaSocialResponse();

            string ip = Utils.Utils.GetClientIP();

            // create extra parameters object
            var extraParameters = new List<KeyValuePair>() 
            {
                new KeyValuePair() 
                {
                    key = "news", 
                    value = subscribe_newsletter ? "1" : "0" 
                },
                new KeyValuePair() 
                {
                    key = "domain", 
                    value = should_create_domain ? "1" : "0" 
                },
                new KeyValuePair() 
                {
                    key = "email", 
                    value = email 
                }  
            };

            if (string.IsNullOrEmpty(token))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "token");

            try
            {
                // call client
                switch (type)
                {
                    case KalturaSocialNetwork.facebook:
                        response = ClientsManager.SocialClient().FBUserRegister(partnerId, token, extraParameters, ip);
                        break;
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Connect an existing user in the system to an external social network user 
        /// </summary>
        /// <param name="token">social token</param>
        /// <param name="type">Social network type</param>
        /// <remarks>Possible status codes: Wrong password or username = 1011, Conflict - 7000, MinFriendsLimitation - 7001</remarks>
        [Action("merge")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.WrongPasswordOrUserName)]
        [Throws(eResponseStatus.Conflict)]
        [Throws(eResponseStatus.MinFriendsLimitation)]
        static public KalturaSocial Merge(string token, KalturaSocialNetwork type)
        {
            if (string.IsNullOrEmpty(token))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "token");

            string userId = KS.GetFromRequest().UserId;
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client                
                switch (type)
                {
                    case KalturaSocialNetwork.facebook:
                        return ClientsManager.SocialClient().FBUserMerge(groupId, userId, token);
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return null;
        }

        /// <summary>
        /// Connect an existing user in the system to an external social network user 
        /// </summary>
        /// <param name="token">social token</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="social_id">external social identifier</param>
        /// <param name="type">Social network type</param>
        /// <param name="partnerId">Partner identifier</param>
        /// <remarks>Possible status codes: Wrong password or username = 1011, Conflict - 7000, MinFriendsLimitation - 7001</remarks>
        [Action("mergeOldStandard")]
        [OldStandardAction("merge")]
        [ApiAuthorize]
        [Obsolete]
        [Throws(eResponseStatus.WrongPasswordOrUserName)]
        [Throws(eResponseStatus.Conflict)]
        [Throws(eResponseStatus.MinFriendsLimitation)]
        static public KalturaSocialResponse MergeOldStandard(int partnerId, string token, string username, string password, string social_id, KalturaSocialNetwork type)
        {
            KalturaSocialResponse response = new KalturaSocialResponse();

            if (string.IsNullOrEmpty(token))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "token");

            try
            {
                // call client                
                switch (type)
                {
                    case KalturaSocialNetwork.facebook:
                        response = ClientsManager.SocialClient().FBUserMerge(partnerId, token, username, password, social_id);
                        break;
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Disconnect an existing user in the system from its external social network user
        /// </summary>
        /// <param name="type">Social network type</param>
        /// <remarks>Possible status codes: Wrong password or username = 1011, Conflict - 7000, MinFriendsLimitation - 7001</remarks>
        [Action("unmerge")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.WrongPasswordOrUserName)]
        [Throws(eResponseStatus.Conflict)]
        [Throws(eResponseStatus.MinFriendsLimitation)]
        static public KalturaSocial Unmerge(KalturaSocialNetwork type)
        {
            string userId = KS.GetFromRequest().UserId;
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client               
                switch (type)
                {
                    case KalturaSocialNetwork.facebook:
                        return ClientsManager.SocialClient().FBUserUnmerge(groupId, userId);
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return null;
        }

        /// <summary>
        /// Disconnect an existing user in the system from its  external social network user
        /// </summary>
        /// <param name="token">Social token</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="type">Social network type</param>
        /// <remarks>Possible status codes: Wrong password or username = 1011, Conflict - 7000, MinFriendsLimitation - 7001</remarks>
        [Action("unmergeOldStandard")]
        [OldStandardAction("unmerge")]
        [ApiAuthorize]
        [Obsolete]
        [Throws(eResponseStatus.WrongPasswordOrUserName)]
        [Throws(eResponseStatus.Conflict)]
        [Throws(eResponseStatus.MinFriendsLimitation)]
        static public KalturaSocialResponse UnmergeOldStandard(string token, string username, string password, KalturaSocialNetwork type)
        {
            KalturaSocialResponse response = new KalturaSocialResponse();

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client               
                switch (type)
                {
                    case KalturaSocialNetwork.facebook:
                        response = ClientsManager.SocialClient().FBUserUnmerge(groupId, token, username, password);
                        break;
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Retrieve the social network’s configuration information 
        /// </summary>        
        /// <param name="type">Social network type</param>
        /// <param name="partnerId">Partner identifier</param>
        /// <returns></returns>
        [Action("getConfiguration")]
        [OldStandardAction("config")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        static public KalturaSocialConfig GetConfiguration( KalturaSocialNetwork? type, int? partnerId = null)
        {
            KalturaSocialConfig response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userID = KS.GetFromRequest().UserId;

                // call client      
                if (type == null)
                {
                    // to do return KalturaSocialUserConfig
                    response = ClientsManager.SocialClient().GetUserSocialPrivacySettings(userID, groupId);
                }
                else
                {
                    switch (type)
                    {
                        case KalturaSocialNetwork.facebook:
                            response = ClientsManager.SocialClient().GetFacebookConfig(groupId);
                            break;
                    }
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Set the user social network’s configuration information 
        /// </summary>      
        /// <param name="configuration">The social action settings</param>
        /// <returns></returns>
        [Action("UpdateConfiguration")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        static public KalturaSocialConfig UpdateConfiguration(KalturaSocialConfig configuration)
        {
            KalturaSocialConfig response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userID = KS.GetFromRequest().UserId;
                
                if (configuration is KalturaSocialUserConfig)
                {
                    KalturaSocialUserConfig socialActionConfig = (KalturaSocialUserConfig)configuration;
                    response = ClientsManager.SocialClient().SetUserActionShareAndPrivacy(groupId, userID, socialActionConfig);
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}