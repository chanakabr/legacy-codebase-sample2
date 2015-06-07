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
        public LoginPin PostGenerateLoginPin([FromUri] string group_id, [FromUri] string user_id, [FromUri] string secret = null)
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
            return PostGenerateLoginPin(group_id, user_id, secret);
        }

        /// <summary>
        /// User sign-in via a time-expired sign-in PIN.
        /// Possible status codes: BadCredentials = 500000, InternalConnectionIssue = 500001, Timeout = 500002, BadRequest = 500003, UserNotExists = 2000, UserSuspended = 2001
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
        public User PostLogInWithPin([FromUri] string group_id, [FromUri] string pin, [FromUri] string device_id = null, [FromUri] string secret = null)
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
            return PostLogInWithPin(group_id, pin, device_id, secret);
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
        public bool PostSetLoginPin([FromUri] string group_id, [FromUri] string user_id, [FromUri] string pin, [FromUri] string secret = null)
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
        public bool DeleteClearLoginPin([FromUri] string group_id, [FromUri] string user_id)
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

        /// <summary>
        /// Retrieving users' data
        /// </summary>
        /// <param name="ids">Users IDs to retreive. Use ',' as a seperator between the IDs</param>
        /// <remarks></remarks>
        /// <returns>WebAPI.Models.User</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [Route("{ids}"), HttpGet]
        //[ApiAuthorize()]
        [ApiExplorerSettings(IgnoreApi = true)]
        public List<ClientUser> GetUsersData(string ids)
        {
            var c = new Users.UsersService();

            //XXX: Example of using the unmasking
            string[] unmaskedIds = null;
            try
            {
                unmaskedIds = ids.Split(',').Select(x => SerializationUtils.UnmaskSensitiveObject(x)).Distinct().ToArray();
            }
            catch
            {
                /*
                 * We don't want to return 500 here, because if something went bad in the parameters, it means 400, but since
                 * the model is valid (we can't really validate the unmasking thing on the model), we are doing it manually.
                */
                throw new BadRequestException();
            }

            var res = c.GetUsersData("users_215", "11111", unmaskedIds);
            List<ClientUser> dto = Mapper.Map<List<ClientUser>>(res);
            return dto;
        }

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
        /// 
        /// </summary>
        /// <param name="request">Credentials</param>
        /// <param name="group_id">Group ID</param>
        [Route("sign_in"), HttpPost]
        [ApiExplorerSettings(IgnoreApi = true)]
        public string SignIn([FromUri] string group_id, [FromBody] SignIn request)
        {
            //TODO: add parameters

            // TODO: change to something later
            string data = string.Empty;

            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be int");
            }

            WebAPI.Models.Users.ClientUser user = ClientsManager.UsersClient().SignIn(groupId, request.Username, request.Password);

            if (user == null)
            {
                throw new InternalServerErrorException();
            }

            string userSecret = GroupsManager.GetGroup(groupId).UserSecret;

            //TODO: get real value
            int expiration = 1462543601;

            return new KS(userSecret, group_id, user.ID, expiration, KS.eUserType.USER, data, string.Empty).ToString();
        }


    }
}
