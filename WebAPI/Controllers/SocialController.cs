using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Social;
using WebAPI.Social;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/social/action")]
    public class SocialController : ApiController
    {
        /// <summary>
        /// Retrieves social user data
        /// </summary>
        /// <param name="partnerId">Partner identifier</param>
        /// <param name="token">Social token</param>
        /// <param name="type">Social network type</param>
        /// <remarks>Possible status codes: Conflict - 7000, MinFriendsLimitationBad - 7001,,         </remarks>
        [Route("getByToken"), HttpPost]
        public KalturaSocialResponse GetByToken(int partnerId, string token, KalturaSocialNetwork type)
        {
            KalturaSocialResponse response = new KalturaSocialResponse();            

            if (string.IsNullOrEmpty(token))
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "token cannot be empty");

            try
            {
                // call client
                switch (type)
                {
                    case KalturaSocialNetwork.facebook:
                        response = ClientsManager.SocialClient().FBUserData(partnerId, token);
                        break;
                    default:
                        throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Unknown social network");
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Registers new user by social credentials
        /// </summary>
        /// <param name="token">social token</param>
        /// <param name="partnerId">Partner identifier</param>
        /// <param name="type">Social network type</param>
        /// <param name="should_create_domain">New domain is created upon registration</param>
        /// <param name="subscribe_newsletter">Subscribes to newsletter</param>
        /// <remarks>Possible status codes: Conflict - 7000, MinFriendsLimitationBad - 7001,,         </remarks>
        [Route("register"), HttpPost]        
        public KalturaSocialResponse Register(int partnerId, string token, bool should_create_domain, bool subscribe_newsletter, KalturaSocialNetwork type)
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
                } 
            };

            if (string.IsNullOrEmpty(token))
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "token cannot be empty");

            try
            {
                // call client
                switch (type)
                {
                    case KalturaSocialNetwork.facebook:
                        response = ClientsManager.SocialClient().FBUserRegister(partnerId, token, extraParameters, ip);
                        break;
                    default:
                        throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Unknown social network");
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Merge a registered social user with an existing regular user
        /// </summary>
        /// <param name="token">social token</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="social_id">external social identifier</param>
        /// <param name="type">Social network type</param>
        /// <param name="partnerId">Partner identifier</param>
        /// <remarks>Possible status codes: Wrong password or username = 1011, Conflict - 7000, MinFriendsLimitationBad - 7001,,         </remarks>
        [Route("merge"), HttpPost]        
        public KalturaSocialResponse Merge(int partnerId, string token, string username, string password, string social_id, KalturaSocialNetwork type)
        {
            KalturaSocialResponse response = new KalturaSocialResponse();

            if (string.IsNullOrEmpty(token))
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "token cannot be empty");

            try
            {
                // call client                
                switch (type)
                {
                    case KalturaSocialNetwork.facebook:
                        response = ClientsManager.SocialClient().FBUserMerge(partnerId, token, username, password, social_id);
                        break;
                    default:
                        throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Unknown social network");
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Removes data stored in Kaltura's DB which makes social actions (login, share, like, etc) on the customer site feasible. 
        /// The user is still be able to see the actions he performed as these are logged as 'Kaltura actions'. 
        /// However, his friends won't be able to view his actions as they are deleted from social feed
        /// </summary>
        /// <param name="token">Social token</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="type">Social network type</param>
        /// <remarks>Possible status codes: Wrong password or username = 1011, Conflict - 7000, MinFriendsLimitationBad - 7001,,         </remarks>
        [Route("unmerge"), HttpPost]
        [ApiAuthorize]
        public KalturaSocialResponse Unmerge(string token, string username, string password, KalturaSocialNetwork type)
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
                    default:
                        throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Unknown social network");
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Returns the social application configuration for the partner
        /// </summary>        
        /// <param name="type">Social network type</param>
        /// <param name="partnerId">Partner identifier</param>
        /// <returns></returns>
        [Route("config"), HttpPost]
        public KalturaSocialConfig Config(int partnerId, KalturaSocialNetwork type)
        {
            KalturaSocialConfig response = null;            

            try
            {
                // call client               
                switch (type)
                {
                    case KalturaSocialNetwork.facebook:
                        response = ClientsManager.SocialClient().GetFacebookConfig(partnerId);
                        break;
                    default:
                        throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Unknown social network");
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