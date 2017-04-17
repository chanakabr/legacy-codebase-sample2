using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Notification;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/engagementAdapterProfileSettings/action")]
    [Obsolete]
    public class EngagementAdapterProfileSettingsController : ApiController
    {
        /// <summary>
        /// Returns all Engagement adapter settings for partner
        /// </summary>
        /// <remarks>       
        /// </remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public List<KalturaEngagementAdapter> List()
        {
            List<KalturaEngagementAdapter> response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.NotificationClient().GetEngagementAdapterSettings(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Delete Engagement adapter specific settings by settings keys 
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        /// engagement adapter identifier required = 8025, engagement adapter not exist = 8026, engagement adapter params required = 8027, conflicted params = 5014
        /// </remarks>
        /// <param name="engagementAdapterId">Engagement adapter Identifier</param>
        /// <param name="settings">Dictionary (string,KalturaStringValue) for partner specific settings: Format Example
        /// "settings": { "key3": {"value": "value3"},
        ///"key1": {"value": "value2"}}
        ///</param>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.EngagementAdapterIdentifierRequired)]
        [Throws(eResponseStatus.EngagementAdapterNotExist)]
        [Throws(eResponseStatus.EngagementAdapterParamsRequired)]
        [Throws(eResponseStatus.ConflictedParams)]
        public bool Delete(int engagementAdapterId, SerializableDictionary<string, KalturaStringValue> settings)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.NotificationClient().DeleteEngagementAdapterSettings(groupId, engagementAdapterId, settings);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Insert new settings for Engagement adapter for partner
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        /// engagement adapter identifier required = 8025, engagement adapter not exist = 8026, engagement adapter params required = 8027, conflicted params = 5014
        /// </remarks>
        /// <param name="engagementAdapterId">Engagement Adapter identifier</param> 
        /// <param name="settings">Dictionary (string,KalturaStringValue) for partner specific settings: Format Example
        /// "settings": { "key3": {"value": "value3"},
        ///"key1": {"value": "value2"}}
        ///</param>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.EngagementAdapterIdentifierRequired)]
        [Throws(eResponseStatus.EngagementAdapterNotExist)]
        [Throws(eResponseStatus.EngagementAdapterParamsRequired)]
        [Throws(eResponseStatus.ConflictedParams)]
        public bool Add(int engagementAdapterId, SerializableDictionary<string, KalturaStringValue> settings)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.NotificationClient().InsertEngagementAdapterSettings(groupId, engagementAdapterId, settings);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update settings for Engagement adapter
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        /// engagement adapter identifier required = 8025, engagement adapter not exist = 8026, engagement adapter params required = 8027,conflicted params = 5014
        /// </remarks>
        /// <param name="engagementAdapterId">Engagement Adapter identifier</param> 
        /// <param name="settings">Dictionary (string,KalturaStringValue) for partner specific settings: Format Example
        /// "settings": { "key3": {"value": "value3"},
        ///"key1": {"value": "value2"}}
        ///</param>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.EngagementAdapterIdentifierRequired)]
        [Throws(eResponseStatus.EngagementAdapterNotExist)]
        [Throws(eResponseStatus.EngagementAdapterParamsRequired)]
        [Throws(eResponseStatus.ConflictedParams)]
        public bool Update(int engagementAdapterId, SerializableDictionary<string, KalturaStringValue> settings)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.NotificationClient().SetEngagementAdapterSettings(groupId, engagementAdapterId, settings);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}