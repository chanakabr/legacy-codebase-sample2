using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("recommendationProfile")]
    public class RecommendationProfileController : IKalturaController
    {
        /// <summary>
        /// Returns all recommendation engines for partner 
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        ///  
        /// </remarks>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaRecommendationProfileListResponse List()
        {
            List<KalturaRecommendationProfile> list = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                list = ClientsManager.ApiClient().GetRecommendationEngines(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaRecommendationProfileListResponse() { RecommendationProfiles = list, TotalCount = list.Count };
        }

        /// <summary>
        /// Returns all recommendation engines for partner 
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        ///  
        /// </remarks>
        [Action("listOldStandard")]
        [ApiAuthorize]
        [OldStandardAction("list")]
        [Obsolete]
        static public List<KalturaRecommendationProfile> ListOldStandard()
        {
            List<KalturaRecommendationProfile> response = null;

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
        /// recommendation engine not exist = 4007, recommendation engine identifier required = 4008
        /// </remarks>
        /// <param name="id">recommendation engine Identifier</param>
        [Action("delete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.RecommendationEngineNotExist)]
        [Throws(eResponseStatus.RecommendationEngineIdentifierRequired)]
        static public bool Delete(int id)
        {
            bool response = false;
            
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().DeleteRecommendationEngine(groupId, id);
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
        /// name required = 5005, adapter url required = 5013, external identifier required = 6016, external identifier must be unique = 6040
        /// </remarks>
        /// <param name="recommendationEngine">recommendation engine Object</param>
        [Action("add")]
        [ApiAuthorize]
        [OldStandardArgument("recommendationEngine", "recommendation_engine")]
        [Throws(eResponseStatus.NameRequired)]
        [Throws(eResponseStatus.AdapterUrlRequired)]
        [Throws(eResponseStatus.ExternalIdentifierRequired)]
        [Throws(eResponseStatus.ExternalIdentifierMustBeUnique)]
        static public KalturaRecommendationProfile Add(KalturaRecommendationProfile recommendationEngine)
        {
            KalturaRecommendationProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().InsertRecommendationEngine(groupId, recommendationEngine);
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
        /// recommendation engine not exist = 4007, recommendation engine identifier required = 4008, name required = 5005, 
        /// adapter url required = 5013, external identifier required = 6016, external identifier must be unique = 6040
        /// </remarks>        
        /// <param name="recommendationEngineId">recommendation engine identifier</param>    
        /// <param name="recommendationEngine">recommendation engine Object</param>       
        [Action("update")]
        [ApiAuthorize]
        [Throws(eResponseStatus.RecommendationEngineNotExist)]
        [Throws(eResponseStatus.RecommendationEngineIdentifierRequired)]
        [Throws(eResponseStatus.NameRequired)]
        [Throws(eResponseStatus.AdapterUrlRequired)]
        [Throws(eResponseStatus.ExternalIdentifierRequired)]
        [Throws(eResponseStatus.ExternalIdentifierMustBeUnique)]
        static public KalturaRecommendationProfile Update(int recommendationEngineId, KalturaRecommendationProfile recommendationEngine)
        {
            KalturaRecommendationProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().SetRecommendationEngine(groupId, recommendationEngineId, recommendationEngine);
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
        /// recommendation engine not exist = 4007, recommendation engine identifier required = 4008, name required = 5005, 
        /// adapter url required = 5013, external identifier required = 6016, external identifier must be unique = 6040
        /// </remarks>        
        /// <param name="recommendation_engine">recommendation engine Object</param>       
        [Action("updateOldStandard")]
        [OldStandardAction("update")]
        [ApiAuthorize]
        [Obsolete]
        [Throws(eResponseStatus.RecommendationEngineNotExist)]
        [Throws(eResponseStatus.RecommendationEngineIdentifierRequired)]
        [Throws(eResponseStatus.NameRequired)]
        [Throws(eResponseStatus.AdapterUrlRequired)]
        [Throws(eResponseStatus.ExternalIdentifierRequired)]
        [Throws(eResponseStatus.ExternalIdentifierMustBeUnique)]
        static public KalturaRecommendationProfile UpdateOldStandard(KalturaRecommendationProfile recommendation_engine)
        {
            KalturaRecommendationProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().SetRecommendationEngine(groupId, recommendation_engine.getId(), recommendation_engine);
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
        /// <param name="recommendationEngineId">recommendation engine Identifier</param>
        [Action("generateSharedSecret")]
        [ApiAuthorize]
        [OldStandardArgument("recommendationEngineId", "recommendation_engine_id")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.RecommendationEngineNotExist)]
        [Throws(eResponseStatus.RecommendationEngineIdentifierRequired)]
        static public KalturaRecommendationProfile GenerateSharedSecret(int recommendationEngineId)
        {
            KalturaRecommendationProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().GeneratereRecommendationEngineSharedSecret(groupId, recommendationEngineId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}