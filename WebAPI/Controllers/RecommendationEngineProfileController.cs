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
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/RecommendationEngineProfile/action")]
    public class RecommendationEngineProfileController : ApiController
    {
        /// <summary>
        /// Returns all recommendation engines for partner : id + name
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        ///  
        /// </remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public List<Models.Catalog.KalturaRecommendationEngineBaseProfile> List()
        {
            List<Models.Catalog.KalturaRecommendationEngineBaseProfile> response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.CatalogClient().GetRecommendationEngines(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Delete recommendation engine by recommendation engine id
        /// </summary>
        /// <remarks>
        /// Possible status codes:               
        /// </remarks>
        /// <param name="recommendation_engine_id">recommendation engine Identifier</param>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        public bool Delete(int recommendation_engine_id)
        {
            bool response = false;
            
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.CatalogClient().DeleteRecommendationEngine(groupId, recommendation_engine_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Insert new recommendation engine for partner
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        /// </remarks>
        /// <param name="recommendation_engine">recommendation engine Object</param>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public KalturaRecommendationEngineProfile Add(KalturaRecommendationEngineProfile recommendation_engine)
        {
            KalturaRecommendationEngineProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.CatalogClient().InsertRecommendationEngine(groupId, recommendation_engine);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update recommendation engine details
        /// </summary>
        /// <remarks>
        /// Possible status codes:      
        /// </remarks>
        /// <param name="recommendation_engine_id">recommendation engine Identifier</param> 
        /// <param name="recommendation_engine">recommendation engine Object</param>       
        [Route("update"), HttpPost]
        [ApiAuthorize]
        public KalturaRecommendationEngineProfile Update(int recommendation_engine_id, KalturaRecommendationEngineProfile recommendation_engine)
        {
            KalturaRecommendationEngineProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.CatalogClient().SetRecommendationEngine(groupId, recommendation_engine_id, recommendation_engine);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}