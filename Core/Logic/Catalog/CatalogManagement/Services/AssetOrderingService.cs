using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiLogic.Catalog.CatalogManagement.Models;
using ApiLogic.Catalog.CatalogManagement.Services.GroupRepresentatives;
using ApiLogic.IndexManager.Helpers;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using ApiObjects.SearchObjects.GroupRepresentatives;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Core.Catalog.Request;
using ElasticSearch.Searcher;
using GroupsCacheManager;
using ManualMedia = GroupsCacheManager.ManualMedia;

namespace ApiLogic.Catalog.CatalogManagement.Services
{
    public class AssetOrderingService : IAssetOrderingService
    {
        private static readonly Lazy<IAssetOrderingService> LazyInstance = new Lazy<IAssetOrderingService>(
            () => new AssetOrderingService(CatalogManager.Instance, GroupRepresentativesExtendedRequestMapper.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        public static IAssetOrderingService Instance => LazyInstance.Value;

        private readonly ICatalogManager _catalogManager;
        private readonly IGroupRepresentativesExtendedRequestMapper _requestMapper;

        public AssetOrderingService(ICatalogManager catalogManager, IGroupRepresentativesExtendedRequestMapper requestMapper)
        {
            _catalogManager = catalogManager ?? throw new ArgumentNullException(nameof(catalogManager));
            _requestMapper = requestMapper ?? throw new ArgumentNullException(nameof(requestMapper));
        }

        public ChannelEsOrderingResult MapToChannelEsOrderByFields(
            InternalChannelRequest request,
            Channel channel,
            AssetListEsOrderingCommonInput input)
        {
            var (orderingParameters, sourceOrder) = GetResolvedChannelOrdering(request, channel);
            var shouldSortById = orderingParameters?.Count == 1 && orderingParameters.Single().Field == OrderBy.ID
                || sourceOrder?.m_eOrderBy == OrderBy.ID;

            return channel.m_nChannelTypeID == (int)ChannelType.Manual && shouldSortById
                ? GetOrderingResultForManualChannel(channel, sourceOrder, orderingParameters, input)
                : GetOrderingResult(sourceOrder, orderingParameters, input);
        }

        public AssetListEsOrderingResult MapToEsOrderByFields(
            OrderObj order,
            IReadOnlyCollection<AssetOrder> orderings,
            AssetListEsOrderingCommonInput input)
            => orderings?.Count > 0 ? MapToEsOrderByFields(orderings, input) : MapToEsOrderByFields(order, input);

        public AssetListEsOrderingResult MapToEsOrderByFields(
            MediaRelatedRequest request,
            AssetListEsOrderingCommonInput input)
        {
            if (request.OrderObj == null
                && (request.OrderingParameters == null || !request.OrderingParameters.Any())
                && request.m_nMediaID > 0)
            {
                return new AssetListEsOrderingResult
                {
                    Order = new OrderObj { m_eOrderBy = OrderBy.RELATED, m_eOrderDir = OrderDir.DESC },
                    EsOrderByFields = new List<IEsOrderByField>
                    {
                        new EsOrderByField(OrderBy.RELATED, OrderDir.DESC)
                    }
                };
            }

            return MapToEsOrderByFields(request.OrderObj, request.OrderingParameters, input);
        }

        public GenericResponse<IEsOrderByField> MapToEsOrderByField(GroupRepresentativesRequest request, CatalogClientData clientData)
        {
            BooleanPhraseNode filterTree = null;
            var filterQueryParseStatus = BooleanPhraseNode.ParseSearchExpression(request.Filter, ref filterTree);
            if (!filterQueryParseStatus.IsOkStatusCode())
            {
                return new GenericResponse<IEsOrderByField>(filterQueryParseStatus);
            }

            var definitions = BuildUnifiedSearchDefinitions(request, clientData, filterTree);
            var input = new AssetListEsOrderingCommonInput
            {
                GroupId = definitions.groupId,
                ShouldSearchEpg = definitions.shouldSearchEpg,
                ShouldSearchMedia = definitions.shouldSearchRecordings,
                ShouldSearchRecordings = definitions.shouldSearchRecordings,
                AssociationTags = definitions.associationTags,
                ParentMediaTypes = definitions.parentMediaTypes,
                Language = definitions.langauge
            };

            var esOrderByFieldResult = MapToEsOrderByFields(request.OrderingParameters, input);

            return new GenericResponse<IEsOrderByField>(Status.Ok, esOrderByFieldResult.EsOrderByFields.Single());
        }

        private UnifiedSearchDefinitions BuildUnifiedSearchDefinitions(
            GroupRepresentativesRequest request,
            CatalogClientData clientData,
            BooleanPhraseNode filterTree)
        {
            var unifiedSearchRequest = _requestMapper.BuildRequest(request, clientData, filterTree);

            return CatalogLogic.BuildUnifiedSearchObject(unifiedSearchRequest);
        }

        public AssetListEsOrderingResult MapToEsOrderByFields(
            OrderObj source,
            AssetListEsOrderingCommonInput input)
        {
            var order = MapToOrderObject(source, input);

            return new AssetListEsOrderingResult
            {
                EsOrderByFields = new List<IEsOrderByField> { MapToEsOrderByField(order, input) },
                Order = order
            };
        }

        private ChannelEsOrderingResult GetOrderingResultForManualChannel(
            Channel channel,
            OrderObj sourceOrder,
            IEnumerable<AssetOrder> sourceOrderingParameters,
            AssetListEsOrderingCommonInput input)
        {
            var orderingDirection = sourceOrderingParameters?.SingleOrDefault()?.Direction ?? sourceOrder.m_eOrderDir;
            var specificOrder = BuildSpecificOrdering(channel, orderingDirection);
            var orderingParameters = new List<AssetOrder>();
            if (channel.SupportSegmentBasedOrdering)
            {
                // order by score at first
                orderingParameters.Add(new AssetOrder { Field = OrderBy.RELATED, Direction = OrderDir.DESC });
            }

            // it's not real recommendation but it's used to return items in a specific order.
            orderingParameters.Add(new AssetOrder { Field = OrderBy.RECOMMENDATION, Direction = OrderDir.DESC });
            var assetListEsOrderingResult = MapToEsOrderByFields(orderingParameters, input);

            return new ChannelEsOrderingResult
            {
                SpecificOrder = specificOrder,
                EsOrderByFields = assetListEsOrderingResult.EsOrderByFields,
                Order = assetListEsOrderingResult.Order
            };
        }

        private ChannelEsOrderingResult GetOrderingResult(
            OrderObj order,
            IReadOnlyCollection<AssetOrder> orderingParameters,
            AssetListEsOrderingCommonInput input)
        {
            var result = MapToEsOrderByFields(order, orderingParameters, input);

            return new ChannelEsOrderingResult
            {
                EsOrderByFields = result.EsOrderByFields,
                Order = result.Order
            };
        }

        private static (IReadOnlyCollection<AssetOrder> orderingParameters, OrderObj sourceOrder) GetResolvedChannelOrdering(
            InternalChannelRequest request,
            Channel channel)
        {
            var orderingParameters = request.orderingParameters;
            var sourceOrder = request.order;
            var shouldUseOrderFromRequest = orderingParameters?.Count > 0
                || sourceOrder != null && sourceOrder.m_eOrderBy != OrderBy.NONE;
            if (!shouldUseOrderFromRequest)
            {
                // replace channel order with default if it's not compatible with group by.
                orderingParameters = IndexManagerCommonHelpers.IsChannelOrderCompatibleWithGroupBy(
                    request.searchGroupBy,
                    channel.OrderingParameters,
                    channel.m_OrderObject)
                    ? channel.OrderingParameters
                    : new List<AssetOrder> { new AssetOrder { Field = OrderBy.ID, Direction = OrderDir.DESC } };
                sourceOrder = channel.m_OrderObject;
            }

            return (orderingParameters, sourceOrder);
        }

        private OrderObj MapToOrderObject(AssetOrder parameters, AssetListEsOrderingCommonInput input)
        {
            switch (parameters)
            {
                case AssetOrderByStatistics statisticsParameters:
                    return new OrderObj
                    {
                        m_eOrderBy = statisticsParameters.Field,
                        m_eOrderDir = statisticsParameters.Direction,
                        trendingAssetWindow = statisticsParameters.TrendingAssetWindow
                    };
                case AssetOrderByMeta metaParameters:
                    var (isMetaPadded, _) = GetMetaEsOrderingValues(metaParameters.MetaName, input);

                    return new OrderObj
                    {
                        m_eOrderBy = metaParameters.Field,
                        m_eOrderDir = metaParameters.Direction,
                        m_sOrderValue = metaParameters.MetaName,
                        shouldPadString = isMetaPadded
                    };
                case AssetSlidingWindowOrder slidingWindowOrder:
                    return new OrderObj
                    {
                        m_eOrderBy = slidingWindowOrder.Field,
                        m_eOrderDir = slidingWindowOrder.Direction,
                        m_bIsSlidingWindowField = true,
                        lu_min_period_id = slidingWindowOrder.SlidingWindowPeriod
                    };
                default:
                {
                    return new OrderObj
                    {
                        m_eOrderBy = parameters.Field,
                        m_eOrderDir = parameters.Direction
                    };
                }
            }
        }

        private static IReadOnlyCollection<long> BuildSpecificOrdering(Channel channel, OrderDir orderingDirection)
        {
            if (channel.ManualAssets?.Count > 0)
            {
                var orderedAssets = orderingDirection == OrderDir.DESC
                    ? channel.ManualAssets.OrderByDescending(x => x.OrderNum)
                    : channel.ManualAssets.OrderBy(x => x.OrderNum);

                return orderedAssets.Select(x => x.AssetId).ToList();
            }

            var manualMedias = channel.m_lManualMedias ?? new List<ManualMedia>();
            var orderedMedias = orderingDirection == OrderDir.DESC
                ? manualMedias.OrderByDescending(x => x.m_nOrderNum)
                : manualMedias.OrderBy(x => x.m_nOrderNum);

            return orderedMedias.Select(x => long.Parse(x.m_sMediaId)).ToList();
        }

        private static OrderObj MapToOrderObject(OrderObj source)
        {
            var order = new OrderObj
            {
                m_eOrderBy = OrderBy.NONE,
                m_eOrderDir = OrderDir.DESC
            };

            CatalogLogic.GetOrderValues(ref order, source);
            order.trendingAssetWindow = source?.trendingAssetWindow;
            if (order.m_eOrderBy == OrderBy.META && string.IsNullOrEmpty(order.m_sOrderValue))
            {
                order.m_eOrderBy = OrderBy.CREATE_DATE;
                order.m_eOrderDir = OrderDir.DESC;
            }

            return order;
        }

        private (bool isMetaPadded, Type metaType) GetMetaEsOrderingValues(
            string metaName,
            AssetListEsOrderingCommonInput input)
        {
            var model = new MetaByNameInput
            {
                MetaName = metaName,
                GroupId = input.GroupId,
                ShouldSearchEpg = input.ShouldSearchEpg,
                ShouldSearchMedia = input.ShouldSearchMedia,
                ShouldSearchRecordings = input.ShouldSearchRecordings
            };

            var meta = _catalogManager.GetMetaByName(model);
            var type = meta?.ValueType;
            var isMetaPadded = (input.ShouldSearchEpg || input.ShouldSearchRecordings)
                && (type == typeof(int) || type == typeof(double) || type == typeof(long) || type == typeof(float));

            return (isMetaPadded, type);
        }

        private OrderObj MapToOrderObject(OrderObj source, AssetListEsOrderingCommonInput input)
        {
            var result = MapToOrderObject(source);
            if (result.m_eOrderBy != OrderBy.META)
            {
                return result;
            }

            var (isMetaPadded, _) = GetMetaEsOrderingValues(result.m_sOrderValue, input);
            result.shouldPadString = isMetaPadded;

            return result;
        }

        private AssetListEsOrderingResult MapToEsOrderByFields(
            IReadOnlyCollection<AssetOrder> source,
            AssetListEsOrderingCommonInput input)
        {
            var esOrderByFields = source
                .Select(x => MapToEsOrderByField(x, input))
                .ToList();

            var order = MapToOrderObject(source.First(), input);

            return new AssetListEsOrderingResult
            {
                EsOrderByFields = esOrderByFields,
                Order = order
            };
        }

        private IEsOrderByField MapToEsOrderByField(AssetOrder source, AssetListEsOrderingCommonInput input)
        {
            switch (source)
            {
                case AssetOrderByMeta parameters:
                    var (isMetaPadded, metaType) = GetMetaEsOrderingValues(parameters.MetaName, input);
                    return new EsOrderByMetaField(parameters.MetaName, parameters.Direction, isMetaPadded, metaType, input.Language, parameters.IsMissingFirst);
                case AssetOrderByStatistics parameters:
                    return new EsOrderByStatisticsField(parameters.Field, parameters.Direction, parameters.TrendingAssetWindow);
                case AssetSlidingWindowOrder parameters:
                    return new EsOrderBySlidingWindow(parameters.Field, parameters.Direction, parameters.SlidingWindowPeriod);
                default:
                    return ShouldSortByStartDateOfAssociationTags(source.Field, input)
                        ? new EsOrderByStartDateAndAssociationTags(source.Direction)
                        : (IEsOrderByField)new EsOrderByField(source.Field, source.Direction, input.Language);
            }
        }

        private static bool ShouldSortByStartDateOfAssociationTags(OrderBy orderBy, AssetListEsOrderingCommonInput input)
            => orderBy == OrderBy.START_DATE
                && input.AssociationTags?.Count > 0
                && input.ParentMediaTypes?.Count > 0
                && input.ShouldSearchMedia;

        private IEsOrderByField MapToEsOrderByField(OrderObj order, AssetListEsOrderingCommonInput input)
        {
            switch (order.m_eOrderBy)
            {
                case OrderBy.META:
                    var (isMetaPadded, metaType) = GetMetaEsOrderingValues(order.m_sOrderValue, input);

                    return new EsOrderByMetaField(order.m_sOrderValue, order.m_eOrderDir, isMetaPadded, metaType, input.Language);
                case OrderBy.VIEWS:
                case OrderBy.LIKE_COUNTER:
                case OrderBy.VOTES_COUNT:
                case OrderBy.RATING:
                    return order.m_bIsSlidingWindowField
                        ? (IEsOrderByField)new EsOrderBySlidingWindow(
                            order.m_eOrderBy,
                            order.m_eOrderDir,
                            order.lu_min_period_id)
                        : new EsOrderByStatisticsField(
                            order.m_eOrderBy,
                            order.m_eOrderDir,
                            order.trendingAssetWindow);
                default:
                    return ShouldSortByStartDateOfAssociationTags(order.m_eOrderBy, input)
                        ? new EsOrderByStartDateAndAssociationTags(order.m_eOrderDir)
                        : (IEsOrderByField)new EsOrderByField(order.m_eOrderBy, order.m_eOrderDir, input.Language);
            }
        }
    }
}