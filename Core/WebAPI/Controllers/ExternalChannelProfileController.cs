using ApiObjects.Response;
using System;
using System.Collections.Generic;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("externalChannelProfile")]
    public class ExternalChannelProfileController : IKalturaController
    {
        /// <summary>
        /// Returns all External channels for partner 
        /// </summary>
        /// <param name="filter">External channel profile filter</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaExternalChannelProfileListResponse List(KalturaExternalChannelProfileFilter filter = null)
        {
            KalturaExternalChannelProfileListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();
            try
            {
                if (filter == null)
                {
                    filter = new KalturaExternalChannelProfileFilter();
                }

                filter.Validate();

                if (filter is KalturaExternalChannelProfileByIdInFilter)
                {
                    KalturaExternalChannelProfileByIdInFilter idInFilter = filter as KalturaExternalChannelProfileByIdInFilter;
                    response = ClientsManager.ApiClient().GetExternalChannels(groupId, userId, idInFilter.GetIdIn());
                }
                else
                {
                    var list = ClientsManager.ApiClient().GetExternalChannels(groupId, userId);
                    response = new KalturaExternalChannelProfileListResponse() { Objects = list, TotalCount = list.Count };
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Returns all External channels for partner 
        /// </summary>
        /// <remarks>       
        /// </remarks>
        [Action("listOldStandard")]
        [ApiAuthorize]
        [OldStandardAction("list")]
        [Obsolete]
        static public List<KalturaExternalChannelProfile> ListOldStandard()
        {
            List<KalturaExternalChannelProfile> response = null;

            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            try
            {
                // call client
                response = ClientsManager.ApiClient().GetExternalChannels(groupId, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Delete External channel by External channel id
        /// </summary>
        /// <param name="externalChannelId">External channel identifier</param>
        [Action("delete")]
        [ApiAuthorize]
        [OldStandardArgument("externalChannelId", "external_channel_id")]
        [Throws(eResponseStatus.ExternalChannelNotExist)]
        [Throws(eResponseStatus.ExternalChannelIdentifierRequired)]
        [Throws(eResponseStatus.ActionIsNotAllowed)]
        static public bool Delete(int externalChannelId)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            try
            {
                // call client
                response = ClientsManager.ApiClient().DeleteExternalChannel(groupId, externalChannelId, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Insert new External channel for partner
        /// </summary>
        /// <param name="externalChannel">External channel Object</param>
        [Action("add")]
        [ApiAuthorize]
        [OldStandardArgument("externalChannel", "external_channel")]
        [Throws(eResponseStatus.NoExternalChannelToInsert)]
        [Throws(eResponseStatus.RecommendationEngineNotExist)]
        [Throws(eResponseStatus.RecommendationEngineIdentifierRequired)]
        [Throws(eResponseStatus.InactiveExternalChannelEnrichment)]
        [Throws(eResponseStatus.NameRequired)]
        [Throws(eResponseStatus.ExternalIdentifierRequired)]
        [Throws(eResponseStatus.ExternalIdentifierMustBeUnique)]
        [Throws(eResponseStatus.ActionIsNotAllowed)]
        static public KalturaExternalChannelProfile Add(KalturaExternalChannelProfile externalChannel)
        {
            KalturaExternalChannelProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            try
            {
                // call client
                response = ClientsManager.ApiClient().InsertExternalChannel(groupId, externalChannel, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update External channel details
        /// </summary>
        /// <remarks>
        /// Possible status codes:   
        /// external channel not exist = 4011, external channel identifier required = 4013, inactive external channel enrichment = 4016,
        /// name required = 5005, external identifier required = 6016, external identifier must be unique = 6040  
        /// </remarks>
        /// <param name="externalChannelId">External channel identifier</param>       
        /// <param name="externalChannel">External channel Object</param>       
        [Action("update")]
        [ApiAuthorize]
        [Throws(eResponseStatus.NoExternalChannelToUpdate)]
        [Throws(eResponseStatus.ExternalChannelNotExist)]
        [Throws(eResponseStatus.ExternalChannelIdentifierRequired)]
        [Throws(eResponseStatus.InactiveExternalChannelEnrichment)]
        [Throws(eResponseStatus.NameRequired)]
        [Throws(eResponseStatus.ExternalIdentifierRequired)]
        [Throws(eResponseStatus.ExternalIdentifierMustBeUnique)]
        [Throws(eResponseStatus.ActionIsNotAllowed)]
        static public KalturaExternalChannelProfile Update(int externalChannelId, KalturaExternalChannelProfile externalChannel)
        {
            KalturaExternalChannelProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;
            externalChannel.Id = externalChannelId;
            long userId = Utils.Utils.GetUserIdFromKs();

            try
            {
                // call client
                response = ClientsManager.ApiClient().SetExternalChannel(groupId, externalChannel, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update External channel details
        /// </summary>
        /// <remarks>
        /// Possible status codes:
        /// external channel not exist = 4011, external channel identifier required = 4013, inactive external channel enrichment = 4016,
        /// name required = 5005, external identifier required = 6016, external identifier must be unique = 6040
        /// </remarks>
        /// <param name="external_channel">External channel Object</param>
        [Action("updateOldStandard")]
        [ApiAuthorize]
        [OldStandardAction("update")]
        [Obsolete]
        [Throws(eResponseStatus.ExternalChannelNotExist)]
        [Throws(eResponseStatus.ExternalChannelIdentifierRequired)]
        [Throws(eResponseStatus.InactiveExternalChannelEnrichment)]
        [Throws(eResponseStatus.NameRequired)]
        [Throws(eResponseStatus.ExternalIdentifierRequired)]
        [Throws(eResponseStatus.ExternalIdentifierMustBeUnique)]
        static public KalturaExternalChannelProfile UpdateOldStandard(KalturaExternalChannelProfile external_channel)
        {
            KalturaExternalChannelProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            try
            {
                // call client
                response = ClientsManager.ApiClient().SetExternalChannel(groupId, external_channel, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}