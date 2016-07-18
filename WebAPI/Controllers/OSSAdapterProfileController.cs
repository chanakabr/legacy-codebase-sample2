using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Schema;
using WebAPI.Models.API;
using WebAPI.Models.Billing;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/ossAdapterProfile/action")]
    public class OssAdapterProfileController : ApiController
    {
        /// <summary>
        /// Returns all OSS adapters for partner : id + name
        /// </summary>
        /// <remarks>       
        /// </remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public List<KalturaOSSAdapterBaseProfile> List()
        {
            List<KalturaOSSAdapterBaseProfile> response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().GetOSSAdapter(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Delete OSS adapter by OSS adapter id
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        /// oss adapter identifier required = 5007, oss adapter not exist = 5008,  action is not allowed = 5011
        /// </remarks>
        /// <param name="ossAdapterId">OSS adapter identifier</param>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        [OldStandard("ossAdapterId", "oss_adapter_id")]
        public bool Delete(int ossAdapterId)
        {
            bool response = false;
            
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().DeleteOSSAdapter(groupId, ossAdapterId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Insert new OSS adapter for partner
        /// </summary>
        /// <remarks>
        /// Possible status codes:     
        /// no oss adapter to insert = 5004, name required = 5005, adapter url required = 5013, external identifier required = 6016, external identifier must be unique = 6040  
        /// </remarks>
        /// <param name="ossAdapter">OSS adapter Object</param>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        [OldStandard("ossAdapter", "oss_adapter")]
        public KalturaOSSAdapterProfile Add(KalturaOSSAdapterProfile ossAdapter)
        {
            KalturaOSSAdapterProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().InsertOSSAdapter(groupId, ossAdapter);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update OSS adapter details
        /// </summary>
        /// <remarks>
        /// Possible status codes:   
        /// name required = 5005, oss adapter identifier required = 5007, no oss adapter to update = 5012, adapter url required = 5013, external identifier required = 6016
        /// </remarks>
        /// <param name="oss_adapter">OSS adapter Object</param>       
        [Route("update"), HttpPost]
        [ApiAuthorize]
        public KalturaOSSAdapterProfile Update(KalturaOSSAdapterProfile oss_adapter)
        {
            KalturaOSSAdapterProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().SetOSSAdapter(groupId, oss_adapter);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Generate oss adapter shared secret
        /// </summary>
        /// <remarks>
        /// Possible status codes:  
        /// oss adapter identifier required = 5007, oss adapter not exist = 5008
        /// </remarks>
        /// <param name="ossAdapterId">OSS adapter identifier</param>
        [Route("generateSharedSecret"), HttpPost]
        [ApiAuthorize]
        [OldStandard("ossAdapterId", "oss_adapter_id")]
        [ValidationException(SchemaValidationType.ACTION_NAME)]
        public KalturaOSSAdapterProfile GenerateSharedSecret(int ossAdapterId)
        {
            KalturaOSSAdapterProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().GenerateOSSSharedSecret(groupId, ossAdapterId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }


    }
}