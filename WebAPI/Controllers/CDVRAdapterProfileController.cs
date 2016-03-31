using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/CDVRAdapterProfile/action")]
    public class CDVRAdapterProfileController : ApiController
    {
        /// <summary>
        /// Returns all C-DVR adapters for partner
        /// </summary>
        /// <remarks>       
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
        /// Possible status codes:       
        /// C-DVR adapter identifier required = 5007, C-DVR adapter not exist = 5008,  action is not allowed = 5011
        /// </remarks>
        /// <param name="adapter_id">C-DVR adapter identifier</param>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        public bool Delete(int adapter_id)
        {
            bool response = false;
            
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().DeleteCDVRAdapter(groupId, adapter_id);
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
        /// no oss adapter to insert = 5004, name required = 5005, adapter url required = 5013, external identifier required = 6016, external identifier must be unique = 6040  
        /// </remarks>
        /// <param name="adapter">OSS adapter Object</param>
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
        /// name required = 5005, oss adapter identifier required = 5007, no oss adapter to update = 5012, adapter url required = 5013, external identifier required = 6016
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
        /// oss adapter identifier required = 5007, oss adapter not exist = 5008
        /// </remarks>
        /// <param name="adapter_id">C-DVR adapter identifier</param>
        [Route("generateSharedSecret"), HttpPost]
        [ApiAuthorize]
        public KalturaCDVRAdapterProfile GenerateSharedSecret(int adapter_id)
        {
            KalturaCDVRAdapterProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().GenerateCDVRSharedSecret(groupId, adapter_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }


    }
}