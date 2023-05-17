using System;
using System.Threading;
using ApiLogic.IndexManager.Sorting;
using ApiObjects.CanaryDeployment.Elasticsearch;
using ApiObjects.SearchObjects;
using ElasticSearch.Utils;
using Phx.Lib.Appconfig;

namespace ApiLogic.IndexManager
{
    public class UnifiedQueryBuilderInitializer : IUnifiedQueryBuilderInitializer
    {
        private static readonly Lazy<IUnifiedQueryBuilderInitializer> LazyInstanceV2 =
            new Lazy<IUnifiedQueryBuilderInitializer>(
                () => new UnifiedQueryBuilderInitializer(EsSortingService.Instance(ElasticsearchVersion.ES_2_3),
                    SortingAdapter.Instance),
                LazyThreadSafetyMode.PublicationOnly);

        private static readonly Lazy<IUnifiedQueryBuilderInitializer> LazyInstanceV7 =
            new Lazy<IUnifiedQueryBuilderInitializer>(
                () => new UnifiedQueryBuilderInitializer(EsSortingService.Instance(ElasticsearchVersion.ES_7),
                    SortingAdapter.Instance),
                LazyThreadSafetyMode.PublicationOnly);

        private readonly IEsSortingService _esSortingService;
        private readonly ISortingAdapter _sortingAdapter;

        public static IUnifiedQueryBuilderInitializer Instance(ElasticsearchVersion version) =>
            version == ElasticsearchVersion.ES_2_3
                ? LazyInstanceV2.Value
                : LazyInstanceV7.Value;

        public UnifiedQueryBuilderInitializer(IEsSortingService esSortingService, ISortingAdapter sortingAdapter)
        {
            _sortingAdapter = sortingAdapter;
            _esSortingService = esSortingService;
        }

        public void SetPagingForUnifiedSearch(IUnifiedQueryBuilder queryBuilder)
        {
            var orderByFields = _sortingAdapter.ResolveOrdering(queryBuilder.SearchDefinitions);
            if (_esSortingService.ShouldSortByStatistics(orderByFields))
            {
                queryBuilder.PageIndex = 0;
                int maxStatSortResult = ApplicationConfiguration.Current.ElasticSearchConfiguration.MaxStatSortResults
                    .Value;
                if (maxStatSortResult > 0)
                {
                    queryBuilder.PageSize = maxStatSortResult;
                }
                else
                {
                    queryBuilder.PageSize = 0;
                    queryBuilder.GetAllDocuments = true;
                }

                if (_esSortingService.ShouldSortByStartDateOfAssociationTags(orderByFields))
                {
                    queryBuilder.SearchDefinitions.extraReturnFields.Add("start_date");
                    queryBuilder.SearchDefinitions.extraReturnFields.Add("media_type_id");
                }

                // if ordered by stats, we want at least one top hit count
                if (queryBuilder.SearchDefinitions.topHitsCount < 1)
                {
                    queryBuilder.SearchDefinitions.topHitsCount = 1;
                }
            }
            // if there is group by
            else if (_esSortingService.IsBucketsReorderingRequired(orderByFields, queryBuilder.SearchDefinitions.distinctGroup)
                || _esSortingService.ShouldReorderMissedKeyBucket(
                    orderByFields,
                    queryBuilder.SearchDefinitions.distinctGroup,
                    queryBuilder.SearchDefinitions.GroupByOption))
            {
                queryBuilder.PageIndex = 0;
                queryBuilder.PageSize = 0;
                queryBuilder.GetAllDocuments = true;

                // if ordered by stats, we want at least one top hit count
                if (queryBuilder.SearchDefinitions.topHitsCount < 1)
                {
                    queryBuilder.SearchDefinitions.topHitsCount = 1;
                }
            }
            else
            {
                // normal case - regular page index and size
                queryBuilder.PageIndex = queryBuilder.SearchDefinitions.pageIndex;
                queryBuilder.PageSize = queryBuilder.SearchDefinitions.pageSize;
                queryBuilder.From = queryBuilder.SearchDefinitions.from;

                // no group by and page size is 0 means we want all documents
                if (queryBuilder.PageSize == 0 && (queryBuilder.SearchDefinitions.groupBy == null || queryBuilder.SearchDefinitions.groupBy.Count == 0))
                {
                    queryBuilder.GetAllDocuments = true;
                }
            }
        }
    }
}