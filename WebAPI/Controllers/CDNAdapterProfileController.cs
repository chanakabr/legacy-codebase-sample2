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
using WebAPI.Models.API;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/cdnAdapterProfile/action")]
    public class CdnAdapterProfileController : ApiController
    {
        /// <summary>
        /// Returns all CDN adapters for partner
        /// </summary>
        /// <remarks> 
        /// 
        /// </remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaCDNAdapterProfileListResponse List()
        {
            KalturaCDNAdapterProfileListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().GetCDNRAdapters(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Delete CDN adapter by CDN adapter id
        /// </summary>
        /// <remarks>
        /// Possible status codes: AdapterNotExists = 10000, AdapterIdentifierRequired = 10001
        /// </remarks>
        /// <param name="adapterId">CDN adapter identifier</param>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        [OldStandard("adapterId", "adapter_id")]
        [Throws(eResponseStatus.AdapterNotExists)]
        [Throws(eResponseStatus.AdapterIdentifierRequired)]
        public bool Delete(int adapterId)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().DeleteCDNAdapter(groupId, adapterId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Insert new CDN adapter for partner
        /// </summary>
        /// <remarks>
        /// Possible status codes:     
        /// Adapter NameRequired = 5005, AdapterUrlRequired = 5013, SystemNameMustBeUnique = 5019, SystemNameRequired = 5020
        /// </remarks>
        /// <param name="adapter">CDN adapter object</param>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.NameRequired)]
        [Throws(eResponseStatus.AdapterUrlRequired)]
        [Throws(eResponseStatus.AliasMustBeUnique)]
        [Throws(eResponseStatus.AliasRequired)]
        public KalturaCDNAdapterProfile Add(KalturaCDNAdapterProfile adapter)
        {
            KalturaCDNAdapterProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().InsertCDNAdapter(groupId, adapter);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update CDN adapter details
        /// </summary>
        /// <remarks>
        /// Possible status codes: AdapterNotExists = 10000, AdapterIdentifierRequired = 10001, NameRequired = 5005, AdapterUrlRequired = 5013, SystemNameMustBeUnique = 5019, SystemNameRequired = 5020
        /// </remarks>
        /// <param name="adapterId">CDN adapter id to update</param>       
        /// <param name="adapter">CDN adapter Object</param>       
        [Route("update"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.AdapterNotExists)]
        [Throws(eResponseStatus.AdapterIdentifierRequired)]
        [Throws(eResponseStatus.NameRequired)]
        [Throws(eResponseStatus.AdapterUrlRequired)]
        [Throws(eResponseStatus.AliasMustBeUnique)]
        [Throws(eResponseStatus.AliasRequired)]
        public KalturaCDNAdapterProfile Update(int adapterId, KalturaCDNAdapterProfile adapter)
        {
            KalturaCDNAdapterProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().SetCDNAdapter(groupId, adapter, adapterId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Generate CDN adapter shared secret
        /// </summary>
        /// <remarks>
        /// Possible status codes:  
        /// AdapterIdentifierRequired = 10001, AdapterNotExists = 10000
        /// </remarks>
        /// <param name="adapterId">CDN adapter identifier</param>
        [Route("generateSharedSecret"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.AdapterNotExists)]
        [Throws(eResponseStatus.AdapterIdentifierRequired)]
        public KalturaCDNAdapterProfile GenerateSharedSecret(int adapterId)
        {
            KalturaCDNAdapterProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().GenerateCDNSharedSecret(groupId, adapterId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}