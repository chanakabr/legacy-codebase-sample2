using System;
using System.Linq;
using ApiLogic.IndexManager.QueryBuilders;
using ApiObjects.SearchObjects;
using ElasticSearch.Searcher;

namespace ApiLogic.Catalog.IndexManager.GroupBy
{
    public class GroupByWithSpecificOrder : IGroupBySearch
    {
        public ESAggregationsResult HandleQueryResponse(UnifiedSearchDefinitions search, int pageSize, int fromIndex, ESUnifiedQueryBuilder queryBuilder, string responseBody)
        {
            var elasticAggregation = ESAggregationsResult.FullParse(responseBody, queryBuilder.Aggregations);
            if (elasticAggregation.Aggregations == null) throw new Exception("Unable to parse Elasticsearch response");

            var groupBy = search.groupBy.Single();
            var buckets = elasticAggregation.Aggregations[groupBy.Key].buckets;

            var assetIdToBucket = search.specificOrder.ToDictionary(x => x, x => (ESAggregationBucket)null);
            foreach (var bucket in buckets)
            {
                var hits = bucket.Aggregations[ESTopHitsAggregation.DEFAULT_NAME].hits?.hits;
                if (hits?.Count == 1)
                {
                    var document = hits.Single();
                    assetIdToBucket[document.asset_id] = bucket;
                }
            }

            var pagedBuckets = assetIdToBucket.Values
                .Where(x => x != null)
                .Skip(fromIndex)
                .Take(pageSize)
                .ToList();
            elasticAggregation.Aggregations[groupBy.Key].buckets = pagedBuckets;

            return elasticAggregation;
        }

        public void SetQueryPaging(UnifiedSearchDefinitions unifiedSearchDefinitions, ESUnifiedQueryBuilder queryBuilder)
        {
            queryBuilder.PageIndex = 0;
            queryBuilder.PageSize = 0;
            queryBuilder.From = 0;
            unifiedSearchDefinitions.topHitsCount = 1;
        }
    }
}