using AutoMapper;
using Newtonsoft.Json;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Routing;
using WebAPI.Clients.Utils;
using WebAPI.Filters;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Models;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [RoutePrefix("users")]
    public class UsersController : ApiController
    {
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
        [ApiAuthorize(Role = ApiAuthorizeAttribute.eRole.Admin | ApiAuthorizeAttribute.eRole.User)]
        public List<User> GetUsersData(string ids)
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

            List<User> dto = Mapper.Map<List<User>>(res);

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
        public bool Post([FromBody]User user)
        {

            return true;
        }

        [Route("{id}")]
        public void Put(int id, [FromBody]User value)
        {

        }

        [Route("{id}")]
        public void Delete(int id)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request">Credentials</param>
        /// <param name="group_id">Group ID</param>
        [Route("sign_in"), HttpPost]
        public string SignIn([FromUri] string group_id, [FromBody] SignIn request)
        {
            //TODO: add parameters

            // TODO: change to something later
            string data = string.Empty;

            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.StatusCode.BadRequest, "group_id must be int");
            }

            User user = ClientsManager.UsersClient().SignIn(groupId, request.Username, request.Password);

            if (user == null)
            {
                throw new InternalServerErrorException();
            }

            string userSecret = GroupsManager.GetGroup(groupId).AdminSecret;
            
            //TODO: get real value
            int expiration = 1462543601;

            return new KS(userSecret, group_id, user.ID, expiration, KS.eUserType.USER, data, string.Empty).ToString();
        }
    }
}
