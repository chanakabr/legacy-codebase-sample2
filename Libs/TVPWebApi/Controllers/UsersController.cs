using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.ValueProviders;
using System.Web.Routing;
using TVPApi;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using TVPPro.SiteManager.TvinciPlatform.Users;
using TVPWebApi.Models;

namespace TVPWebApi.Controllers
{
    public class UsersController : ApiController
    {
        private readonly IUsersService _service;

        public UsersController(IUsersService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get a specific user.
        /// </summary>
        /// <param name="site_guid">User identifer</param>
        /// <returns></returns>
        [PartialResponse]
        public HttpResponseMessage Get(string site_guid, string fields = "")
        {
            InitializationObject initObj = (InitializationObject)Request.Properties["InitObj"];

            var response = _service.GetUserData(initObj, site_guid);

            if (response == null)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
            else if (response.m_RespStatus == ResponseStatus.UserDoesNotExist)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, response);
            }

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>
        /// Update an existing user.
        /// </summary>
        /// <param name="site_guid">User identifer</param>
        /// <param name="user_data">UserData object</param>
        /// <returns></returns>
        public HttpResponseMessage Put(string site_guid, UserData user_data)
        {
            InitializationObject initObj = (InitializationObject)Request.Properties["InitObj"];

            var response = _service.SetUserData(initObj, site_guid, user_data.basic_data, user_data.dynamic_data);

            if (response == null)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
            else if (response.m_RespStatus != ResponseStatus.OK)
            {
                return Request.CreateResponse(HttpStatusCode.NotModified, response);
            }

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>
        /// Add a new user.
        /// </summary>
        /// <param name="user">User object</param>
        /// <returns></returns>
        public HttpResponseMessage Post(TVPWebApi.Models.User user)
        {
            InitializationObject initObj = (InitializationObject)Request.Properties["InitObj"];

            var response = _service.SignUp(initObj, user.user_data.basic_data, user.user_data.dynamic_data, user.password, user.affiliate_code);

            if (response == null)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
            else if (response.m_RespStatus != ResponseStatus.OK)
            {
                return Request.CreateResponse(HttpStatusCode.NotModified, response);
            }

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>
        /// Get user's permitterd subscriptions
        /// </summary>
        /// <param name="site_guid">User identifer</param>
        /// <returns></returns>
        [ActionName("permittedsubscriptions")]
        [PartialResponse]
        public HttpResponseMessage GetPermittedSubscriptions(string site_guid, int limit = 10, int offset = 0, string fields = "")
        {
            InitializationObject initObj = (InitializationObject)Request.Properties["InitObj"];

            var response = _service.GetUserPermitedSubscriptions(initObj, site_guid);

            if (response == null)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
            else if (response.Length == 0)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, response);
            }

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>
        /// Get user's expired subscriptions
        /// </summary>
        /// <param name="site_guid">User identifer</param>
        /// <param name="limit">records per page</param>
        /// <param name="offset">page</param>
        /// <param name="fields">required fields</param>
        /// <returns></returns>
        [ActionName("expiredsubscriptions")]
        [PartialResponse]
        public HttpResponseMessage GetExpiredSubscriptions(string site_guid, int limit = 10, int offset = 0, string fields = "")
        {
            InitializationObject initObj = (InitializationObject)Request.Properties["InitObj"];

            var response = _service.GetUserExpiredSubscriptions(initObj, site_guid, limit);

            if (response == null)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
            else if (response.Length == 0)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, response);
            }

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>
        /// Get user's permitted items
        /// </summary>
        /// <param name="site_guid">User identifer</param>
        /// <param name="limit">records per page</param>
        /// <param name="offset">page</param>
        /// <param name="fields">required fields</param>
        /// <returns></returns>
        [ActionName("permitteditems")]
        [PartialResponse]
        public HttpResponseMessage GetPermittedItems(string site_guid, int limit = 10, int offset = 0, string fields = "")
        {
            InitializationObject initObj = (InitializationObject)Request.Properties["InitObj"];

            var response = _service.GetUserPermittedItems(initObj, site_guid);

            if (response == null)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
            else if (response.Length == 0)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, response);
            }

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>
        /// Get user's expired items
        /// </summary>
        /// <param name="site_guid">User identifer</param>
        /// <param name="limit">records per page</param>
        /// <param name="offset">page</param>
        /// <param name="fields">required fields</param>
        /// <returns></returns>
        [ActionName("expireditems")]
        [PartialResponse]
        public HttpResponseMessage GetExpiredItems(string site_guid, int limit = 10, int offset = 0, string fields = "")
        {
            InitializationObject initObj = (InitializationObject)Request.Properties["InitObj"];

            var response = _service.GetUserExpiredItems(initObj, site_guid, limit);

            if (response == null)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
            else if (response.Length == 0)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, response);
            }

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>
        /// Get user's favorites
        /// </summary>
        /// <param name="site_guid">User identifer</param>
        /// <param name="limit">records per page</param>
        /// <param name="offset">page</param>
        /// <param name="fields">required fields</param>
        /// <returns></returns>
        [ActionName("favorites")]
        [PartialResponse]
        public HttpResponseMessage GetUserFavorites(string site_guid, int limit = 10, int offset = 0, string fields = "")
        {
            InitializationObject initObj = (InitializationObject)Request.Properties["InitObj"];

            var response = _service.GetUserFavorites(initObj, site_guid);

            if (response == null)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
            else if (response.Length == 0)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, response);
            }

            //response = response.Skip(offset * limit).Take(limit).ToArray();

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>
        /// Add a new favorite
        /// </summary>
        /// <param name="site_guid">User identifer</param>
        /// <param name="request_data">Additional data</param>
        /// <returns></returns>
        [ActionName("favorites")]
        public HttpResponseMessage PostUserFavorite(string site_guid, FavoriteRequest request_data)
        {
            InitializationObject initObj = (InitializationObject)Request.Properties["InitObj"];

            bool response = _service.AddUserFavorite(initObj, site_guid, request_data.media_id, request_data.media_type, request_data.extra_val);

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>
        /// Delete an exisiting favorite
        /// </summary>
        /// <param name="site_guid">User identifer</param>
        /// <param name="media_id">Relevant media identifier</param>
        /// <returns></returns>
        [ActionName("favorites")]
        public HttpResponseMessage DeleteUserFavorite(string site_guid, int media_id)
        {
            InitializationObject initObj = (InitializationObject)Request.Properties["InitObj"];

            bool response = _service.RemoveUserFavorite(initObj, site_guid, media_id);

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>
        /// Get user group rules.
        /// </summary>
        /// <param name="site_guid">User identifer</param>
        /// <param name="limit">records per page</param>
        /// <param name="offset">page</param>
        /// <param name="fields">required fields</param>
        /// <returns></returns>
        [ActionName("rules")]
        [PartialResponse]
        public HttpResponseMessage GetUserGroupRules(string site_guid, int limit = 10, int offset = 0, string fields = "") 
        {
            InitializationObject initObj = (InitializationObject)Request.Properties["InitObj"];

            var response = _service.GetUserGroupRules(initObj, site_guid);

            if (response == null)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
            else if (response.Length == 0)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, response);
            }

            //response = response.Skip(offset * limit).Take(limit).ToArray();

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>
        /// Set user group rules
        /// </summary>
        /// <param name="site_guid">User identifer</param>
        /// <param name="request_data">Additional data</param>
        /// <returns></returns>
        [ActionName("rules")]
        public HttpResponseMessage PutUserGroupRule(string site_guid, GroupRuleRequest request_data)
        {
            InitializationObject initObj = (InitializationObject)Request.Properties["InitObj"];

            bool response = _service.SetUserGroupRule(initObj, site_guid, request_data.rule_id, request_data.pin_code, request_data.is_active);

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>
        /// Check user group rule
        /// </summary>
        /// <param name="site_guid">User identifer</param>
        /// <param name="request_data">Additional data</param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("rules")]
        public HttpResponseMessage CheckUserGroupRule(string site_guid, GroupRuleRequest request_data)
        {
            InitializationObject initObj = (InitializationObject)Request.Properties["InitObj"];

            bool response = _service.CheckGroupRule(initObj, site_guid, request_data.rule_id, request_data.pin_code);

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }
    }
}
