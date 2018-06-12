using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Users;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/ssoAdapterProfile/action")]
    public class SsoAdapterProfileController : ApiController
    {
        /// <summary>
        /// Returns all sso adapters for partner : id + name
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        ///  
        /// </remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaSsoAdapterProfileListResponse List()
        {
            var response = new KalturaSsoAdapterProfileListResponse();
            var groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response.SSOAdapters = ClientsManager.UsersClient().GetSSOAdapters(groupId);
                response.TotalCount = response.SSOAdapters.Count;
                return response;
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Delete sso adapters by sso adapters id
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        /// sso adapter not exist = 2056
        /// </remarks>
        /// <param name="ssoAdapterId">SSO Adapter Identifier</param>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        [OldStandardArgument("ssoAdapterId", "sso_adapter_id")]
        [Throws(eResponseStatus.SSOAdapaterNotExist)]
        public bool Delete(int ssoAdapterId)
        {
            var response = false;
            var groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.UsersClient().DeleteSSOAdapater(groupId, ssoAdapterId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Insert new sso adapter for partner
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        ///   External identifier is required = 6016, Name is required = 5005, Shared secret is required = 5006, External identifier must be unique = 6040, No sso adapter to insert = 2057
        /// </remarks>
        /// <param name="ssoAdapater">SSO Adapter Object</param>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.ExternalIdentifierRequired)]
        [Throws(eResponseStatus.NameRequired)]
        [Throws(eResponseStatus.SharedSecretRequired)]
        [Throws(eResponseStatus.ExternalIdentifierMustBeUnique)]
        [Throws(eResponseStatus.NoSSOAdapaterToInsert)]
        public KalturaSsoAdapterProfile Add(KalturaSsoAdapterProfile ssoAdapater)
        {
            KalturaSsoAdapterProfile response = null;
            var ks = KS.GetFromRequest();
            var groupId = ks.GroupId;
            var userId = ks.UserId;

            try
            {
                // call client
                response = ClientsManager.UsersClient().InsertSSOAdapter(groupId, ssoAdapater, int.Parse(userId));
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update sso adapter details
        /// </summary>
        /// <remarks>
        /// Possible status codes:      
        /// Action is not allowed = 5011, sso adapter identifier is required = 2058, Name is required = 5005, Shared secret is required = 5006, External idntifier missing = 6016, 
        /// External identifier must be unique = 6040            
        /// </remarks>
        /// <param name="ssoAdapterId">SSO Adapter Identifier</param> 
        /// <param name="ssoAdapater">SSO Adapter Object</param>       
        [Route("update"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.ActionIsNotAllowed)]
        [Throws(eResponseStatus.SSOAdapterIdRequired)]
        [Throws(eResponseStatus.NameRequired)]
        [Throws(eResponseStatus.SharedSecretRequired)]
        [Throws(eResponseStatus.ExternalIdentifierRequired)]
        [Throws(eResponseStatus.ExternalIdentifierMustBeUnique)]
        public KalturaSsoAdapterProfile Update(int ssoAdapterId, KalturaSsoAdapterProfile ssoAdapater)
        {
            KalturaSsoAdapterProfile response = null;
            var groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.UsersClient().SetSSOAdapter(groupId, ssoAdapterId, ssoAdapater);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Generate SSO Adapter shared secret
        /// </summary>
        /// <remarks>
        /// Possible status codes:  
        /// SSO Adapter id required = 2058, sso adapater not exist = 2056
        /// </remarks>
        /// <param name="ssoAdapterId">SSO Adapter identifier</param>
        [Route("generateSharedSecret"), HttpPost]
        [ApiAuthorize]
        [OldStandardArgument("ssoAdapterId", "sso_adapter_id")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.SSOAdapterIdRequired)]
        [Throws(eResponseStatus.SSOAdapaterNotExist)]
        public KalturaSsoAdapterProfile GenerateSharedSecret(int ssoAdapterId)
        {
            KalturaSsoAdapterProfile response = null;
            var groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.UsersClient().GenerateSSOAdapaterSharedSecret(groupId, ssoAdapterId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

    }
}