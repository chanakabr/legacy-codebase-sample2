using System;
using System.Collections.Generic;
using System.Linq;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Models.Catalog;
using WebAPI.Models.Catalog.Ordering;
using WebAPI.Models.General;
using WebAPI.ModelsValidators;
using WebAPI.ObjectsConvertor.Extensions;
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
        [Throws(eResponseStatus.AccountIsNotOpcSupported)]
        [Throws(eResponseStatus.ActionIsNotAllowed)]
        [Throws(StatusCode.NotFound)]
        [Throws(eResponseStatus.ObjectNotExist)]
        static public KalturaChannel Get(int id)
        {
            KalturaChannel response = null;
            var contextData = KS.GetContextData();

            try
            {
                if (Utils.Utils.DoesGroupUsesTemplates(contextData.GroupId))
                {
                    bool isAllowedToViewInactiveAssets = Utils.Utils.IsAllowedToViewInactiveAssets(contextData.GroupId, contextData.UserId.ToString());
                    response = ClientsManager.CatalogClient().GetChannel(contextData, id, isAllowedToViewInactiveAssets);
                }
                else
                {
                    response = ClientsManager.CatalogClient().GetChannelInfo(contextData, id);

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

            var contextData = KS.GetContextData();

            try
            {
                response = ClientsManager.CatalogClient().GetChannelInfo(contextData, id);

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
        [Throws(eResponseStatus.AccountIsNotOpcSupported)]
        [Throws(eResponseStatus.ActionIsNotAllowed)]
        [Throws(StatusCode.NotFound)]
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
        [Throws(eResponseStatus.InvalidMediaType)]
        [Throws(eResponseStatus.AssetStructDoesNotExist)]
        [Throws(eResponseStatus.AccountIsNotOpcSupported)]
        [Throws(eResponseStatus.SyntaxError)]
        [Throws(eResponseStatus.ActionIsNotAllowed)]
        public static KalturaChannel Add(KalturaChannel channel)
        {
            var userId = Utils.Utils.GetUserIdFromKs();
            var groupId = KS.GetFromRequest().GroupId;
            var isManualChannelOrDynamicChannel = channel is KalturaManualChannel || channel is KalturaDynamicChannel;
            if (isManualChannelOrDynamicChannel)
            {
                BuildOrderingsForInsert(channel);
                channel.ValidateForInsert();
            }

            KalturaChannel response = null;
            if (isManualChannelOrDynamicChannel)
            {
                var searchContext = Utils.Utils.GetUserSearchContext();
                response = ClientsManager.CatalogClient().InsertChannel(groupId, channel, searchContext);
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
        [OldStandardArgument("id", "channelId", sinceVersion = OPC_MERGE_VERSION)]
        [Throws(eResponseStatus.ObjectNotExist)]
        [Throws(eResponseStatus.NoObjectToInsert)]
        [Throws(eResponseStatus.ChannelDoesNotExist)]
        [Throws(eResponseStatus.ChannelSystemNameAlreadyInUse)]
        [Throws(eResponseStatus.ChannelMetaOrderByIsInvalid)]
        [Throws(eResponseStatus.AssetStructDoesNotExist)]
        [Throws(eResponseStatus.AssetDoesNotExist)]
        [Throws(eResponseStatus.InvalidMediaType)]
        [Throws(eResponseStatus.SyntaxError)]
        [Throws(eResponseStatus.AccountIsNotOpcSupported)]
        [Throws(eResponseStatus.ActionIsNotAllowed)]
        public static KalturaChannel Update(int id, KalturaChannel channel)
        {
            var userId = Utils.Utils.GetUserIdFromKs();
            var groupId = KS.GetFromRequest().GroupId;
            var isManualChannelOrDynamicChannel = channel is KalturaManualChannel || channel is KalturaDynamicChannel;
            if (isManualChannelOrDynamicChannel)
            {
                BuildOrderingsForUpdate(channel);
                channel.ValidateForUpdate();
            }

            channel.FillEmptyFieldsForUpdate();

            KalturaChannel response = null;
            if (isManualChannelOrDynamicChannel)
            {
                var searchContext = Utils.Utils.GetUserSearchContext();
                response = ClientsManager.CatalogClient().UpdateChannel(groupId, id, channel, searchContext);
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
                    channel.Id = id;
                    response = ClientsManager.CatalogClient().SetKSQLChannel(groupId, channel, userId);
                }
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
        [Throws(eResponseStatus.ElasticSearchReturnedDeleteItem)]
        [Throws(eResponseStatus.ActionIsNotAllowed)]
        [Throws(eResponseStatus.ChannelDoesNotExist)]
        [Throws(eResponseStatus.ObjectNotExist)]
        [Throws(eResponseStatus.AssetStructDoesNotExist)]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        static public KalturaChannelListResponse List(KalturaChannelsBaseFilter filter = null, KalturaFilterPager pager = null)
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

            filter.Validate();

            try
            {
                var contextData = KS.GetContextData();

                int groupId = KS.GetFromRequest().GroupId;
                bool isAllowedToViewInactiveAssets = Utils.Utils.IsAllowedToViewInactiveAssets(contextData.GroupId, contextData.UserId.ToString());

                response = filter.GetChannels(contextData, isAllowedToViewInactiveAssets, pager);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
        
        private static void BuildOrderingsForInsert(KalturaChannel channel)
        {
            BuildOrderings(channel,true);
        }

        private static void BuildOrderingsForUpdate(KalturaChannel channel)
        {
            BuildOrderings(channel, channel.OrderBy != null);
        }

        private static void BuildOrderings(KalturaChannel channel, bool isOrderingParametersRequired)
        {
            if (channel.OrderingParameters == null)
            {
                channel.OrderingParameters = new List<KalturaBaseChannelOrder>();
            }

            if (channel.OrderingParameters.Any() && channel.OrderBy != null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "orderingParametersEqual", "orderBy");
            }

            if (!channel.OrderingParameters.Any() && isOrderingParametersRequired)
            {
                var orderBy = channel.OrderBy ?? new KalturaChannelOrder { orderBy = KalturaChannelOrderBy.CREATE_DATE_DESC };
                var assetOrder = CreateBaseChannelOrder(channel, orderBy);
                channel.OrderingParameters.Add(assetOrder);
            }
        }

        private static KalturaBaseChannelOrder CreateBaseChannelOrder(KalturaChannel channel, KalturaChannelOrder order)
        {
            if (order.DynamicOrderBy != null)
            {
                var metaTagOrderBy = channel.OrderBy.DynamicOrderBy.OrderBy ?? KalturaMetaTagOrderBy.META_ASC;
                return new KalturaChannelDynamicOrder { Name = channel.OrderBy.DynamicOrderBy.Name, OrderBy = metaTagOrderBy };
            }

            var slidingWindowPeriod = order.SlidingWindowPeriod ?? 0;
            switch (order.orderBy)
            {
                case KalturaChannelOrderBy.NAME_ASC:
                    return new KalturaChannelFieldOrder { OrderBy = KalturaChannelFieldOrderByType.NAME_ASC };
                case KalturaChannelOrderBy.NAME_DESC:
                    return new KalturaChannelFieldOrder { OrderBy = KalturaChannelFieldOrderByType.NAME_DESC };
                case KalturaChannelOrderBy.START_DATE_ASC:
                    return new KalturaChannelFieldOrder { OrderBy = KalturaChannelFieldOrderByType.START_DATE_ASC };
                case KalturaChannelOrderBy.START_DATE_DESC:
                    return new KalturaChannelFieldOrder { OrderBy = KalturaChannelFieldOrderByType.START_DATE_DESC };
                case KalturaChannelOrderBy.CREATE_DATE_ASC:
                    return new KalturaChannelFieldOrder { OrderBy = KalturaChannelFieldOrderByType.CREATE_DATE_ASC };
                case KalturaChannelOrderBy.CREATE_DATE_DESC:
                    return new KalturaChannelFieldOrder { OrderBy = KalturaChannelFieldOrderByType.CREATE_DATE_DESC };
                case KalturaChannelOrderBy.RELEVANCY_DESC:
                    return new KalturaChannelFieldOrder { OrderBy = KalturaChannelFieldOrderByType.RELEVANCY_DESC };
                case KalturaChannelOrderBy.ORDER_NUM:
                    return new KalturaChannelFieldOrder { OrderBy = KalturaChannelFieldOrderByType.ORDER_NUM };
                case KalturaChannelOrderBy.LIKES_DESC:
                    return new KalturaChannelSlidingWindowOrder { OrderBy = KalturaChannelSlidingWindowOrderByType.LIKES_DESC, SlidingWindowPeriod = slidingWindowPeriod };
                case KalturaChannelOrderBy.VOTES_DESC:
                    return new KalturaChannelSlidingWindowOrder { OrderBy = KalturaChannelSlidingWindowOrderByType.VOTES_DESC, SlidingWindowPeriod = slidingWindowPeriod };
                case KalturaChannelOrderBy.RATINGS_DESC:
                    return new KalturaChannelSlidingWindowOrder { OrderBy = KalturaChannelSlidingWindowOrderByType.RATINGS_DESC, SlidingWindowPeriod = slidingWindowPeriod };
                case KalturaChannelOrderBy.VIEWS_DESC:
                    return new KalturaChannelSlidingWindowOrder { OrderBy = KalturaChannelSlidingWindowOrderByType.VIEWS_DESC, SlidingWindowPeriod = slidingWindowPeriod };
                default:
                    throw new BadRequestException(BadRequestException.ARGUMENT_ENUM_VALUE_NOT_SUPPORTED, order.orderBy, "orderBy.orderBy");
            }
        }
    }
}