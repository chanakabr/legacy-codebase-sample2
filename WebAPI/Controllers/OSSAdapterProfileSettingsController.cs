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
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/ossAdapterProfileSettings/action")]
    [Obsolete]
    public class OssAdapterProfileSettingsController : ApiController
    {
        /// <summary>
        /// Returns all OSS adapter settings for partner
        /// </summary>
        /// <remarks>       
        /// </remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public List<KalturaOSSAdapterProfile> List()
        {
            List<KalturaOSSAdapterProfile> response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().GetOSSAdapterSettings(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Delete OSS adapter specific settings by settings keys 
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        /// oss adapter identifier required = 5007, oss adapter not exist = 5008, oss adapter params required = 5009, conflicted params = 5014
        /// </remarks>
        /// <param name="oss_adapter_id">OSS adapter Identifier</param>
        /// <param name="settings">Dictionary (string,KalturaStringValue) for partner specific settings: Format Example
        /// "settings": { "key3": {"value": "value3"},
        ///"key1": {"value": "value2"}}
        ///</param>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        public bool Delete(int oss_adapter_id, SerializableDictionary<string, KalturaStringValue> settings)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().DeleteOSSAdapterSettings(groupId, oss_adapter_id, settings);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Insert new settings for OSS adapter for partner
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        /// oss adapter identifier required = 5007, oss adapter not exist = 5008, oss adapter params required = 5009, conflicted params = 5014
        /// </remarks>
        /// <param name="oss_adapter_id">OSS Adapter identifier</param> 
        /// <param name="settings">Dictionary (string,KalturaStringValue) for partner specific settings: Format Example
        /// "settings": { "key3": {"value": "value3"},
        ///"key1": {"value": "value2"}}
        ///</param>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public bool Add(int oss_adapter_id, SerializableDictionary<string, KalturaStringValue> settings)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().InsertOSSAdapterSettings(groupId, oss_adapter_id, settings);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update settings for OSS adapter
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        /// oss adapter identifier required = 5007, oss adapter not exist = 5008, oss adapter params required = 5009,conflicted params = 5014
        /// </remarks>
        /// <param name="oss_adapter_id">OSS Adapter identifier</param> 
        /// <param name="settings">Dictionary (string,KalturaStringValue) for partner specific settings: Format Example
        /// "settings": { "key3": {"value": "value3"},
        ///"key1": {"value": "value2"}}
        ///</param>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        public bool Update(int oss_adapter_id, SerializableDictionary<string, KalturaStringValue> settings)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().SetOSSAdapterSettings(groupId, oss_adapter_id, settings);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}