using System.Collections.Generic;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.API;
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
        public List<KalturaRecommendationEngineBaseProfile> List()
        {
            List<KalturaRecommendationEngineBaseProfile> response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().GetRecommendationEngines(groupId);
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
        /// recommendation engine not exist = 4007, recommendatio nengine identifier required = 4008,
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
                response = ClientsManager.ApiClient().DeleteRecommendationEngine(groupId, recommendation_engine_id);
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
        /// no recommendation engine to insert = 4006, name required = 5005, adapter url required = 5013, external identifier required = 6016, external identifier must be unique = 6040
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
                response = ClientsManager.ApiClient().InsertRecommendationEngine(groupId, recommendation_engine);
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
        /// no recommendation engine to update = 4007, recommendation engine identifier required = 4008, name required = 5005, adapter url required = 5013,
        /// external identifier required = 6016, external identifier must be unique = 6040
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
                response = ClientsManager.ApiClient().SetRecommendationEngine(groupId, recommendation_engine_id, recommendation_engine);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Generate recommendation engine  shared secret
        /// </summary>
        /// <remarks>
        /// Possible status codes:  
        /// recommendation engine not exist = 4007, recommendation engine identifier required = 4008
        /// </remarks>
        /// <param name="recommendation_engine_id">recommendation engine Identifier</param>
        [Route("generateSharedSecret"), HttpPost]
        [ApiAuthorize]
        public KalturaRecommendationEngineProfile GenerateSharedSecret(int recommendation_engine_id)
        {
            KalturaRecommendationEngineProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().GeneratereRecommendationEngineSharedSecret(groupId, recommendation_engine_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}