using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiLogic.IndexManager.QueryBuilders;
using ApiLogic.IndexManager.Sorting;
using ApiObjects.SearchObjects;
using ElasticSearch.Common;
using ElasticSearch.Searcher;
using Phx.Lib.Appconfig;

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
            () => new GroupByWithOrderByNonNumericField(SortingAdapter.Instance, SortingByBasicFieldsService.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        public static IGroupBySearch Instance => LazyInstance.Value;

        private readonly ISortingAdapter _sortingAdapter;
        private readonly ISortingByBasicFieldsService _sortingByBasicFieldsService;

        public GroupByWithOrderByNonNumericField(
            ISortingAdapter sortingAdapter,
            ISortingByBasicFieldsService sortingByBasicFieldsService)
        {
            _sortingAdapter = sortingAdapter ?? throw new ArgumentNullException(nameof(sortingAdapter));
            _sortingByBasicFieldsService = sortingByBasicFieldsService ?? throw new ArgumentNullException(nameof(sortingByBasicFieldsService));
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

        public ESAggregationsResult HandleQueryResponse(UnifiedSearchDefinitions search, ESUnifiedQueryBuilder queryBuilder, string responseBody)
        {
            var esOrderByFields = _sortingAdapter.ResolveOrdering(search);
            var esOrderByField = esOrderByFields.Single(); // if group by is set, then only primary sorting can be set.
            var extraFields = esOrderByField is EsOrderByMetaField orderByMetaField
                ? new List<string> { new EsOrderByFieldAdapter(orderByMetaField).EsField }
                : null;

            var elasticAggregation = ESAggregationsResult.FullParse(responseBody, queryBuilder.Aggregations, extraFields);
            if (elasticAggregation.Aggregations == null) throw new Exception("Unable to parse Elasticsearch response");

            var groupBy = search.groupBy.Single(); // key - original field name; value - field name how it's stored in ES
            var buckets = GetOrderedBuckets(search, elasticAggregation, groupBy, esOrderByField);
            elasticAggregation.Aggregations[groupBy.Key].buckets = buckets;

            return elasticAggregation;
        }

        // TODO: How to deal with that method in terms of asset id. Does this method have a mirror in V7???
        private List<ESAggregationBucket> GetOrderedBuckets(
            UnifiedSearchDefinitions search,
            ESAggregationsResult elasticAggregation,
            GroupByDefinition groupBy,
            IEsOrderByField esOrderByField)
        {
            var assetIdToBucket = new Dictionary<long, ESAggregationBucket>();
            var assetsToReorder = new List<ElasticSearchApi.ESAssetDocument>();
            ESAggregationBucket missingKeysBucket = null;
            foreach (var bucket in elasticAggregation.Aggregations[groupBy.Key].buckets)
            {
                if (search.GroupByOption == GroupingOption.Include &&
                    bucket.key == ESUnifiedQueryBuilder.MissedHitBucketKeyString)
                {
                    missingKeysBucket = bucket;
                    continue;
                }

                var hits = bucket.Aggregations[ESTopHitsAggregation.DEFAULT_NAME].hits?.hits;
                var document = hits?.FirstOrDefault();
                if (document == null)
                {
                    continue;
                }

                assetIdToBucket[document.asset_id] = bucket;
                assetsToReorder.Add(document);
            }

            var resultBuckets = _sortingByBasicFieldsService
                .ListOrderedIdsWithSortValues(assetsToReorder, esOrderByField)
                .Select(x => assetIdToBucket[x.id])
                .ToList();
            if (missingKeysBucket != null)
            {
                resultBuckets.Add(missingKeysBucket);
            }

            return resultBuckets;
        }
    }
}
