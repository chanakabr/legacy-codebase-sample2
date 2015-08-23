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
        /// Retrieves facebook user data
        /// </summary>
        /// <param name="token">Facebook token</param>
        /// <remarks>Possible status codes: Conflict - 7000, MinFriendsLimitationBad - 7001, credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008 </remarks>
        [Route("getFBUserData"), HttpPost]
        [ApiAuthorize]
        public KalturaFacebookResponse GetFBUserData(string token)
        {
            KalturaFacebookResponse response = new KalturaFacebookResponse();
            
            int groupId = KS.GetFromRequest().GroupId;

            if (string.IsNullOrEmpty(token))
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "token cannot be empty");

            try
            {
                // call client
                response = ClientsManager.SocialClient().FBUserData(groupId, token);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Registers new user by Facebook credentials
        /// </summary>
        /// <param name="token">Facebook token</param>
        /// <param name="should_create_domain">New domain is created upon registration</param>
        /// <param name="subscribe_newsletter">Subscribes to newsletter</param>
        /// <remarks>Possible status codes: Conflict - 7000, MinFriendsLimitationBad - 7001, credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008 </remarks>
        [Route("FBUserRegister"), HttpPost]
        [ApiAuthorize]
        public KalturaFacebookResponse FBUserRegister(string token, bool should_create_domain, bool subscribe_newsletter)
        {
            KalturaFacebookResponse response = new KalturaFacebookResponse();
            
            int groupId = KS.GetFromRequest().GroupId;

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
                response = ClientsManager.SocialClient().FBUserRegister(groupId, token, extraParameters, ip);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Merge a registered FB user with an existing regular user
        /// </summary>
        /// <param name="token">Facebook token</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="facebook_id">Facebook identifier</param>
        /// <remarks>Possible status codes: Wrong password or username = 1011, Conflict - 7000, MinFriendsLimitationBad - 7001, credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008 </remarks>
        [Route("FBUserMerge"), HttpPost]
        public KalturaFacebookResponse FBUserMerge(string token, string username, string password, string facebook_id)
        {
            KalturaFacebookResponse response = new KalturaFacebookResponse();
            
            int groupId = KS.GetFromRequest().GroupId;

            if (string.IsNullOrEmpty(token))
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "token cannot be empty");

            try
            {
                // call client
                response = ClientsManager.SocialClient().FBUserMerge(groupId, token, username, password, facebook_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Removes data stored in Kaltura's DB which makes Facebook actions (login, share, like, etc) on the customer site feasible. The user is still be able to see the actions he performed as these are logged as 'Kaltura actions'. However, his friends won't be able to view his actions as they are deleted from social feed
        /// </summary>
        /// <param name="token">Facebook token</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <remarks>Possible status codes: Wrong password or username = 1011, Conflict - 7000, MinFriendsLimitationBad - 7001, credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008 </remarks>
        [Route("FBUserUnmerge"), HttpPost]
        [ApiAuthorize]
        public KalturaFacebookResponse FBUserUnmerge(string token, string username, string password)
        {
            KalturaFacebookResponse response = new KalturaFacebookResponse();
            
            int groupId = KS.GetFromRequest().GroupId;

            if (string.IsNullOrEmpty(token))
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "token cannot be empty");

            try
            {
                // call client
                response = ClientsManager.SocialClient().FBUserUnmerge(groupId, token, username, password);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Returns the facebook application configuration for the partner
        /// </summary>        
        /// <returns></returns>
        [Route("FBConfig"), HttpPost]
        [ApiAuthorize(true)]
        public KalturaFacebookConfig FBConfig()
        {
            KalturaFacebookConfig response = null;
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.SocialClient().GetFacebookConfig(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}