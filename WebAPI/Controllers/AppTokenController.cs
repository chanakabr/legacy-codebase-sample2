using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.Exceptions;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Users;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/appToken/action")]
    public class AppTokenController : ApiController
    {
        /// <summary>
        /// Add new application authentication token
        /// </summary>
        /// <param name="appToken">Application token</param>
        /// <remarks>
        /// </remarks>
        /// <returns></returns>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public KalturaAppToken Add(KalturaAppToken appToken)
        {
            KalturaAppToken response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                response = AuthorizationManager.AddAppToken(appToken, groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Get application authentication token by id
        /// </summary>
        /// <param name="id">Application token identifier</param>
        /// <remarks>
        /// Possible status codes: 50020 = Invalid Application token
        /// </remarks>
        /// <returns></returns>
        [Route("get"), HttpPost]
        [ApiAuthorize]
        public KalturaAppToken Get(string id)
        {
            KalturaAppToken response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                response = AuthorizationManager.GetAppToken(id, groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Delete application authentication token by id
        /// </summary>
        /// <param name="id">Application token identifier</param>
        /// <remarks>
        /// Possible status codes: 50020 = Invalid Application token
        /// </remarks>
        /// <returns></returns>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        public bool Delete(string id)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                response = AuthorizationManager.DeleteAppToken(id, groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Starts a new KS (Kaltura Session) based on application authentication token id
        /// </summary>
        /// <param name="id">application token id</param>
        /// <param name="tokenHash">hashed token - current KS concatenated with the application token hashed using the application token ‘hashType’</param>
        /// <param name="userId">session user id, will be ignored if a different user id already defined on the application token</param>
        /// <param name="type">session type, will be ignored if a different session type already defined on the application token</param>
        /// <param name="expiry">session expiry (in seconds), could be overwritten by shorter expiry of the application token and the session-expiry that defined on the application token</param>
        /// <param name="udid"></param>
        /// <remarks>
        /// Possible status codes: 50020 = Invalid Application token, 50022 = Invalid application token hash, 50023 = Not active application token
        /// </remarks>
        /// <returns></returns>
        [Route("startSession"), HttpPost]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [ApiAuthorize]
        public KalturaSessionInfo StartSession(string id, string tokenHash, string userId = null, KalturaSessionType? type = null, int? expiry = null, string udid = null)
        {
            KalturaSessionInfo response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                response = AuthorizationManager.StartSessionWithAppToken(groupId, id, tokenHash, userId, udid, type, expiry);
                
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}