using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Models.Social;
using WebAPI.Social;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("social")]
    public class SocialController : ApiController
    {
        /// <summary>
        /// Retrieves facebook user data
        /// </summary>
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="token">Facebook token</param>
        /// <remarks>Possible status codes: Conflict - 7000, MinFriendsLimitationBad - 7001, credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008 </remarks>
        [Route("social/fb/user_data"), HttpGet]
        public KalturaFacebookResponse GetFBUserData([FromUri] string partner_id, [FromUri] string token)
        {
            KalturaFacebookResponse response = new KalturaFacebookResponse();

            // validate group ID
            int groupId = 0;
            if (!int.TryParse(partner_id, out groupId) || groupId < 1)
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Illegal partner ID");

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

        [Route("social/fb/user_data"), HttpPost]
        [ApiExplorerSettings(IgnoreApi = true)]
        public KalturaFacebookResponse _GetFBUserData([FromBody] string partner_id, [FromBody] string token)
        {
            return GetFBUserData(partner_id, token);
        }

        /// <summary>
        /// Registers new user by Facebook credentials
        /// </summary>
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="token">Facebook token</param>
        /// <param name="should_create_domain">New domain is created upon registration</param>
        /// <param name="subscribe_newsletter">Subscribes to newsletter</param>
        /// <remarks>Possible status codes: Conflict - 7000, MinFriendsLimitationBad - 7001, credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008 </remarks>
        [Route("social/fb/register"), HttpGet]
        public KalturaFacebookResponse FBUserRegister([FromUri] string partner_id, [FromUri] string token, [FromUri] bool should_create_domain, [FromUri] bool subscribe_newsletter)
        {
            KalturaFacebookResponse response = new KalturaFacebookResponse();

            // validate group ID
            int groupId = 0;
            if (!int.TryParse(partner_id, out groupId) || groupId < 1)
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Illegal partner ID");

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

        [Route("social/fb/register"), HttpPost]
        [ApiExplorerSettings(IgnoreApi = true)]
        public KalturaFacebookResponse _FBUserRegister([FromBody] string partner_id, [FromBody] string token, [FromBody] bool should_create_domain, [FromBody] bool get_newsletter)
        {
            return FBUserRegister(partner_id, token, should_create_domain, get_newsletter);
        }

        /// <summary>
        /// Merge a registered FB user with an existing regular user
        /// </summary>
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="token">Facebook token</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="facebook_id">Facebook identifier</param>
        /// <remarks>Possible status codes: Conflict - 7000, MinFriendsLimitationBad - 7001, credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008 </remarks>
        [Route("social/fb/merge"), HttpGet]
        public KalturaFacebookResponse FBUserMerge([FromUri] string partner_id, [FromUri] string token, [FromUri] string username, [FromUri] string password, [FromUri] string facebook_id)
        {
            KalturaFacebookResponse response = new KalturaFacebookResponse();

            // validate group ID
            int groupId = 0;
            if (!int.TryParse(partner_id, out groupId) || groupId < 1)
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Illegal partner ID");

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

        [Route("social/fb/merge"), HttpPost]
        [ApiExplorerSettings(IgnoreApi = true)]
        public KalturaFacebookResponse _FBUserMerge([FromBody] string partner_id, [FromBody] string token, [FromBody] string username, [FromBody] string password, [FromBody] string facebook_id)
        {
            return FBUserMerge(partner_id, token, username, password, facebook_id);
        }

        /// <summary>
        /// Removes data stored in Kaltura's DB which makes Facebook actions (login, share, like, etc) on the customer site feasible. The user is still be able to see the actions he performed as these are logged as 'Kaltura actions'. However, his friends won't be able to view his actions as they are deleted from social feed
        /// </summary>
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="token">Facebook token</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <remarks>Possible status codes: Conflict - 7000, MinFriendsLimitationBad - 7001, credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008 </remarks>
        [Route("social/fb/unmerge"), HttpGet]
        public KalturaFacebookResponse FBUserUnmerge([FromUri] string partner_id, [FromUri] string token, [FromUri] string username, [FromUri] string password)
        {
            KalturaFacebookResponse response = new KalturaFacebookResponse();

            // validate group ID
            int groupId = 0;
            if (!int.TryParse(partner_id, out groupId) || groupId < 1)
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Illegal partner ID");

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

        [Route("social/fb/unmerge"), HttpPost]
        [ApiExplorerSettings(IgnoreApi = true)]
        public KalturaFacebookResponse _FBUserUnmerge([FromBody] string partner_id, [FromBody] string token, [FromBody] string username, [FromBody] string password)
        {
            return FBUserUnmerge(partner_id, token, username, password);
        }
    }
}