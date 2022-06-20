using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiLogic.Catalog.IndexManager.GroupBy;
using ApiLogic.IndexManager.Helpers;
using ApiLogic.IndexManager.Models;
using ApiLogic.IndexManager.Sorting.Stages;
using ApiObjects.CanaryDeployment.Elasticsearch;
using ApiObjects.SearchObjects;
using Core.Catalog.Response;
using ElasticSearch.Searcher;
using ElasticSearch.Utils;

namespace ApiLogic.IndexManager.Sorting
{
    public class SortingService : ISortingService
    {
        private readonly ISortingByStatsService _sortingByStatsService;
        private readonly ISortingByBasicFieldsService _sortingByBasicFieldsService;
        private readonly ISortingAdapter _sortingAdapter;
        private readonly IEsSortingService _esSortingService;

        private static readonly Lazy<ISortingService> LazyInstanceV2 =
            new Lazy<ISortingService>(
                () => new SortingService(
                    SortingByStatsService.Instance(ElasticsearchVersion.ES_2_3),
                    SortingByBasicFieldsService.Instance,
                    SortingAdapter.Instance,
                    EsSortingService.Instance(ElasticsearchVersion.ES_2_3)),
                LazyThreadSafetyMode.PublicationOnly);
        
        private static readonly Lazy<ISortingService> LazyInstanceV7 =
            new Lazy<ISortingService>(
                () => new SortingService(
                    SortingByStatsService.Instance(ElasticsearchVersion.ES_7),
                    SortingByBasicFieldsService.Instance,
                    SortingAdapter.Instance,
                    EsSortingService.Instance(ElasticsearchVersion.ES_7)),
                LazyThreadSafetyMode.PublicationOnly);

        public SortingService(
            ISortingByStatsService sortingByStatsService,
            ISortingByBasicFieldsService sortingByBasicFieldsService,
            ISortingAdapter sortingAdapter,
            IEsSortingService esSortingService)
        {
            _sortingByStatsService = sortingByStatsService ?? throw new ArgumentNullException(nameof(sortingByStatsService));
            _sortingByBasicFieldsService = sortingByBasicFieldsService ?? throw new ArgumentNullException(nameof(sortingByBasicFieldsService));
            _sortingAdapter = sortingAdapter ?? throw new ArgumentNullException(nameof(sortingAdapter));
            _esSortingService = esSortingService ?? throw new ArgumentNullException(nameof(esSortingService));
        }

        public static ISortingService Instance(ElasticsearchVersion version)
        {
            switch (version)
            {
                case ElasticsearchVersion.ES_2_3:
                    return LazyInstanceV2.Value;
                case ElasticsearchVersion.ES_7:
                    return LazyInstanceV7.Value;
                default:
                    throw new ArgumentOutOfRangeException(nameof(version), version, null);
            }
        }

        public IReadOnlyCollection<long> GetReorderedAssetIds(UnifiedSearchDefinitions definitions, IEnumerable<ExtendedUnifiedSearchResult> extendedUnifiedSearchResults)
        {
            return GetReorderedAssetIdsInternal(definitions, extendedUnifiedSearchResults);
        }

        public IReadOnlyCollection<UnifiedSearchResult> GetReorderedAssets(UnifiedSearchDefinitions definitions, IEnumerable<ExtendedUnifiedSearchResult> extendedUnifiedSearchResults)
        {
            var assetIds = GetReorderedAssetIds(definitions, extendedUnifiedSearchResults);
            var extendedUnifiedSearchResultsDic = extendedUnifiedSearchResults.ToDictionary(x => x.AssetId);
            return assetIds.Select(id => extendedUnifiedSearchResultsDic[id].Result).ToArray();
        }

        public bool IsSortingCompleted(UnifiedSearchDefinitions definitions)
        {
            return BuildSortingStages(definitions).All(s => s.Status == StageStatus.Completed);
        }

        public bool IsSortingCompatibleWithGroupBy(IReadOnlyCollection<IEsOrderByField> orderByFields)
            => GetGroupBySortingStrategy(orderByFields) != null;

        public IGroupBySearch GetGroupBySortingStrategy(IReadOnlyCollection<IEsOrderByField> orderByFields)
        {
            if (orderByFields?.Count != 1)
            {
                return null;
            }

            var orderByField = orderByFields.Single();
            if (orderByField is EsOrderByField esOrderByField)
            {
                return IndexManagerCommonHelpers.GetStrategy(esOrderByField.OrderByField);
            }

            return orderByField is EsOrderByMetaField ? GroupByWithOrderByNonNumericField.Instance : null;
        }

        private static IEnumerable<(long id, string sortValue)> GetReorderedItemIds<TItem, TKey>(
            IEnumerable<TItem> itemsToReorder,
            Func<TItem, TKey> primarySortingSelector,
            Func<IEnumerable<TItem>, IEnumerable<(long id, string sortValue)>> getReorderedItemIds,
            Func<TItem, long> idSelector)
        {
            var bucketsToReorder = itemsToReorder
                .GroupBy(primarySortingSelector)
                .ToDictionary(x => x.Key, y => y.ToArray());

            var unorderedBuckets = new ConcurrentDictionary<TKey, TItem[]>(
                bucketsToReorder.Where(x => x.Value.Length > 1));

            if (!unorderedBuckets.Any())
            {
                return null;
            }

            var unorderedItems = unorderedBuckets.SelectMany(x => x.Value).ToArray();
            var reorderedAssetIds = getReorderedItemIds(unorderedItems);

            var increment = 0;
            var reorderedAssetIdsDictionary = reorderedAssetIds.ToDictionary(x => x.id, _ => ++increment);
            Parallel.ForEach(unorderedBuckets,
                item =>
                {
                    unorderedBuckets[item.Key] =
                        item.Value.OrderBy(x => reorderedAssetIdsDictionary[idSelector(x)]).ToArray();
                });

            foreach (var unorderedBucket in unorderedBuckets)
            {
                bucketsToReorder[unorderedBucket.Key] = unorderedBucket.Value;
            }

            // TODO: for now we skip sort value, which could be used for third, forth level sorting.
            return bucketsToReorder.Values.SelectMany(x => x)
                .Select(x => (idSelector(x), (string) null))
                .ToArray();
        }

        private long[] GetReorderedAssetIdsInternal(
            UnifiedSearchDefinitions definitions,
            IEnumerable<ExtendedUnifiedSearchResult> extendedUnifiedSearchResults)
        {
            var stages = BuildSortingStages(definitions);
            if (stages.Count > 1)
            {
                return GetReorderedAssetIds(extendedUnifiedSearchResults, definitions, stages);
            }

            var singleStage = stages.Single();
            return singleStage.Status == StageStatus.InProgress
                ? GetSortedItemsByStatistics(definitions,
                        singleStage.OrderByField,
                        extendedUnifiedSearchResults)
                    .Select(x => x.id)
                    .ToArray()
                : null;
        }

        private long[] GetReorderedAssetIds(
            IEnumerable<ExtendedUnifiedSearchResult> extendedUnifiedSearchResults,
            UnifiedSearchDefinitions definitions,
            LinkedList<ISortingStage> stages)
        {
            // IMPORTANT!!! There is no need to go through stages in case all of them are already completed.
            if (stages.All(x => x.Status == StageStatus.Completed))
            {
                return null;
            }
            
            IReadOnlyDictionary<long, ExtendedUnifiedSearchResult> idToExtendedUnifiedSearchResult = extendedUnifiedSearchResults.ToDictionary(x => x.AssetId);
            for (var stage = stages.First; stage != null; stage = stage.Next)
            {
                var sortedAssetIds = stage.Previous == null
                    ? GetReorderedItemsAfterFirstStage(extendedUnifiedSearchResults, definitions, stage.Value)
                    : GetReorderedItems(extendedUnifiedSearchResults, idToExtendedUnifiedSearchResult, definitions, stage.Value, stage.Previous.Value.SortedResults);

                stage.Value.SetSortedResults(sortedAssetIds);
            }

            return stages.Last().SortedResults.Select(x => x.id).ToArray();
        }

        private IEnumerable<(long id, string sortValue)> GetReorderedItems(
            IEnumerable<ExtendedUnifiedSearchResult> extendedUnifiedSearchResults,
            IReadOnlyDictionary<long, ExtendedUnifiedSearchResult> idToExtendedUnifiedSearchResult,
            UnifiedSearchDefinitions definitions,
            ISortingStage stage,
            IEnumerable<(long id, string sortValue)> sortedResults)
        {
            // for third, forth sorting levels, like (META, META, STATS) and primary and secondary orderings were applied by elastic.
            if (stage.Status == StageStatus.Completed)
            {
                return sortedResults;
            }

            IEnumerable<(long id, string sortValue)> GetReorderedBySecondarySorting(
                IEnumerable<(long id, string sortValue)> sortedByPrimarySortingResult)
            {
                var extendedUnifiedSearchResultsToSort = sortedByPrimarySortingResult.Select(x => idToExtendedUnifiedSearchResult[x.id]).ToArray();

                return _esSortingService.ShouldSortByStatistics(stage.OrderByField)
                    ? _sortingByStatsService.ListOrderedIdsWithSortValues(extendedUnifiedSearchResults, definitions, stage.OrderByField)
                    : _sortingByBasicFieldsService.ListOrderedIdsWithSortValues(
                        extendedUnifiedSearchResultsToSort,
                        stage.OrderByField);
            }

            var reorderedItemIds = GetReorderedItemIds(
                sortedResults,
                x => x.sortValue ?? string.Empty,
                GetReorderedBySecondarySorting,
                x => x.id);

            return reorderedItemIds ?? sortedResults;
        }

        private IEnumerable<(long id, string sortValue)> GetReorderedItemsAfterFirstStage(
            IEnumerable<ExtendedUnifiedSearchResult> extendedUnifiedSearchResults,
            UnifiedSearchDefinitions definitions,
            ISortingStage sortingStage)
        {
            if (_esSortingService.ShouldSortByStatistics(sortingStage.OrderByField))
            {
                return GetSortedItemsByStatistics(definitions, sortingStage.OrderByField, extendedUnifiedSearchResults);
            }

            return extendedUnifiedSearchResults.Select(x => (x.AssetId, ExtractSortValue(x.DocAdapter, sortingStage.OrderByField))).ToArray();
        }

        private IEnumerable<(long id, string sortValue)> GetSortedItemsByStatistics(
            UnifiedSearchDefinitions definitions,
            IEsOrderByField esOrderByField,
            IEnumerable<ExtendedUnifiedSearchResult> extendedUnifiedSearchResults)
        {
            return _sortingByStatsService.ListOrderedIdsWithSortValues(extendedUnifiedSearchResults, definitions, esOrderByField);
        }

        private string ExtractSortValue(EsAssetAdapter esAssetAdapter, IEsOrderByField orderField)
        {
            switch (orderField)
            {
                case EsOrderByField field:
                    // TODO: Please, be aware that the pretty the same switch clause is placed in SortingByBasicFieldsService class. If you change smth there, you might need changes in SortingByBasicFieldsService class as well.
                    switch (field.OrderByField)
                    {
                        case OrderBy.ID:
                            return esAssetAdapter.Id;
                        case OrderBy.START_DATE:
                            return esAssetAdapter.StartDate.ToString("s");
                        case OrderBy.NAME:
                            return esAssetAdapter.Name;
                        case OrderBy.UPDATE_DATE:
                            return esAssetAdapter.UpdateDate.ToString("s");
                        case OrderBy.NONE:
                        case OrderBy.RELATED:
                            return esAssetAdapter.Score.ToString(CultureInfo.InvariantCulture);
                        case OrderBy.CREATE_DATE:
                            return esAssetAdapter.CreateDate.ToString("s");
                    }

                    break;
                case EsOrderByMetaField castedOrderField:
                    return esAssetAdapter.GetMetaValue(castedOrderField);
            }

            return string.Empty;
        }

        private LinkedList<ISortingStage> BuildSortingStages(UnifiedSearchDefinitions definitions)
        {
            var stages = new LinkedList<ISortingStage>();
            var isSortedByElastic = true;
            var esOrderByFields = _sortingAdapter.ResolveOrdering(definitions);
            foreach (var esOrderByField in esOrderByFields)
            {
                isSortedByElastic = !_esSortingService.ShouldSortByStatistics(esOrderByField) && isSortedByElastic;
                var stageStatus = isSortedByElastic ? StageStatus.Completed : StageStatus.InProgress;

                stages.AddLast(new SortingStage(esOrderByField, stageStatus));
            }

            return stages;
        }
    }
}
