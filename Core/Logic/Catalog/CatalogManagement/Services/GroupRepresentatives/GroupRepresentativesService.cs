using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiLogic.Catalog.Response;
using ApiLogic.IndexManager.QueryBuilders;
using ApiLogic.IndexManager.Sorting;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using ApiObjects.SearchObjects.GroupRepresentatives;
using Catalog.Response;
using Core.Catalog;
using Core.Catalog.Request;
using Core.Catalog.Response;
using ElasticSearch.Searcher;

namespace ApiLogic.Catalog.CatalogManagement.Services.GroupRepresentatives
{
    public class GroupRepresentativesService : IGroupRepresentativesService
    {
        private static readonly Lazy<GroupRepresentativesService> LazyInstance = new Lazy<GroupRepresentativesService>(
            () => new GroupRepresentativesService(
                IndexManagerFactory.Instance,
                SortingByBasicFieldsService.Instance,
                AssetOrderingService.Instance,
                StatisticsSortStrategy.Instance,
                StatisticsSortStrategyV7.Instance,
                TopRspStrategy.Instance,
                TopEntitledRspStrategy.Instance,
                GroupRepresentativesExtendedRequestMapper.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        public static IGroupRepresentativesService Instance => LazyInstance.Value;

        private readonly IIndexManagerFactory _indexManagerFactory;
        private readonly ISortingByBasicFieldsService _sortingByBasicFieldsService;
        private readonly IAssetOrderingService _assetOrderingService;
        private readonly IStatisticsSortStrategy _statisticsSortStrategyV2;
        private readonly IStatisticsSortStrategy _statisticsSortStrategyV7;
        private readonly IRspStrategy _topRspStrategy;
        private readonly IRspStrategy _topEntitledRspStrategy;
        private readonly IGroupRepresentativesExtendedRequestMapper _requestMapper;

        public GroupRepresentativesService(
            IIndexManagerFactory indexManagerFactory,
            ISortingByBasicFieldsService sortingByBasicFieldsService,
            IAssetOrderingService assetOrderingService,
            IStatisticsSortStrategy statisticsSortStrategyV2,
            IStatisticsSortStrategy statisticsSortStrategyV7,
            IRspStrategy topRspStrategy,
            IRspStrategy topEntitledRspStrategy,
            IGroupRepresentativesExtendedRequestMapper requestMapper)
        {
            _indexManagerFactory = indexManagerFactory;
            _sortingByBasicFieldsService = sortingByBasicFieldsService;
            _assetOrderingService = assetOrderingService;
            _statisticsSortStrategyV2 = statisticsSortStrategyV2;
            _statisticsSortStrategyV7 = statisticsSortStrategyV7;
            _topRspStrategy = topRspStrategy;
            _topEntitledRspStrategy = topEntitledRspStrategy;
            _requestMapper = requestMapper;
        }

        public GenericResponse<GroupRepresentativesResult> GetGroupRepresentativeList(
            GroupRepresentativesRequest request,
            CatalogClientData clientData)
        {
            var isV2Version = _indexManagerFactory.IsV2Version(request.PartnerId);
            var esOrderByFieldResponse = _assetOrderingService.MapToEsOrderByField(request, clientData);
            if (!esOrderByFieldResponse.IsOkStatusCode())
            {
                return BuildErrorResponse<GroupRepresentativesResult>(esOrderByFieldResponse.Status);
            }

            // Send unified search request with group by and group-level ordering to get correct group representatives.
            var unifiedSearchRequest = BuildGroupRepresentativesRequest(request, clientData, esOrderByFieldResponse.Object, isV2Version);
            var groupRepresentativesResponse = (UnifiedSearchResponse)unifiedSearchRequest.GetResponse(unifiedSearchRequest);
            if (!groupRepresentativesResponse.status.IsOkStatusCode())
            {
                return BuildErrorResponse<GroupRepresentativesResult>(groupRepresentativesResponse.status);
            }

            // Return empty response if no group representatives found.
            if (groupRepresentativesResponse.aggregationResults == null ||
                !groupRepresentativesResponse.aggregationResults.Any())
            {
                return BuildEmptyResponse(unifiedSearchRequest);
            }

            // Split aggregation result into group representatives and "missed key" assets group.
            var (aggregations, missedKeyAssets) = GetAssetGroupsToReorder(groupRepresentativesResponse, request);
            // Handle different selection policies.
            var assetsToReorderResponse = HandleSelectionPolicy(request, clientData, aggregations);
            if (!assetsToReorderResponse.IsOkStatusCode())
            {
                return BuildErrorResponse<GroupRepresentativesResult>(assetsToReorderResponse.Status);
            }

            return GetOrderedAssets(
                request,
                assetsToReorderResponse.Object.ToList(),
                esOrderByFieldResponse.Object,
                isV2Version,
                unifiedSearchRequest,
                missedKeyAssets);
        }

        private GenericResponse<GroupRepresentativesResult> GetOrderedAssets(
            GroupRepresentativesRequest request,
            List<UnifiedSearchResult> assetsToReorder,
            IEsOrderByField esOrderByField,
            bool isV2Version,
            UnifiedSearchRequest unifiedSearchRequest,
            IEnumerable<UnifiedSearchResult> missedKeyAssets)
        {
            if (missedKeyAssets != null)
            {
                assetsToReorder.AddRange(missedKeyAssets);
            }

            var assetsDictionary = assetsToReorder.ToDictionary(x => long.Parse(x.AssetId));
            var reorderedAssets = GetReorderedAssets(request, assetsToReorder, esOrderByField, isV2Version);
            var pagedAssets = reorderedAssets
                .Select(x => assetsDictionary[x])
                .Skip(request.PageIndex * request.PageSize)
                .Take(request.PageSize);

            return new GenericResponse<GroupRepresentativesResult>(
                Status.Ok,
                new GroupRepresentativesResult
                {
                    OriginalRequest = unifiedSearchRequest,
                    SearchResponse = new UnifiedSearchResponse
                    {
                        searchResults = pagedAssets.ToList(),
                        m_nTotalItems = reorderedAssets.Count()
                    }
                });
        }

        private List<long> GetReorderedAssets(
            GroupRepresentativesRequest request,
            IEnumerable<UnifiedSearchResult> assetsToReorder,
            IEsOrderByField esOrderByField,
            bool isV2Version)
        {
            if (esOrderByField is EsOrderByStatisticsField esOrderByStatisticsField)
            {
                var assetIds = assetsToReorder.Select(x => long.Parse(x.AssetId)).ToList();

                return GetSortStrategy(isV2Version)
                    .SortAssetsByStats(assetIds, esOrderByStatisticsField, request.PartnerId).ToList();
            }

            var extraReturnField = GetExtraReturnField(esOrderByField, isV2Version);
            var assets = assetsToReorder.Select(x => (ExtendedSearchResult)x).ToList();

            return _sortingByBasicFieldsService.GetSortedAssets(assets, esOrderByField, extraReturnField)
                .Select(x => x.id)
                .ToList();
        }

        private static (IEnumerable<AggregationResult>, IEnumerable<UnifiedSearchResult>) GetAssetGroupsToReorder(
            UnifiedSearchResponse response,
            GroupRepresentativesRequest request)
        {
            var groups = response.aggregationResults[0].results;
            if (request.UnmatchedItemsPolicy == UnmatchedItemsPolicy.Omit)
            {
                return (groups, null);
            }

            IEnumerable<UnifiedSearchResult> missedKeyAssets = null;
            var aggregations = new List<AggregationResult>();
            var missedBucketKey = ESUnifiedQueryBuilder.MissedHitBucketKeyString;
            foreach (var group in groups)
            {
                if (group.value == missedBucketKey)
                {
                    missedKeyAssets = group.topHits.ToList();
                }
                else
                {
                    aggregations.Add(group);
                }
            }

            return (aggregations, missedKeyAssets);
        }

        private GenericResponse<IEnumerable<UnifiedSearchResult>> HandleSelectionPolicy(
            GroupRepresentativesRequest request,
            CatalogClientData clientData,
            IEnumerable<AggregationResult> aggregations)
        {
            var strategy = request.SelectionPolicy is TopSubscriptionEntitledRsp
                ? _topEntitledRspStrategy
                : _topRspStrategy;

            return strategy.ApplyPolicy(request, clientData, aggregations);
        }

        private static GenericResponse<T> BuildErrorResponse<T>(Status status)
        {
            var response = new GenericResponse<T>();
            response.SetStatus(status);

            return response;
        }

        private static GenericResponse<GroupRepresentativesResult> BuildEmptyResponse(UnifiedSearchRequest request)
        {
            var result = new GroupRepresentativesResult
            {
                OriginalRequest = request,
                SearchResponse = new UnifiedSearchResponse()
            };

            return new GenericResponse<GroupRepresentativesResult>(Status.Ok, result);
        }

        private UnifiedSearchRequest BuildGroupRepresentativesRequest(
            GroupRepresentativesRequest request,
            CatalogClientData clientData,
            IEsOrderByField esOrderByField,
            bool isV2Version)
        {
            // Should be not-null value if in-memory sorting is possible.
            var extraReturnField = GetExtraReturnField(esOrderByField, isV2Version);

            return _requestMapper.BuildRequest(request, clientData, extraReturnField);
        }

        private static string GetExtraReturnField(IEsOrderByField esOrderByField, bool isV2Version)
        {
            if (esOrderByField is EsOrderByStatisticsField)
            {
                return null;
            }

            var adapter = new EsOrderByFieldAdapter(esOrderByField);

            return !isV2Version ? adapter.EsV7ExtraReturnField : adapter.EsField;
        }

        private IStatisticsSortStrategy GetSortStrategy(bool isV2Version)
            => isV2Version
                ? _statisticsSortStrategyV2
                : _statisticsSortStrategyV7;
    }
}
