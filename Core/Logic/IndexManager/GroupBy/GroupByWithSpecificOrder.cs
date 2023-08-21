using System;
using System.Linq;
using ApiLogic.IndexManager.QueryBuilders;
using ApiObjects.SearchObjects;
using ElasticSearch.Searcher;
using Phx.Lib.Appconfig;

namespace ApiLogic.Catalog.IndexManager.GroupBy
{
    public class GroupByWithSpecificOrder : IGroupBySearch
    {
        public ESAggregationsResult HandleQueryResponse(UnifiedSearchDefinitions search, ESUnifiedQueryBuilder queryBuilder, string responseBody)
        {
            var elasticAggregation = ESAggregationsResult.FullParse(responseBody, queryBuilder.Aggregations);
            if (elasticAggregation.Aggregations == null) throw new Exception("Unable to parse Elasticsearch response");

            var groupBy = search.groupBy.Single();
            var buckets = elasticAggregation.Aggregations[groupBy.Key].buckets;

            ESAggregationBucket missingKeysBucket = null;
            var assetIdToBucket = search.specificOrder.ToDictionary(x => x, x => (ESAggregationBucket)null);
            foreach (var bucket in buckets)
            {
                if (search.GroupByOption == GroupingOption.Include &&
                    bucket.key == ESUnifiedQueryBuilder.MissedHitBucketKeyString)
                {
                    missingKeysBucket = bucket;
                }

                var hits = bucket.Aggregations[ESTopHitsAggregation.DEFAULT_NAME].hits?.hits;
                var document = hits?.FirstOrDefault();
                if (document == null)
                {
                    continue;
                }

                assetIdToBucket[document.asset_id] = bucket;
            }

            var filteredBuckets = assetIdToBucket.Values.Where(x => x != null).ToList();
            if (missingKeysBucket != null)
            {
                filteredBuckets.Add(missingKeysBucket);
            }

            elasticAggregation.Aggregations[groupBy.Key].buckets = filteredBuckets;

            return elasticAggregation;
        }

        public void SetQueryPaging(UnifiedSearchDefinitions unifiedSearchDefinitions, ESUnifiedQueryBuilder queryBuilder)
        {
            queryBuilder.PageIndex = 0;
            queryBuilder.PageSize = 0;
            queryBuilder.From = 0;
            unifiedSearchDefinitions.topHitsCount = 1;

            if (unifiedSearchDefinitions.GroupByOption == GroupingOption.Include)
            {
                unifiedSearchDefinitions.pageIndex = 0;
                unifiedSearchDefinitions.pageSize = ApplicationConfiguration.Current.ElasticSearchConfiguration.MaxResults.Value;
            }
        }
    }
}
