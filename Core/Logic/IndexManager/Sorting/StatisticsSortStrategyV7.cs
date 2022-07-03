using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiLogic.IndexManager.Helpers;
using ApiLogic.IndexManager.Models;
using ApiLogic.IndexManager.NestData;
using ApiObjects.SearchObjects;
using CachingProvider.LayeredCache;
using ElasticSearch.NEST;
using ElasticSearch.Searcher;
using Nest;
using Phx.Lib.Appconfig;
using Phx.Lib.Log;

namespace ApiLogic.IndexManager.Sorting
{
    public class StatisticsSortStrategyV7 : IStatisticsSortStrategy
    {
        private readonly ILayeredCache _layeredCache;
        private readonly IKLogger _log;
        private readonly IElasticClient _elasticClient;

        private static readonly Lazy<IStatisticsSortStrategy> LazyInstance = new Lazy<IStatisticsSortStrategy>(
            () => new StatisticsSortStrategyV7(LayeredCache.Instance,
                new KLogger(typeof(StatisticsSortStrategyV7).FullName),
                NESTFactory.GetInstance(ApplicationConfiguration.Current)),
            LazyThreadSafetyMode.PublicationOnly);

        public static IStatisticsSortStrategy Instance => LazyInstance.Value;

        public StatisticsSortStrategyV7(ILayeredCache layeredCache, IKLogger log, IElasticClient elasticClient)
        {
            _layeredCache = layeredCache;
            _log = log;
            _elasticClient = elasticClient;
        }

        public IEnumerable<(long id, string sortValue)> SortAssetsByStatsWithSortValues(IEnumerable<long> assetIds, EsOrderByStatisticsField esOrderByField, int partnerId)
        {
            if (esOrderByField.TrendingAssetWindow.HasValue)
            {
                //BEO-9415
                return SortAssetsByStatsWithSortValues(assetIds,
                    esOrderByField.OrderByField,
                    esOrderByField.OrderByDirection,
                    partnerId,
                    esOrderByField.TrendingAssetWindow,
                    DateTime.UtcNow);
            }

            return SortAssetsByStatsWithSortValues(assetIds, esOrderByField.OrderByField, esOrderByField.OrderByDirection, partnerId);
        }

        public IEnumerable<(long id, string sortValue)> SortAssetsByStatsWithSortValues(IEnumerable<long> assetIds, OrderBy orderBy, OrderDir orderDirection, int partnerId, DateTime? startDate = null, DateTime? endDate = null)
        {
            return SortAssetsByStats(assetIds, assetIds.ToDictionary(x => x), orderBy, orderDirection, partnerId, startDate, endDate);
        }

        public IEnumerable<(long id, string sortValue)> SortAssetsByStatsWithSortValues(
            IEnumerable<ExtendedUnifiedSearchResult> extendedUnifiedSearchResults,
            OrderBy orderBy,
            OrderDir orderDirection,
            int partnerId,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var assetIds = extendedUnifiedSearchResults.Select(x => x.AssetId).ToArray();
            var idsWithSortValues = SortAssetsByStats(assetIds, assetIds.ToDictionary(x => x), orderBy, orderDirection, partnerId, startDate, endDate);
            return idsWithSortValues;
        }
        
        public IEnumerable<long> SortAssetsByStats(IEnumerable<long> assetIds, EsOrderByStatisticsField esOrderByField, int partnerId)
        {
            return SortAssetsByStatsWithSortValues(assetIds, esOrderByField, partnerId)
                .Select(x => x.id)
                .ToArray();
        }

        public IEnumerable<long> SortAssetsByStats(IEnumerable<long> assetIds, OrderBy orderBy, OrderDir orderDirection, int partnerId, DateTime? startDate = null, DateTime? endDate = null)
        {
            return SortAssetsByStatsWithSortValues(assetIds, orderBy, orderDirection, partnerId, startDate, endDate)
                .Select(x => x.id)
                .ToArray();
        }
        
        private List<(T item, string sortValue)> SortAssetsByStats<T>(
            IEnumerable<T> searchResultsList,
            Dictionary<long, T> resultsDictionary,
            OrderBy orderBy,
            OrderDir orderDirection,
            int partnerId,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var assetIds = resultsDictionary.Keys.Distinct().ToList();

            IDictionary<long, double> ratingsAggregationsDictionary = null;
            IDictionary<long, int> countsAggregationsDictionary = null;

            // we will use layered cache for asset stats for non-rating values and only if we don't have dates in filter
            if (startDate == null && endDate == null && orderBy != OrderBy.RATING)
            {
                countsAggregationsDictionary = SortAssetsByStatsWithLayeredCache(assetIds, orderBy, orderDirection, partnerId);
            }
            else
            {
                if (orderBy == OrderBy.RATING)
                {
                    ratingsAggregationsDictionary = GetAssetStatsValuesFromElasticSearch<double>(assetIds, orderBy, startDate, endDate, partnerId);
                }
                else
                {
                    countsAggregationsDictionary = GetAssetStatsValuesFromElasticSearch<int>(assetIds, orderBy, startDate, endDate, partnerId);
                }
            }

            #region Process Aggregations

            // get a sorted list of the asset Ids that have statistical data in the aggregations dictionary
            var sortedList = new List<(T item, string sortValue)>();
            var alreadyContainedIds = new HashSet<T>();

            if (countsAggregationsDictionary != null)
            {
                ProcessStatsDictionaryResults(countsAggregationsDictionary, orderDirection, alreadyContainedIds, sortedList, resultsDictionary);
            }
            else
            {
                ProcessStatsDictionaryResults(ratingsAggregationsDictionary, orderDirection, alreadyContainedIds, sortedList, resultsDictionary);
            }

            #endregion

            // Add all ids that don't have stats
            foreach (var currentSearchResult in searchResultsList)
            {
                if (alreadyContainedIds == null || !alreadyContainedIds.Contains(currentSearchResult))
                {
                    var item = (currentSearchResult, (string) null);
                    // Depending on direction - if it is ascending, insert Id at start. Otherwise at end
                    if (orderDirection == OrderDir.ASC)
                    {
                        sortedList.Insert(0, item);
                    }
                    else
                    {
                        sortedList.Add(item);
                    }
                }
            }

            return sortedList;
        }
        
        private Dictionary<long, int> SortAssetsByStatsWithLayeredCache(List<long> assetIds, OrderBy orderBy, OrderDir orderDirection, int partnerId)
        {
            var result = new Dictionary<long, int>();
            Dictionary<string, int> layeredCacheResult = new Dictionary<string, int>();
            if (assetIds != null && assetIds.Count > 0)
            {
                try
                {
                    Dictionary<string, string> keyToOriginalValueMap = 
                        assetIds.Select(x => x.ToString()).ToDictionary(x => LayeredCacheKeys.GetAssetStatsSortKey(x, orderBy.ToString()));
                    Dictionary<string, List<string>> invalidationKeys =
                        keyToOriginalValueMap.Keys.ToDictionary(x => x, x => new List<string>() { LayeredCacheKeys.GetAssetStatsInvalidationKey(partnerId) });

                    Dictionary<string, object> funcParams = new Dictionary<string, object>();
                    funcParams.Add("orderBy", orderBy);
                    funcParams.Add("assetIds", assetIds);
                    funcParams.Add("partnerId", partnerId);

                    if (!_layeredCache.GetValues(keyToOriginalValueMap, ref layeredCacheResult, SortAssetsByStatsDelegate, funcParams,
                        partnerId, LayeredCacheConfigNames.ASSET_STATS_SORT_CONFIG_NAME, invalidationKeys))
                    {
                        _log.ErrorFormat("Failed getting asset stats from cache, ids: {0}:", 
                            assetIds.Count < 100 ? string.Join(",", assetIds) : string.Join(",", assetIds.Take(100)));
                    }
                    else
                    {
                        foreach (var item in layeredCacheResult)
                        {
                            string key = keyToOriginalValueMap[item.Key];
                            result.Add(long.Parse(key), item.Value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.Error("Failed SortAssetsByStatsLayeredCache", ex);
                }
            }

            return result;
        }
        
        private IDictionary<long, T> GetAssetStatsValuesFromElasticSearch<T>(List<long> assetIds, OrderBy orderBy, DateTime? startDate, DateTime? endDate, int partnerId)
        {
            var statsDictionary = new ConcurrentDictionary<long, T>();
            #region Define Aggregation Query

            var mustQueryContainers = new List<QueryContainer>();
            var descriptor = new QueryContainerDescriptor<NestSocialActionStatistics>();

            var groupIdTerm = descriptor.Term(field => field.GroupID, partnerId);
            mustQueryContainers.Add(groupIdTerm);

            #region define date filter

            if ((startDate != null && startDate.HasValue && !startDate.Equals(DateTime.MinValue)) ||
                (endDate != null && endDate.HasValue && !endDate.Equals(DateTime.MaxValue)))
            {
                var dateRange = descriptor.DateRange(selector =>
                {
                    selector = selector.Field(field => field.Date);
                    if (startDate != null && startDate.HasValue && !startDate.Equals(DateTime.MinValue))
                    {
                        selector = selector.GreaterThanOrEquals(startDate);
                    }

                    if (endDate != null && endDate.HasValue && !endDate.Equals(DateTime.MaxValue))
                    {
                        selector = selector.LessThanOrEquals(endDate);
                    }

                    return selector;
                });

                mustQueryContainers.Add(dateRange);
            }

            #endregion

            #region define action filter

            string actionName = string.Empty;

            switch (orderBy)
            {
                case ApiObjects.SearchObjects.OrderBy.VIEWS:
                    {
                        actionName = NamingHelper.STAT_ACTION_FIRST_PLAY;
                        break;
                    }
                case ApiObjects.SearchObjects.OrderBy.RATING:
                    {
                        actionName = NamingHelper.STAT_ACTION_RATES;
                        break;
                    }
                case ApiObjects.SearchObjects.OrderBy.VOTES_COUNT:
                    {
                        actionName = NamingHelper.STAT_ACTION_RATES;
                        break;
                    }
                case ApiObjects.SearchObjects.OrderBy.LIKE_COUNTER:
                    {
                        actionName = NamingHelper.STAT_ACTION_LIKE;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            var actionTerm = descriptor.Term(field => field.Action, actionName);

            mustQueryContainers.Add(actionTerm);

            #endregion

            string aggregationName = "stats";
            string subSumAggregationName = string.Empty;
            AggregationDictionary aggregations = new AggregationDictionary();
            AggregationContainer subAggregation = null;

            // Ratings is based on average, others on sum (count)
            if (orderBy == OrderBy.RATING)
            {
                subAggregation = new StatsAggregation(NamingHelper.SUB_STATS_AGGREGATION_NAME, NamingHelper.STAT_ACTION_RATE_VALUE_FIELD);
                subSumAggregationName = NamingHelper.SUB_STATS_AGGREGATION_NAME;
            }
            else
            {
                subAggregation = new SumAggregation(NamingHelper.SUB_SUM_AGGREGATION_NAME, NamingHelper.STAT_ACTION_COUNT_VALUE_FIELD)
                {
                    Missing = 1
                };
                subSumAggregationName = NamingHelper.SUB_SUM_AGGREGATION_NAME;
            }

            var termsAggregation = new TermsAggregation(aggregationName)
            {
                Field = "media_id",
                Aggregations = new AggregationDictionary()
                {
                    { subSumAggregationName, subAggregation}
                }
            };

            aggregations.Add(aggregationName, termsAggregation);

            #endregion

            #region Split call of aggregations query to pieces

            int aggregationsSize = ApplicationConfiguration.Current.ElasticSearchConfiguration.StatSortBulkSize.Value;

            //Start MultiThread Call
            List<Task> tasks = new List<Task>();

            // Split the request to small pieces, to avoid timeout exceptions
            for (int assetIndex = 0; assetIndex < assetIds.Count; assetIndex += aggregationsSize)
            {
                // Convert partial Ids to strings
                string index = NamingHelper.GetStatisticsIndexName(partnerId);

                try
                {
                    LogContextData contextData = new LogContextData();
                    // Create a task for the search and merge of partial aggregations

                    var idsTerm = new TermsQuery
                    {
                        Field = "media_id",
                        Terms = assetIds.Skip(assetIndex).Take(aggregationsSize).Select(id => id.ToString())
                    };

                    Task task = Task.Run(() =>
                    {
                        contextData.Load();

                        // each time create a copy of the previous list of conditions (group id, dates, action)
                        // and add the current IDs term (each run of the loop it changes)
                        var currentMustQueryContainers = new List<QueryContainer>(mustQueryContainers);
                        currentMustQueryContainers.Add(idsTerm);

                        var boolQuery = descriptor.Bool(b => b.Must(currentMustQueryContainers.ToArray()));

                        var searchResponse = _elasticClient.Search<NestSocialActionStatistics>(searchRequest => searchRequest
                            // hits don't interest us at all
                            .Size(0)
                            .Index(index)
                            .Query(x => boolQuery)
                            .Aggregations(aggregations)
                            );

                        if (searchResponse.IsValid)
                        {
                            var termsAggregate = searchResponse.Aggregations.Terms<long>(aggregationName);

                            if (orderBy == OrderBy.RATING)
                            {
                                foreach (var bucket in termsAggregate.Buckets)
                                {
                                    var statsAggregation = bucket.Stats(NamingHelper.SUB_STATS_AGGREGATION_NAME);
                                    statsDictionary[bucket.Key] = (T)Convert.ChangeType(statsAggregation.Average, typeof(T));
                                }
                            }
                            else
                            {
                                foreach (var bucket in termsAggregate.Buckets)
                                {
                                    var sumAgg = bucket.Sum(NamingHelper.SUB_SUM_AGGREGATION_NAME);
                                    statsDictionary[bucket.Key] = (T)Convert.ChangeType(sumAgg.Value, typeof(T));
                                }
                            }
                        }
                    });

                    tasks.Add(task);
                }
                catch (Exception ex)
                {
                    _log.ErrorFormat("Error in SortAssetsByStats, Exception: {0}", ex);
                }
            }

            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in SortAssetsByStats (WAIT ALL), Exception: {0}", ex);
            }

            return statsDictionary;

            #endregion
        }
        
        private Tuple<Dictionary<string, int>, bool> SortAssetsByStatsDelegate(Dictionary<string, object> funcParams)
        {
            IDictionary<long, int> countsAggregationsDictionary;
            var result = new Dictionary<string, int>();
            bool success = true;
            List<long> assetIds = new List<long>();
            OrderBy orderBy = OrderBy.NONE;
            int partnerId = 0;

            try
            {
                if (funcParams.ContainsKey("partnerId"))
                {
                    partnerId = (int)funcParams["partnerId"];
                }
                
                // extract from funcParams
                if (funcParams.ContainsKey("orderBy"))
                {
                    orderBy = (OrderBy)funcParams["orderBy"];
                }

                // if we don't have missing keys - all ids should be sent
                if (funcParams.ContainsKey(LayeredCache.MISSING_KEYS) && funcParams[LayeredCache.MISSING_KEYS] != null)
                {
                    assetIds = ((List<string>)funcParams[LayeredCache.MISSING_KEYS]).Select(x => long.Parse(x)).ToList();
                }
                else if (funcParams.ContainsKey("assetIds"))
                {
                    assetIds = (List<long>)funcParams["assetIds"];
                }

                countsAggregationsDictionary = GetAssetStatsValuesFromElasticSearch<int>(assetIds, orderBy, null, null, partnerId);

                // fill dictionary of asset-id..stats-value (if it doesn't exist in ES, fill it with a 0)
                foreach (var assetId in assetIds)
                {
                    string dictionaryKey =
                        //assetId.ToString();
                        LayeredCacheKeys.GetAssetStatsSortKey(assetId.ToString(), orderBy.ToString());

                    if (!countsAggregationsDictionary.ContainsKey(assetId))
                    {
                        result[dictionaryKey] = 0;
                    }
                    else
                    {
                        result[dictionaryKey] = countsAggregationsDictionary[assetId];
                    }
                }
            }
            catch (Exception ex)
            {
                success = false;
                _log.ErrorFormat("Error when trying to sort assets by stats. group Id = {0}, ex = {1}", partnerId, ex);
            }

            return new Tuple<Dictionary<string, int>, bool>(result, success);
        }
        
        private void ProcessStatsDictionaryResults<T, TK>(IDictionary<long, T> assetIdToStatsValueDictionary,
            OrderDir orderDirection, 
            HashSet<TK> alreadyContainedIds, 
            List<(TK item, string sortValue)> sortedList, 
            Dictionary<long, TK> searchResultsDictionary)
        {
            if (assetIdToStatsValueDictionary != null && assetIdToStatsValueDictionary.Count > 0)
            {
                var sortedStatsDictionary = assetIdToStatsValueDictionary.OrderBy(o => o.Value).ThenBy(o => o.Key).Reverse();

                // We base this section on the assumption that aggregations request is sorted, descending
                foreach (var currentValue in sortedStatsDictionary)
                {
                    long currentId = currentValue.Key;
                    var searchResult = searchResultsDictionary[currentId];
                    var item = (searchResult, currentValue.Value.ToString());
                    //Depending on direction - if it is ascending, insert Id at start. Otherwise at end
                    if (orderDirection == OrderDir.ASC)
                    {
                        sortedList.Insert(0, item);
                    }
                    else
                    {
                        sortedList.Add(item);
                    }

                    alreadyContainedIds.Add(searchResult);
                }
            }
        }
    }
}
