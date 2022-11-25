using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    public class TopEntitledRspStrategy : IRspStrategy
    {
        private readonly IAssetOrderingService _assetOrderingService;
        private readonly IIndexManagerFactory _indexManagerFactory;
        private readonly IGroupRepresentativesExtendedRequestMapper _requestMapper;

        private static readonly Lazy<IRspStrategy> LazyInstance = new Lazy<IRspStrategy>(
            () => new TopEntitledRspStrategy(
                AssetOrderingService.Instance,
                IndexManagerFactory.Instance,
                GroupRepresentativesExtendedRequestMapper.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        public static IRspStrategy Instance => LazyInstance.Value;

        public TopEntitledRspStrategy(
            IAssetOrderingService assetOrderingService,
            IIndexManagerFactory indexManagerFactory,
            IGroupRepresentativesExtendedRequestMapper requestMapper)
        {
            _assetOrderingService = assetOrderingService;
            _indexManagerFactory = indexManagerFactory;
            _requestMapper = requestMapper;
        }

        public GenericResponse<IEnumerable<UnifiedSearchResult>> ApplyPolicy(
            GroupRepresentativesRequest request,
            CatalogClientData clientData,
            IEnumerable<AggregationResult> aggregations)
        {
            var esOrderByFieldResponse = _assetOrderingService.MapToEsOrderByField(request, clientData);
            if (!esOrderByFieldResponse.IsOkStatusCode())
            {
                return new GenericResponse<IEnumerable<UnifiedSearchResult>>(esOrderByFieldResponse.Status);
            }

            var entitledUnifiedSearchRequest = BuildEntitledGroupRepresentativesRequest(
                request,
                clientData,
                esOrderByFieldResponse.Object);
            // Get group representatives to which a user is entitled.
            var groupEntitledRepresentativesResponse =
                (UnifiedSearchResponse)entitledUnifiedSearchRequest.GetResponse(entitledUnifiedSearchRequest);
            if (!groupEntitledRepresentativesResponse.status.IsOkStatusCode())
            {
                return new GenericResponse<IEnumerable<UnifiedSearchResult>>(groupEntitledRepresentativesResponse.status);
            }

            // If no entitled groups then return original group representatives.
            if (groupEntitledRepresentativesResponse.aggregationResults?.Any() != true)
            {
                return new GenericResponse<IEnumerable<UnifiedSearchResult>>(
                    Status.Ok,
                    aggregations.Select(x => (ExtendedSearchResult)x.topHits[0]));
            }

            // Substitute original group representatives with entitled group representatives where group keys are equal.
            var assetsToOrder = new List<UnifiedSearchResult>();
            var entitledAggregationsMap = groupEntitledRepresentativesResponse.aggregationResults[0].results
                .ToDictionary(x => x.value, y => y.topHits[0]);
            foreach (var aggregation in aggregations)
            {
                var assetToOrder = entitledAggregationsMap.TryGetValue(aggregation.value, out var entitledAsset)
                    ? entitledAsset
                    : aggregation.topHits[0];

                assetsToOrder.Add(assetToOrder);
            }

            return new GenericResponse<IEnumerable<UnifiedSearchResult>>(Status.Ok, assetsToOrder);
        }

        private UnifiedSearchRequest BuildEntitledGroupRepresentativesRequest(
            GroupRepresentativesRequest request,
            CatalogClientData clientData,
            IEsOrderByField esOrderByField)
        {
            var extraReturnField = GetExtraReturnField(request.PartnerId, esOrderByField);

            return _requestMapper.BuildEntitledRequest(request, clientData, extraReturnField);
        }

        private string GetExtraReturnField(int partnerId, IEsOrderByField esOrderByField)
        {
            var isV2Version = _indexManagerFactory.IsV2Version(partnerId);
            if (esOrderByField is EsOrderByStatisticsField)
            {
                return null;
            }

            var adapter = new EsOrderByFieldAdapter(esOrderByField);

            return !isV2Version ? adapter.EsV7ExtraReturnField : adapter.EsField;
        }
    }
}