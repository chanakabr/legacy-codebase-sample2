using System;
using System.Threading;
using ApiLogic.IndexManager.QueryBuilders;
using ApiLogic.IndexManager.Sorting;
using ApiObjects.CanaryDeployment.Elasticsearch;
using ApiObjects.SearchObjects;
using ElasticSearch.Searcher;
using ElasticSearch.Utils;
using Phx.Lib.Appconfig;



namespace ApiLogic.Catalog.IndexManager.GroupBy
{
    /// <summary>
    /// Read to learn unknown words https://www.elastic.co/guide/en/elasticsearch/reference/2.3/search-aggregations-bucket-terms-aggregation.html
    /// 
    /// So, we want to group by one field and distinct by this field (aka collapse) => each aggregation bucket will have single result/value
    /// and order by another field of numeric type(id, create_date,etc)
    /// 
    /// ES could sort buckets between themselves
    /// It applies function like MAX/MIN/AVG/etc to all bucket records and sort by results
    /// These functions could be applied to numeric fields only
    /// 
    /// There is no paging for aggregations, only max size
    /// So, if we need second page(size:10), we need to retrieve 20 records from ES and just ignore first ten records
    /// 
    /// Elasticsearch(ES) could return search and aggregations results in one response
    /// We don't care about search results, that why PageIndex and PageSize set to 0
    /// </summary>
    internal class GroupByWithOrderByNumericField : IGroupBySearch
    {
        private readonly ISortingAdapter _sortingAdapter;
        private readonly IEsSortingService _esSortingService;

        private static readonly Lazy<IGroupBySearch> LazyInstance = new Lazy<IGroupBySearch>(
            () => new GroupByWithOrderByNumericField(SortingAdapter.Instance, EsSortingService.Instance(ElasticsearchVersion.ES_2_3)),
            LazyThreadSafetyMode.PublicationOnly);

        public static IGroupBySearch Instance => LazyInstance.Value;

        public GroupByWithOrderByNumericField(ISortingAdapter sortingAdapter, IEsSortingService esSortingService)
        {
            _sortingAdapter = sortingAdapter;
            _esSortingService = esSortingService;
        }

        public void SetQueryPaging(UnifiedSearchDefinitions unifiedSearchDefinitions, ESUnifiedQueryBuilder queryBuilder)
        {
            queryBuilder.PageIndex = 0;
            queryBuilder.PageSize = 0;
            queryBuilder.From = 0;
            unifiedSearchDefinitions.topHitsCount = 1;
            // We need to put "missing keys" bucket at the end of result list, but if ordering is ASC, then the bucket will come first.
            // Therefore we need to return all buckets to apply paging correctly
            var ordering = _sortingAdapter.ResolveOrdering(queryBuilder.SearchDefinitions);
            if (_esSortingService.ShouldReorderMissedKeyBucket(ordering, unifiedSearchDefinitions.distinctGroup, unifiedSearchDefinitions.GroupByOption))
            {
                unifiedSearchDefinitions.pageIndex = 0;
                unifiedSearchDefinitions.pageSize = ApplicationConfiguration.Current.ElasticSearchConfiguration.MaxResults.Value;
            }
        }

        public ESAggregationsResult HandleQueryResponse(UnifiedSearchDefinitions search, ESUnifiedQueryBuilder queryBuilder, string responseBody)
        {
            var elasticAggregation = ESAggregationsResult.FullParse(responseBody, queryBuilder.Aggregations);
            if (elasticAggregation.Aggregations == null) throw new Exception("Unable to parse Elasticsearch response");

            return elasticAggregation;
        }
    }
}
