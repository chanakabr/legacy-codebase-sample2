using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.API;
using WebAPI.Models.Billing;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/OSSAdapterProfile/action")]
    public class OSSAdapterProfileController : ApiController
    {
        /// <summary>
        /// Returns all OSS adapters for partner : id + name
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        ///  
        /// </remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
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
        /// </remarks>
        /// <param name="oss_adapter_id">OSS adapter identifier</param>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        public bool Delete(int oss_adapter_id)
        {
            bool response = false;
            
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().DeleteOSSAdapter(groupId, oss_adapter_id);
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
        /// </remarks>
        /// <param name="oss_adapter">OSS adapter Object</param>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public bool Add(KalturaOSSAdapterProfile oss_adapter)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().InsertOSSAdapter(groupId, oss_adapter);
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
        /// </remarks>
        /// <param name="oss_adapter_id">OSS adapter Identifier</param> 
        /// <param name="oss_adapter">OSS adapter Object</param>       
        [Route("update"), HttpPost]
        [ApiAuthorize]
        public bool Update(int oss_adapter_id, KalturaOSSAdapterProfile oss_adapter)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().SetOSSAdapter(groupId, oss_adapter_id, oss_adapter);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}