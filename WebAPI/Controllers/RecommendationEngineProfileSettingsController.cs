using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Billing;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/RecommendationEngineProfileSettings/action")]
    public class RecommendationEngineProfileSettingsController : ApiController
    {
        /// <summary>
        /// Returns all recommendation engine settings for partner
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        ///  
        /// </remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public List<KalturaRecommendationEngineProfile> List()
        {
            List<KalturaRecommendationEngineProfile> response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.CatalogClient().GetRecommendationEngineSettings(groupId);
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
        ///  
        /// </remarks>
        /// <param name="recommendation_engine_id">recommendation engine Identifier</param>
        /// <param name="settings">Dictionary (string,KalturaStringValue) for partner specific settings: Format Example
        /// "settings": { "key3": {"value": "value3"},
        ///"key1": {"value": "value2"}}
        ///</param>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        public bool Delete(int recommendation_engine_id, SerializableDictionary<string, KalturaStringValue> settings)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.CatalogClient().DeleteRecommendationEngineSettings(groupId, recommendation_engine_id, settings);
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
        /// </remarks>
        /// <param name="recommendation_engine_id">recommendation engine Identifier</param> 
        /// <param name="settings">Dictionary (string,KalturaStringValue) for partner specific settings: Format Example
        /// "settings": { "key3": {"value": "value3"},
        ///"key1": {"value": "value2"}}
        ///</param>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public bool Add(int recommendation_engine_id, SerializableDictionary<string, KalturaStringValue> settings)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.CatalogClient().InsertRecommendationEngineSettings(groupId, recommendation_engine_id, settings);
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
        ///  
        /// </remarks>
        /// <param name="recommendation_engine_id">recommendation engine Identifier</param> 
        /// <param name="settings">Dictionary (string,KalturaStringValue) for partner specific settings: Format Example
        /// "settings": { "key3": {"value": "value3"},
        ///"key1": {"value": "value2"}}
        ///</param>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        public bool Update(int recommendation_engine_id, SerializableDictionary<string, KalturaStringValue> settings)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.CatalogClient().SetRecommendationEngineSettings(groupId, recommendation_engine_id, settings);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}