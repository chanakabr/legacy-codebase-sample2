using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.API;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/cDNAdapterProfile/action")]
    public class CDNAdapterProfileController : ApiController
    {
        /// <summary>
        /// Returns all CDN adapters for partner
        /// </summary>
        /// <remarks> 
        /// 
        /// </remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public List<KalturaCDNAdapterProfile> List()
        {
            List<KalturaCDNAdapterProfile> response = null;

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
        /// Possible status codes: Adapter does not exist = 10000
        /// </remarks>
        /// <param name="adapter_id">CDN adapter identifier</param>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        public bool Delete(int adapter_id)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().DeleteCDNAdapter(groupId, adapter_id);
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
        /// Adapter Name is required = 5005, Adapter URL is required = 5013, Alias must be unique = 5019, Alias is required = 5020
        /// </remarks>
        /// <param name="adapter">CDN adapter object</param>
        [Route("add"), HttpPost]
        [ApiAuthorize]
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
        /// Possible status codes:   
        /// Adapter name is required = 5005, Adapter identifier is required = 10001, Adapter URL is required = 5013, Alias is required = 5020, Adapter does not exist = 10000
        /// </remarks>
        /// <param name="adapter">CDN adapter Object</param>       
        [Route("update"), HttpPost]
        [ApiAuthorize]
        public KalturaCDNAdapterProfile Update(KalturaCDNAdapterProfile adapter)
        {
            KalturaCDNAdapterProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().SetCDNAdapter(groupId, adapter);
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
        /// Adapter identifier is required = 10001, AdapterNotExists = 10000
        /// </remarks>
        /// <param name="adapter_id">CDN adapter identifier</param>
        [Route("generateSharedSecret"), HttpPost]
        [ApiAuthorize]
        public KalturaCDNAdapterProfile GenerateSharedSecret(int adapter_id)
        {
            KalturaCDNAdapterProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().GenerateCDNSharedSecret(groupId, adapter_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}