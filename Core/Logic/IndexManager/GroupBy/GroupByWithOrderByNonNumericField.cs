using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiLogic.IndexManager.QueryBuilders;
using ApiLogic.IndexManager.Sorting;
using ApiObjects.SearchObjects;
using Phx.Lib.Appconfig;
using ElasticSearch.Common;
using ElasticSearch.Searcher;
using TVinciShared;
using OrderDir = ApiObjects.SearchObjects.OrderDir;

namespace ApiLogic.Catalog.IndexManager.GroupBy
{
    /// <summary>
    /// Read to learn unknown words https://www.elastic.co/guide/en/elasticsearch/reference/2.3/search-aggregations-bucket-terms-aggregation.html
    /// 
    /// So, we want to group by one field and distinct by this field (aka collapse) => each aggregation bucket will have single result/value
    /// and order by another field of NON-numeric type(name, meta).
    /// 
    /// ES couldn't sort buckets between themselves, read here why <see cref="GroupByWithOrderByNumericField"/>
    /// So, we need to load all records(up to MaxResults count) and make sorting and paging in memory
    /// 
    /// Elasticsearch(ES) could return search and aggregations results in one response.
    /// We don't care about search results, that why PageIndex and PageSize set to 0.
    /// </summary>
    internal class GroupByWithOrderByNonNumericField : IGroupBySearch
    {
        private static readonly Lazy<IGroupBySearch> LazyInstance = new Lazy<IGroupBySearch>(
            () => new GroupByWithOrderByNonNumericField(SortingAdapter.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        public static IGroupBySearch Instance => LazyInstance.Value;

        private readonly ISortingAdapter _sortingAdapter;

        public GroupByWithOrderByNonNumericField(ISortingAdapter sortingAdapter)
        {
            _sortingAdapter = sortingAdapter ?? throw new ArgumentNullException(nameof(sortingAdapter));
        }

        public void SetQueryPaging(UnifiedSearchDefinitions unifiedSearchDefinitions, ESUnifiedQueryBuilder queryBuilder)
        {
            queryBuilder.PageIndex = 0;
            queryBuilder.PageSize = 0;
            queryBuilder.From = 0;
            unifiedSearchDefinitions.topHitsCount = 1;
            unifiedSearchDefinitions.pageIndex = 0;
            unifiedSearchDefinitions.pageSize = ApplicationConfiguration.Current.ElasticSearchConfiguration.MaxResults.Value;
        }

        public ESAggregationsResult HandleQueryResponse(UnifiedSearchDefinitions search, int pageSize, int fromIndex, ESUnifiedQueryBuilder queryBuilder, string responseBody)
        {
            var esOrderByFields = _sortingAdapter.ResolveOrdering(search);
            var esOrderByField = esOrderByFields.Single(); // if group by is set, then only primary sorting can be set.
            var extraFields = esOrderByField is EsOrderByMetaField orderByMetaField
                ? new List<string> { orderByMetaField.EsField }
                : null;

            var elasticAggregation = ESAggregationsResult.FullParse(responseBody, queryBuilder.Aggregations, extraFields);
            if (elasticAggregation.Aggregations == null) throw new Exception("Unable to parse Elasticsearch response");

            var groupBy = search.groupBy.Single(); // key - original field name; value - field name how it's stored in ES
            var buckets = elasticAggregation.Aggregations[groupBy.Key].buckets;

            var orderByField = extraFields != null
                ? new Func<ElasticSearchApi.ESAssetDocument, string>(_ => _.extraReturnFields?.GetValueOrDefault(extraFields.Single()))
                : new Func<ElasticSearchApi.ESAssetDocument, string>(_ => _.name);

            buckets.Sort(CompareByStringField(orderByField, esOrderByField.OrderByDirection));

            var from = Math.Min(fromIndex, buckets.Count);
            var count = Math.Min(pageSize, buckets.Count - from);
            elasticAggregation.Aggregations[groupBy.Key].buckets = buckets.GetRange(from, count);

            return elasticAggregation;
        }

        private static Comparison<ESAggregationBucket> CompareByStringField(Func<ElasticSearchApi.ESAssetDocument, string> fieldGetter, OrderDir orderDirection)
        {
            return new Comparison<ESAggregationBucket>((x, y) => {
                var xName = GetFieldValue(x, fieldGetter);
                var yName = GetFieldValue(y, fieldGetter);
                return orderDirection == OrderDir.ASC
                    ? StringComparer.InvariantCultureIgnoreCase.Compare(xName, yName)
                    : StringComparer.InvariantCultureIgnoreCase.Compare(yName, xName);
            });
        }

        private static string GetFieldValue(ESAggregationBucket bucket, Func<ElasticSearchApi.ESAssetDocument, string> fieldGetter)
        {
            var hits = bucket.Aggregations[ESTopHitsAggregation.DEFAULT_NAME].hits?.hits;
            if (hits!=null && hits.Count > 1)
                return null;
            
            var document = hits.SingleOrDefault();
            return document == null ? null : fieldGetter(document);
        }
    }
}
