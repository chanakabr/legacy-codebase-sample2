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
    [Service("ssoAdapterProfile")]
    public class SsoAdapterProfileController : IKalturaController
    {
        /// <summary>
        /// Returns all sso adapters for partner : id + name
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        ///  
        /// </remarks>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaSSOAdapterProfileListResponse List()
        {
            var response = new KalturaSSOAdapterProfileListResponse();
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
        [Action("delete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.SSOAdapterNotExist)]
        static public bool Delete(int ssoAdapterId)
        {
            var ks = KS.GetFromRequest();
            var groupId = ks.GroupId;
            var userId = ks.UserId;

            try
            {
                // call client
                var response = ClientsManager.UsersClient().DeleteSSOAdapater(groupId, ssoAdapterId, int.Parse(userId));
                if (response.Code != (int)eResponseStatus.OK) { throw new ClientException(response.Code); }
                return true;


            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return false;
        }

        /// <summary>
        /// Insert new sso adapter for partner
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        ///   External identifier is required = 6016, Name is required = 5005, Shared secret is required = 5006, External identifier must be unique = 6040, No sso adapter to insert = 2057
        /// </remarks>
        /// <param name="ssoAdapter">SSO Adapter Object to be added</param>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.NameRequired)]
        [Throws(eResponseStatus.SharedSecretRequired)]
        [Throws(eResponseStatus.ExternalIdentifierRequired)]
        [Throws(eResponseStatus.ExternalIdentifierMustBeUnique)]
        [Throws(eResponseStatus.NoSSOAdapaterToInsert)]
        static public KalturaSSOAdapterProfile Add(KalturaSSOAdapterProfile ssoAdapter)
        {
            KalturaSSOAdapterProfile response = null;
            var ks = KS.GetFromRequest();
            var groupId = ks.GroupId;
            var userId = ks.UserId;

            try
            {
                // call client
                response = ClientsManager.UsersClient().InsertSSOAdapter(groupId, ssoAdapter, int.Parse(userId));
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
        /// <param name="ssoAdapter">SSO Adapter Object</param>       
        [Action("update")]
        [ApiAuthorize]
        [Throws(eResponseStatus.ActionIsNotAllowed)]
        [Throws(eResponseStatus.SSOAdapterIdRequired)]
        [Throws(eResponseStatus.NameRequired)]
        [Throws(eResponseStatus.SharedSecretRequired)]
        [Throws(eResponseStatus.ExternalIdentifierRequired)]
        [Throws(eResponseStatus.ExternalIdentifierMustBeUnique)]
        static public KalturaSSOAdapterProfile Update(int ssoAdapterId, KalturaSSOAdapterProfile ssoAdapter)
        {

            var ks = KS.GetFromRequest();
            var groupId = ks.GroupId;
            var userId = ks.UserId;
            try
            {
                // call client
                var response = ClientsManager.UsersClient().SetSSOAdapter(groupId, ssoAdapterId, ssoAdapter, int.Parse(userId));

                return response;
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return null;
        }

        /// <summary>
        /// Generate SSO Adapter shared secret
        /// </summary>
        /// <remarks>
        /// Possible status codes:  
        /// SSO Adapter id required = 2058, sso adapater not exist = 2056
        /// </remarks>
        /// <param name="ssoAdapterId">SSO Adapter identifier</param>
        [Action("generateSharedSecret")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.SSOAdapterIdRequired)]
        [Throws(eResponseStatus.SSOAdapterNotExist)]
        static public KalturaSSOAdapterProfile GenerateSharedSecret(int ssoAdapterId)
        {
            KalturaSSOAdapterProfile response = null;
            var ks = KS.GetFromRequest();
            var groupId = ks.GroupId;
            var userId = ks.UserId;

            try
            {
                // call client
                response = ClientsManager.UsersClient().GenerateSSOAdapaterSharedSecret(groupId, ssoAdapterId, int.Parse(userId));

                if (response == null) { throw new ClientException((int)eResponseStatus.SSOAdapterNotExist); }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

    }
}