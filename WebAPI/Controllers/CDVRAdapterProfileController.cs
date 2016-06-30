using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Schema;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/cDVRAdapterProfile/action")]
    public class CDVRAdapterProfileController : ApiController
    {
        /// <summary>
        /// Returns all C-DVR adapters for partner
        /// </summary>
        /// <remarks> 
        /// 
        /// </remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public List<KalturaCDVRAdapterProfile> List()
        {
            List<KalturaCDVRAdapterProfile> response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().GetCDVRAdapters(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Delete C-DVR adapter by C-DVR adapter id
        /// </summary>
        /// <remarks>
        /// Possible status codes: Adapter does not exist = 10000
        /// </remarks>
        /// <param name="adapterId">C-DVR adapter identifier</param>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        [OldStandard("adapterId", "adapter_id")]
        public bool Delete(int adapterId)
        {
            bool response = false;
            
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().DeleteCDVRAdapter(groupId, adapterId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Insert new C-DVR adapter for partner
        /// </summary>
        /// <remarks>
        /// Possible status codes:     
        /// Adapter Name is required = 5005, Adapter URL is required = 5013, External identifier is required = 6016, External identifier must be unique = 6040
        /// </remarks>
        /// <param name="adapter">C-DVR adapter object</param>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public KalturaCDVRAdapterProfile Add(KalturaCDVRAdapterProfile adapter)
        {
            KalturaCDVRAdapterProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().InsertCDVRAdapter(groupId, adapter);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update C-DVR adapter details
        /// </summary>
        /// <remarks>
        /// Possible status codes:   
        /// Adapter name is required = 5005, Adapter identifier is required = 10001, Adapter URL is required = 5013, External identifier required = 6016, Adapter does not exist = 10000
        /// </remarks>
        /// <param name="adapter">C-DVR adapter Object</param>       
        [Route("update"), HttpPost]
        [ApiAuthorize]
        public KalturaCDVRAdapterProfile Update(KalturaCDVRAdapterProfile adapter)
        {
            KalturaCDVRAdapterProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().SetCDVRAdapter(groupId, adapter);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Generate C-DVR adapter shared secret
        /// </summary>
        /// <remarks>
        /// Possible status codes:  
        /// Adapter identifier is required = 10001, AdapterNotExists = 10000
        /// </remarks>
        /// <param name="adapterId">C-DVR adapter identifier</param>
        [Route("generateSharedSecret"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemaValidationType.ACTION_NAME)]
        [OldStandard("adapterId", "adapter_id")]
        public KalturaCDVRAdapterProfile GenerateSharedSecret(int adapterId)
        {
            KalturaCDVRAdapterProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().GenerateCDVRSharedSecret(groupId, adapterId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }


    }
}