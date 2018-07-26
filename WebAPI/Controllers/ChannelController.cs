using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.Models.Pricing;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("channel")]
    public class ChannelController : IKalturaController
    {

        private const string OPC_MERGE_VERSION = "5.0.0.0";

        /// <summary>
        /// Returns channel        
        /// </summary>
        /// <param name="id">Channel Identifier</param>
        /// <remarks></remarks>
        [Action("get")]
        [ApiAuthorize]
        [SchemeArgument("id", MinInteger = 1)]
        [Throws(eResponseStatus.ChannelDoesNotExist)]
        static public KalturaChannel Get(int id)
        {
            KalturaChannel response = null;
            KS ks = KS.GetFromRequest();
            int groupId = ks.GroupId;            

            try
            {
                if (ClientsManager.CatalogClient().DoesGroupUsesTemplates(groupId))
                {
                    bool isAllowedToViewInactiveAssets = Utils.Utils.IsAllowedToViewInactiveAssets(groupId, ks.UserId);
                    response = ClientsManager.CatalogClient().GetChannel(groupId, id, isAllowedToViewInactiveAssets);
                }
                else
                {
                    response = ClientsManager.CatalogClient().GetChannelInfo(groupId, ks.UserId, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), KSUtils.ExtractKSPayload().UDID,
                                                                                Utils.Utils.GetLanguageFromRequest(), id);

                    // if no response - return not found status 
                    if (response == null || response.Id == 0)
                    {
                        throw new NotFoundException(NotFoundException.OBJECT_NOT_FOUND, "Channel");
                    }
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Returns channel info        
        /// </summary>
        /// <param name="id">Channel Identifier</param>
        /// <remarks></remarks>        
        [Action("getOldStandard")]
        [ApiAuthorize]
        [OldStandardAction("get")]
        [Obsolete]
        [SchemeArgument("id", MinInteger = 1)]
        static public KalturaChannel GetOldStandard(int id)
        {
            KalturaChannel response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                string userID = KS.GetFromRequest().UserId;
                string udid = KSUtils.ExtractKSPayload().UDID;
                string language = Utils.Utils.GetLanguageFromRequest();
                response = ClientsManager.CatalogClient().GetChannelInfo(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid, language, id);

                // if no response - return not found status 
                if (response == null || response.Id == 0)
                {
                    throw new NotFoundException(NotFoundException.OBJECT_NOT_FOUND, "Channel");
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }


        /// <summary>
        /// Delete channel by its channel id
        /// </summary>
        /// <remarks>
        /// Possible status codes:  
        /// IdentifierRequired = 4017,
        /// ObjectNotExist = 4018
        /// </remarks>
        /// <param name="channelId">channel identifier</param>
        [Action("delete")]
        [ApiAuthorize]
        [OldStandardArgument("channelId", "channel_id")]
        [Throws(eResponseStatus.IdentifierRequired)]
        [Throws(eResponseStatus.ObjectNotExist)]
        static public bool Delete(int channelId)
        {
            bool response = false;
            long userId = Utils.Utils.GetUserIdFromKs();
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.CatalogClient().DeleteChannel(groupId, channelId, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Insert new channel for partner. Supports KalturaDynamicChannel or KalturaManualChannel
        /// </summary>
        /// <remarks>
        /// Possible status codes:     
        /// NoObjectToInsert = 4019,
        /// NameRequired = 5005,
        /// </remarks>
        /// <param name="channel">KSQL channel Object</param>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.NoObjectToInsert)]
        [Throws(eResponseStatus.NameRequired)]
        [Throws(eResponseStatus.ChannelMetaOrderByIsInvalid)]
        [Throws(eResponseStatus.ChannelSystemNameAlreadyInUse)]
        [Throws(eResponseStatus.AssetDoesNotExist)]
        [Throws(eResponseStatus.AccountIsNotOpcSupported)]
        static public KalturaChannel Add(KalturaChannel channel)
        {
            KalturaChannel response = null;
            long userId = Utils.Utils.GetUserIdFromKs();
            int groupId = KS.GetFromRequest().GroupId;            
            bool isManualChannelOrDynamicChannel = false;

            // KalturaManualChannel or KalturaDynamicChannel                             
            if (channel is KalturaManualChannel || channel is KalturaDynamicChannel)
            {
                if (channel.OrderBy == null)
                {
                    channel.OrderBy = new KalturaChannelOrder() { orderBy = KalturaChannelOrderBy.CREATE_DATE_DESC };
                }

                // Validate channel
                channel.ValidateForInsert();
                isManualChannelOrDynamicChannel = true;
            }

            try
            {
                if (isManualChannelOrDynamicChannel)
                {
                    response = ClientsManager.CatalogClient().InsertChannel(groupId, channel, userId);
                }                
                else
                {
                    Version version = new Version(OPC_MERGE_VERSION);
                    Version requestVersion = OldStandardAttribute.getCurrentRequestVersion();
                    if (requestVersion.CompareTo(version) > 0)
                    {
                        throw new RequestParserException(RequestParserException.ABSTRACT_PARAMETER, "KalturaChannel");
                    }
                    // KalturaChannel (backward compatibility)
                    else if (channel is KalturaChannel)
                    {
                        response = ClientsManager.CatalogClient().InsertKSQLChannel(groupId, channel, userId);
                    }
                }

            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update channel details. Supports KalturaDynamicChannel or KalturaManualChannel
        /// </summary>
        /// <remarks>
        /// Possible status codes:
        /// ObjectNotExist = 4018,
        /// NoObjectToInsert = 4019,
        /// NameRequired = 5005
        /// </remarks>
        /// <param name="id">Channel identifier</param>      
        /// <param name="channel">KSQL channel Object</param>       
        [Action("update")]
        [ApiAuthorize]
        [Throws(eResponseStatus.ObjectNotExist)]
        [Throws(eResponseStatus.NoObjectToInsert)]
        [Throws(eResponseStatus.NameRequired)]
        [Throws(eResponseStatus.AccountIsNotOpcSupported)]
        static public KalturaChannel Update(int id, KalturaChannel channel)
        {
            KalturaChannel response = null;
            long userId = Utils.Utils.GetUserIdFromKs();
            int groupId = KS.GetFromRequest().GroupId;
            bool isManualChannelOrDynamicChannel = false;

            if (channel is KalturaManualChannel || channel is KalturaDynamicChannel)
            {
                isManualChannelOrDynamicChannel = true;
                // Validate channel
                channel.ValidateForUpdate();
            }

            try
            {
                // KalturaManualChannel or KalturaDynamicChannel                             
                if (isManualChannelOrDynamicChannel)
                {
                    response = ClientsManager.CatalogClient().UpdateChannel(groupId, id, channel, userId);
                }
                
                Version version = new Version(OPC_MERGE_VERSION);
                Version requestVersion = OldStandardAttribute.getCurrentRequestVersion();
                if (requestVersion.CompareTo(version) > 0)
                {
                    throw new RequestParserException(RequestParserException.ABSTRACT_PARAMETER, "KalturaChannel");
                }
                // KalturaChannel (backward compatibility)
                else if (channel is KalturaChannel)                
                {
                    channel.Id = id;
                    response = ClientsManager.CatalogClient().SetKSQLChannel(groupId, channel, userId);
                }

            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Insert new channel for partner. Currently supports only KSQL channel
        /// </summary>
        /// <remarks>
        /// Possible status codes:     
        /// NoObjectToInsert = 4019,
        /// NameRequired = 5005,
        /// </remarks>
        /// <param name="channel">KSQL channel Object</param>
        [Action("addOldStandard")]
        [ApiAuthorize]
        [OldStandardAction("add")]
        [Obsolete]
        [Throws(eResponseStatus.NoObjectToInsert)]
        [Throws(eResponseStatus.NameRequired)]
        static public KalturaChannelProfile AddOldStandard(KalturaChannelProfile channel)
        {
            KalturaChannelProfile response = null;
            long userId = Utils.Utils.GetUserIdFromKs();
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.CatalogClient().InsertKSQLChannelProfile(groupId, channel, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update channel details. Currently supports only KSQL channel
        /// </summary>
        /// <remarks>
        /// Possible status codes:
        /// ObjectNotExist = 4018,
        /// NoObjectToInsert = 4019,
        /// NameRequired = 5005
        /// </remarks>
        /// <param name="channel">KSQL channel Object</param>       
        [Action("updateOldStandard")]
        [ApiAuthorize]
        [OldStandardAction("update")]
        [Obsolete]
        [Throws(eResponseStatus.ObjectNotExist)]
        [Throws(eResponseStatus.NoObjectToInsert)]
        [Throws(eResponseStatus.NameRequired)]
        static public KalturaChannelProfile UpdateOldStandard(KalturaChannelProfile channel)
        {
            KalturaChannelProfile response = null;
            long userId = Utils.Utils.GetUserIdFromKs();
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.CatalogClient().SetKSQLChannelProfile(groupId, channel, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Get the list of tags for the partner
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <param name="pager">Page size and index</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [Throws(eResponseStatus.AccountIsNotOpcSupported)]
        static public KalturaChannelListResponse List(KalturaChannelsFilter filter = null, KalturaFilterPager pager = null)
        {
            KalturaChannelListResponse response = null;

            if (pager == null)
            {
                pager = new KalturaFilterPager();
            }

            if (filter == null)
            {
                filter = new KalturaChannelsFilter();
            }

            try
            {                
                // validate filter
                filter.Validate();

                KS ks = KS.GetFromRequest();
                int groupId = ks.GroupId;
                bool isAllowedToViewInactiveAssets = Utils.Utils.IsAllowedToViewInactiveAssets(groupId, ks.UserId);                

                if (filter.MediaIdEqual > 0)
                {
                    response = ClientsManager.CatalogClient().GetChannelsContainingMedia(groupId, filter.MediaIdEqual, pager.getPageIndex(), pager.getPageSize(), filter.OrderBy, isAllowedToViewInactiveAssets);
                }
                else if (filter.IdEqual > 0)
                {
                    // get by id
                    KalturaChannel channel = ClientsManager.CatalogClient().GetChannel(groupId, filter.IdEqual, isAllowedToViewInactiveAssets);
                    response = new KalturaChannelListResponse() { TotalCount = 1, Channels = new List<KalturaChannel>() { channel } };
                }                
                else if (!string.IsNullOrEmpty(filter.NameEqual))
                {
                    //search using ChannelEqual
                    response = ClientsManager.CatalogClient().SearchChannels(groupId, true, filter.NameEqual, null, pager.getPageIndex(), pager.getPageSize(), filter.OrderBy, isAllowedToViewInactiveAssets);
                }
                else
                {
                    //search using ChannelLike
                    response = ClientsManager.CatalogClient().SearchChannels(groupId, false, filter.NameStartsWith, null, pager.getPageIndex(), pager.getPageSize(), filter.OrderBy, isAllowedToViewInactiveAssets);
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }


    }
}