using System;
using System.Collections.Generic;
using System.Linq;
using ApiObjects.SearchObjects;
using ConfigurationManager;
using ElasticSearch.Common;
using ElasticSearch.Searcher;
using OrderDir = ApiObjects.SearchObjects.OrderDir;
using TVinciShared;

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
            var metaSortField = ESUnifiedQueryBuilder.GetMetaSortField(search.order);
            var extraFields = metaSortField == null ? null : new List<string> { metaSortField };
            var elasticAggregation = ESAggregationsResult.FullParse(responseBody, queryBuilder.Aggregations, extraFields);
            if (elasticAggregation.Aggregations == null) throw new Exception("Unable to parse Elasticsearch response");

            var groupBy = search.groupBy.Single(); // key - original field name; value - field name how it's stored in ES
            var buckets = elasticAggregation.Aggregations[groupBy.Key].buckets;

            var orderByField = search.order.m_eOrderBy == OrderBy.NAME
                ? new Func<ElasticSearchApi.ESAssetDocument, string>(_ => _.name)
                : new Func<ElasticSearchApi.ESAssetDocument, string>(_ => _.extraReturnFields?.GetValueOrDefault(metaSortField));

            buckets.Sort(CompareByStringField(orderByField, search.order.m_eOrderDir));
            
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
