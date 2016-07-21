using System;
using System.Collections.Generic;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.API;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/recommendationProfileSettings/action")]
    [Obsolete]
    public class RecommendationProfileSettingsController : ApiController
    {
        /// <summary>
        /// Returns all recommendation engine settings for partner
        /// </summary>
        /// <remarks>
        /// </remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public List<KalturaRecommendationProfile> List()
        {
            List<KalturaRecommendationProfile> response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().GetRecommendationEngineSettings(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Delete recommendation engine specific settings by settings keys 
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        /// recommendation engine not exist = 4007, recommendation engine identifier required = 4008, conflicted params = 5014
        /// </remarks>
        /// <param name="id">recommendation engine Identifier</param>
        /// <param name="settings">Dictionary (string,KalturaStringValue) for partner specific settings: Format Example
        /// "settings": { "key3": {"value": "value3"},
        ///"key1": {"value": "value2"}}
        ///</param>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        public bool Delete(int id, SerializableDictionary<string, KalturaStringValue> settings)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().DeleteRecommendationEngineSettings(groupId, id, settings);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Insert new settings for recommendation engine for partner
        /// </summary>
        /// <remarks>
        /// Possible status codes:  
        /// recommendation engine not exist = 4007, conflicted params = 5014
        /// </remarks>
        /// <param name="id">recommendation engine Identifier</param> 
        /// <param name="settings">Dictionary (string,KalturaStringValue) for partner specific settings: Format Example
        /// "settings": { "key3": {"value": "value3"},
        ///"key1": {"value": "value2"}}
        ///</param>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public bool Add(int id, SerializableDictionary<string, KalturaStringValue> settings)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().InsertRecommendationEngineSettings(groupId, id, settings);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update settings for recommendation engine 
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        /// recommendation engine not exist = 4007, conflicted params = 5014
        /// </remarks>
        /// <param name="id">recommendation engine Identifier</param> 
        /// <param name="settings">Dictionary (string,KalturaStringValue) for partner specific settings: Format Example
        /// "settings": { "key3": {"value": "value3"},
        ///"key1": {"value": "value2"}}
        ///</param>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        public bool Update(int id, SerializableDictionary<string, KalturaStringValue> settings)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().SetRecommendationEngineSettings(groupId, id, settings);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}