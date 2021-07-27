using System;
using System.Linq;
using ApiObjects.SearchObjects;
using ElasticSearch.Searcher;

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
        public void SetQueryPaging(UnifiedSearchDefinitions unifiedSearchDefinitions, ESUnifiedQueryBuilder queryBuilder)
        {
            queryBuilder.PageIndex = 0;
            queryBuilder.PageSize = 0;
            queryBuilder.From = 0;
            unifiedSearchDefinitions.topHitsCount = 1;
        }

        public ESAggregationsResult HandleQueryResponse(UnifiedSearchDefinitions search, int pageSize, int fromIndex, ESUnifiedQueryBuilder queryBuilder, string responseBody)
        {
            var elasticAggregation = ESAggregationsResult.FullParse(responseBody, queryBuilder.Aggregations);
            if (elasticAggregation.Aggregations == null) throw new Exception("Unable to parse Elasticsearch response");

            var groupBy = search.groupBy.Single();
            var buckets = elasticAggregation.Aggregations[groupBy.Key].buckets;

            buckets.RemoveRange(0, Math.Min(fromIndex, buckets.Count));

            return elasticAggregation;
        }
    }
}
