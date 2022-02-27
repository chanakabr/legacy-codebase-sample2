using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiLogic.Catalog.IndexManager.GroupBy;
using ApiLogic.IndexManager.Helpers;
using ApiLogic.IndexManager.Sorting.Stages;
using ApiObjects.SearchObjects;
using Core.Catalog.Response;
using ElasticSearch.Common;
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

        private static readonly Lazy<ISortingService> LazyInstance =
            new Lazy<ISortingService>(
                () => new SortingService(
                    SortingByStatsService.Instance,
                    SortingByBasicFieldsService.Instance,
                    SortingAdapter.Instance,
                    EsSortingService.Instance),
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
            _esSortingService = esSortingService  ?? throw new ArgumentNullException(nameof(esSortingService));
        }

        public static ISortingService Instance => LazyInstance.Value;

        public IReadOnlyCollection<long> GetReorderedAssetIds(
            IEnumerable<UnifiedSearchResult> searchResults,
            UnifiedSearchDefinitions definitions,
            IDictionary<string, ElasticSearchApi.ESAssetDocument> assetIdToDocument)
        {

            if (definitions.PriorityGroupsMappings == null || !definitions.PriorityGroupsMappings.Any())
            {
                return GetReorderedAssetIdsInternal(searchResults, definitions, assetIdToDocument);
            }

            IEnumerable<(long id, string sortValue)> GetReorderedByPriorityGroups(
                IEnumerable<UnifiedSearchResult> searchResultForPriorityGroup)
                => GetReorderedAssetIdsInternal(searchResultForPriorityGroup, definitions, assetIdToDocument)
                    .Select(x => (x, (string)null))
                    .ToArray();

            return GetReorderedItemIds(
                searchResults,
                x => x.Score,
                GetReorderedByPriorityGroups,
                x => long.Parse(x.AssetId))
                ?.Select(x => x.id)
                .ToArray();
        }

        public IGroupBySearch GetGroupBySortingStrategy(IEsOrderByField orderByField)
        {
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
                .Select(x => (idSelector(x), (string)null))
                .ToArray();
        }

        private long[] GetReorderedAssetIdsInternal(
            IEnumerable<UnifiedSearchResult> searchResults,
            UnifiedSearchDefinitions definitions,
            IDictionary<string, ElasticSearchApi.ESAssetDocument> assetIdToDocument)
        {
            var stages = BuildSortingStages(definitions);
            if (stages.Count > 1)
            {
                return GetReorderedAssetIds(searchResults, definitions, assetIdToDocument, stages);
            }

            var singleStage = stages.Single();
            return singleStage.Status == StageStatus.InProgress
                ? GetSortedItemsByStatistics(
                        searchResults,
                        definitions,
                        assetIdToDocument,
                        singleStage.OrderByField)
                    .Select(x => x.id).ToArray()
                : null;
        }

        private long[] GetReorderedAssetIds(
            IEnumerable<UnifiedSearchResult> searchResults,
            UnifiedSearchDefinitions definitions,
            IDictionary<string, ElasticSearchApi.ESAssetDocument> assetIdToDocument,
            LinkedList<ISortingStage> stages)
        {
            // IMPORTANT!!! There is no need to go through stages in case all of them are already completed.
            if (stages.All(x => x.Status == StageStatus.Completed))
            {
                return null;
            }

            for (var stage = stages.First; stage != null; stage = stage.Next)
            {
                var sortedAssetIds = stage.Previous == null
                    ? GetReorderedItemsAfterFirstStage(
                        searchResults,
                        definitions,
                        assetIdToDocument,
                        stage.Value)
                    : GetReorderedItems(
                        definitions,
                        assetIdToDocument,
                        stage.Value,
                        stage.Previous.Value.SortedResults);

                stage.Value.SetSortedResults(sortedAssetIds);
            }

            return stages.Last().SortedResults.Select(x => x.id).ToArray();
        }

        private IEnumerable<(long id, string sortValue)> GetReorderedItems(
            UnifiedSearchDefinitions definitions,
            IDictionary<string, ElasticSearchApi.ESAssetDocument> assetIdToDocument,
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
                var esAssetDocumentsToSort = sortedByPrimarySortingResult
                    .Select(x => assetIdToDocument[x.id.ToString()])
                    .ToArray();
                var assetIdsToSort = sortedByPrimarySortingResult.Select(x => x.id).ToArray();

                return _esSortingService.ShouldSortByStatistics(stage.OrderByField)
                    ? _sortingByStatsService.ListOrderedIdsWithSortValues(
                        esAssetDocumentsToSort,
                        assetIdsToSort,
                        definitions,
                        stage.OrderByField)
                    : _sortingByBasicFieldsService.ListOrderedIdsWithSortValues(
                        esAssetDocumentsToSort,
                        stage.OrderByField);
            }

            var reorderedItemIds = GetReorderedItemIds(
                sortedResults,
                x => x.sortValue,
                GetReorderedBySecondarySorting,
                x => x.id);

            return reorderedItemIds ?? sortedResults;
        }

        private IEnumerable<(long id, string sortValue)> GetReorderedItemsAfterFirstStage(
            IEnumerable<UnifiedSearchResult> searchResults,
            UnifiedSearchDefinitions definitions,
            IDictionary<string, ElasticSearchApi.ESAssetDocument> assetIdToDocument,
            ISortingStage sortingStage)
        {
            if (_esSortingService.ShouldSortByStatistics(sortingStage.OrderByField))
            {
                return GetSortedItemsByStatistics(
                    searchResults,
                    definitions,
                    assetIdToDocument,
                    sortingStage.OrderByField);
            }

            return searchResults
                .Select(searchResult => assetIdToDocument[searchResult.AssetId])
                .Select(document => (document.asset_id, ExtractSortValue(document, sortingStage.OrderByField)))
                .Select(dummy => ((long id, string sortValue))dummy)
                .ToArray();
        }

        private IEnumerable<(long id, string sortValue)> GetSortedItemsByStatistics(
            IEnumerable<UnifiedSearchResult> searchResults,
            UnifiedSearchDefinitions definitions,
            IDictionary<string, ElasticSearchApi.ESAssetDocument> assetIdToDocument,
            IEsOrderByField esOrderByField)
        {
            var documents = new List<ElasticSearchApi.ESAssetDocument>();
            var assetIds = new List<long>();
            foreach (var searchResult in searchResults)
            {
                assetIds.Add(long.Parse(searchResult.AssetId));
                documents.Add(assetIdToDocument[searchResult.AssetId]);
            }

            return _sortingByStatsService.ListOrderedIdsWithSortValues(
                documents,
                assetIds,
                definitions,
                esOrderByField);
        }

        private static string ExtractSortValue(ElasticSearchApi.ESAssetDocument esAssetDocument, IEsOrderByField orderField)
        {
            if (orderField is EsOrderByField field)
            {
                switch (field.OrderByField)
                {
                    case OrderBy.ID:
                        return esAssetDocument.id;
                    case OrderBy.EPG_ID:
                    case OrderBy.MEDIA_ID:
                        return esAssetDocument.asset_id.ToString();
                    case OrderBy.START_DATE:
                        return esAssetDocument.start_date.ToString("s");
                    case OrderBy.NAME:
                        return esAssetDocument.name;
                    case OrderBy.UPDATE_DATE:
                        return esAssetDocument.update_date.ToString("s");
                    case OrderBy.NONE:
                    case OrderBy.RELATED:
                        return esAssetDocument.score.ToString(CultureInfo.InvariantCulture);
                }
            }

            return esAssetDocument.extraReturnFields.TryGetValue(orderField.EsField, out var value)
                ? value
                : string.Empty;
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