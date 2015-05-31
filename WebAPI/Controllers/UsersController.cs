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
using WebAPI.Models.User;
using WebAPI.Models.General;

namespace WebAPI.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [RoutePrefix("users")]
    public class UsersController : ApiController
    {
        /// <summary>
        /// Generates a temporarily PIN that can allow a user to log-in.
        /// Possible status codes: BadCredentials = 500000, InternalConnectionIssue = 500001, Timeout = 500002, BadRequest = 500003, UserNotExists = 2000, UserSuspended = 2001
        /// </summary>
        /// <param name="search_assets">The search asset request parameter</param>
        /// <param name="group_id">Group Identifier</param>
        /// <param name="user_id">User Identifier</param>
        /// <remarks></remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [Route("generate_login_pin"), HttpPost]
        public LoginPin PostGenerateLoginPin([FromUri] string group_id, [FromUri] string user_id)
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
                response = ClientsManager.UsersClient().GenerateLoginPin(groupId, user_id);
            }
            catch (ClientException ex)
            {
                if (ex.Code == (int)WebAPI.Models.General.StatusCode.BadRequest)
                {
                    throw new BadRequestException(ex.Code, ex.ExceptionMessage);
                }

                throw new InternalServerErrorException(ex.Code, ex.ExceptionMessage);
            }

            return response;
        }


        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("generate_login_pin"), HttpGet]
        public LoginPin GetGenerateLoginPin(string group_id, string user_id)
        {
            return PostGenerateLoginPin(group_id, user_id);
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

            WebAPI.Models.User.ClientUser user = ClientsManager.UsersClient().SignIn(groupId, request.Username, request.Password);

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
