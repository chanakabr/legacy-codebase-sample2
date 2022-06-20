using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiLogic.IndexManager.Helpers;
using ApiLogic.IndexManager.Models;
using ApiObjects.SearchObjects;
using CachingProvider.LayeredCache;
using Phx.Lib.Appconfig;
using ElasticSearch.Common;
using ElasticSearch.Searcher;
using Phx.Lib.Log;

namespace ApiLogic.IndexManager.Sorting
{
    public class StatisticsSortStrategy : IStatisticsSortStrategy
    {
        private readonly ILayeredCache _layeredCache;
        private readonly IKLogger _log;
        private readonly IElasticSearchApi _elasticSearchApi;

        private static readonly Lazy<IStatisticsSortStrategy> LazyInstance = new Lazy<IStatisticsSortStrategy>(() =>
                new StatisticsSortStrategy(LayeredCache.Instance, new KLogger(typeof(StatisticsSortStrategy).FullName), new ElasticSearchApi(ApplicationConfiguration.Current)),
            LazyThreadSafetyMode.PublicationOnly);

        public static IStatisticsSortStrategy Instance => LazyInstance.Value;

        public StatisticsSortStrategy(ILayeredCache layeredCache, IKLogger log, IElasticSearchApi elasticSearchApi)
        {
            _layeredCache = layeredCache;
            _log = log;
            _elasticSearchApi = elasticSearchApi;
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

        /// <summary>
        /// For a given list of asset Ids, returns a list of the same IDs, after sorting them by a specific statistics
        /// </summary>
        /// <param name="assetIds"></param>
        /// <param name="_partnerId"></param>
        /// <param name="orderBy"></param>
        /// <param name="orderDirection"></param>
        /// <returns></returns>
        public IEnumerable<(long id, string sortValue)> SortAssetsByStatsWithSortValues(IEnumerable<long> assetIds, OrderBy orderBy, OrderDir orderDirection, int partnerId, DateTime? startDate = null, DateTime? endDate = null)
        {
            assetIds = assetIds.Distinct().ToList();

            var ratingsAggregationsDictionary = new ConcurrentDictionary<string, List<StatisticsAggregationResult>>();
            var countsAggregationsDictionary = new ConcurrentDictionary<string, ConcurrentDictionary<string, int>>();

            // we will use layered cache for asset stats for non-rating values and only if we don't have dates in filter
            if (startDate == null && endDate == null && orderBy != OrderBy.RATING)
            {
                Dictionary<string, int> sortValues = SortAssetsByStatsWithLayeredCache(assetIds, orderBy, orderDirection, partnerId);
                ConcurrentDictionary<string, int> innerDictionary = new ConcurrentDictionary<string, int>();
                foreach (var sortValue in sortValues)
                {
                    // we don't check if the key exists since the SortAssetsByStatsWithLayeredCache function returns a value for all passed assetIds
                    innerDictionary[sortValue.Key] = sortValue.Value;
                }

                countsAggregationsDictionary["stats"] = innerDictionary;
            }
            else
            {
                GetAssetStatsValuesFromElasticSearch(assetIds, orderBy, startDate, endDate, ratingsAggregationsDictionary, countsAggregationsDictionary, partnerId);
            }

            #region Process Aggregations

            var alreadyContainedIds = new HashSet<long>();

            // Ratings is a special case, because it is not based on count, but on average instead
            var sortedListWithSortValues = orderBy == OrderBy.RATING
                ? ProcessRatingsAggregationsResult(ratingsAggregationsDictionary, orderDirection, alreadyContainedIds)
                : ProcessCountDictionaryResults(countsAggregationsDictionary, orderDirection, alreadyContainedIds);

            #endregion

            // Add all ids that don't have stats
            foreach (var currentId in assetIds)
            {
                if (!alreadyContainedIds.Contains(currentId))
                {
                    // Depending on direction - if it is ascending, insert Id at start. Otherwise at end
                    if (orderDirection == OrderDir.ASC)
                    {
                        sortedListWithSortValues.Insert(0, (currentId, null));
                    }
                    else
                    {
                        sortedListWithSortValues.Add((currentId, null));
                    }
                }
            }

            return sortedListWithSortValues;
        }

        public IEnumerable<(long id, string sortValue)> SortAssetsByStatsWithSortValues(
            IEnumerable<ExtendedUnifiedSearchResult> extendedUnifiedSearchResults,
            OrderBy orderBy,
            OrderDir orderDirection,
            int partnerId,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var assetIds = extendedUnifiedSearchResults.Select(x => x.AssetId).Distinct().ToList();
            return SortAssetsByStatsWithSortValues(assetIds, orderBy, orderDirection, partnerId, startDate, endDate);
        }

        public IEnumerable<long> SortAssetsByStats(
            IEnumerable<long> assetIds,
            EsOrderByStatisticsField esOrderByStatisticsField,
            int partnerId)
            => SortAssetsByStatsWithSortValues(assetIds, esOrderByStatisticsField, partnerId)
                .Select(x => x.id)
                .ToArray();

        public IEnumerable<long> SortAssetsByStats(
            IEnumerable<long> assetIds,
            OrderBy orderBy,
            OrderDir orderDirection,
            int partnerId,
            DateTime? startDate = null,
            DateTime? endDate = null)
            => SortAssetsByStatsWithSortValues(assetIds, orderBy, orderDirection, partnerId, startDate, endDate)
                .Select(x => x.id)
                .ToArray();

        /// <summary>
        /// After receiving a result from ES server, process it to create a list of Ids with the given order
        /// </summary>
        /// <param name="statisticsDictionary"></param>
        /// <param name="orderDirection"></param>
        /// <param name="alreadyContainedIds"></param>
        /// <returns></returns>
        private static IList<(long, string)> ProcessRatingsAggregationsResult(
            ConcurrentDictionary<string, List<StatisticsAggregationResult>> statisticsDictionary,
            OrderDir orderDirection,
            ISet<long> alreadyContainedIds)
        {
            var sortedListWithSortValues = new List<(long, string)>();
            if (statisticsDictionary?.Count > 0)
            {
                //retrieve specific aggregation result
                statisticsDictionary.TryGetValue("stats", out var statResult);

                if (statResult != null && statResult.Count > 0)
                {
                    // sort ASCENDING - different than normal execution!
                    statResult.Sort(new AggregationsComparer(AggregationsComparer.eCompareType.Average));

                    foreach (var result in statResult)
                    {
                        // Depending on direction - if it is ascending, insert Id at end. Otherwise at start
                        if (int.TryParse(result.key, out var currentId))
                        {
                            if (orderDirection == OrderDir.ASC)
                            {
                                sortedListWithSortValues.Insert(0, (currentId, result.avg.ToString(CultureInfo.InvariantCulture)));
                            }
                            else
                            {
                                sortedListWithSortValues.Add((currentId, result.avg.ToString(CultureInfo.InvariantCulture)));
                            }

                            alreadyContainedIds.Add(currentId);
                        }
                    }
                }
            }

            return sortedListWithSortValues;
        }

        /// <summary>
        /// After receiving a result from ES server, process it to create a list of Ids with the given order
        /// </summary>
        /// <param name="statsDictionary"></param>
        /// <param name="orderDirection"></param>
        /// <param name="alreadyContainedIds"></param>
        /// <returns></returns>
        private static IList<(long, string)> ProcessCountDictionaryResults(
            ConcurrentDictionary<string, ConcurrentDictionary<string, int>> statsDictionary,
            OrderDir orderDirection,
            ISet<long> alreadyContainedIds)
        {
            var sortedListWithSortValues = new List<(long, string)>();
            if (statsDictionary?.Count > 0)
            {
                //retrieve specific stats result
                statsDictionary.TryGetValue("stats", out var statResult);

                if (statResult != null && statResult.Count > 0)
                {
                    var sortedStatsDictionary = statResult.OrderBy(o => o.Value).ThenBy(o => o.Key).Reverse();

                    // We base this section on the assumption that aggregations request is sorted, descending
                    foreach (var currentValue in sortedStatsDictionary)
                    {
                        if (int.TryParse(currentValue.Key, out var currentId))
                        {
                            // Depending on direction - if it is ascending, insert Id at start. Otherwise at end
                            var sortValue = currentValue.Value.ToString();
                            // TODO: Decide what we could refactor there.
                            if (orderDirection == OrderDir.ASC)
                            {
                                sortedListWithSortValues.Insert(0, (currentId, sortValue));
                            }
                            else
                            {
                                sortedListWithSortValues.Add((currentId, sortValue));
                            }

                            alreadyContainedIds.Add(currentId);
                        }
                    }
                }
            }

            return sortedListWithSortValues;
        }

        private Dictionary<string, int> SortAssetsByStatsWithLayeredCache(IEnumerable<long> assetIds, OrderBy orderBy, OrderDir orderDirection, int partnerId)
        {
            var result = new Dictionary<string, int>();
            var layeredCacheResult = new Dictionary<string, int>();
            if (assetIds != null && assetIds.Any())
            {
                try
                {
                    var keyToOriginalValueMap = assetIds.Select(x => x.ToString()).ToDictionary(x => LayeredCacheKeys.GetAssetStatsSortKey(x, orderBy.ToString()));
                    var invalidationKeys =
                        keyToOriginalValueMap.Keys.ToDictionary(x => x, x => new List<string>() {LayeredCacheKeys.GetAssetStatsInvalidationKey(partnerId)});

                    var funcParams = new Dictionary<string, object>
                    {
                        { "orderBy", orderBy },
                        { "assetIds", assetIds },
                        { "partnerId", partnerId }
                    };

                    if (!_layeredCache.GetValues<int>(keyToOriginalValueMap, ref layeredCacheResult, SortAssetsByStatsDelegate, funcParams,
                        partnerId, LayeredCacheConfigNames.ASSET_STATS_SORT_CONFIG_NAME, invalidationKeys))
                    {
                        _log.ErrorFormat("Failed getting asset stats from cache, ids: {0}:", assetIds.Count() < 100 ? string.Join(",", assetIds) : string.Join(",", assetIds.Take(100)));
                    }
                    else
                    {
                        foreach (var item in layeredCacheResult)
                        {
                            var key = keyToOriginalValueMap[item.Key];
                            result.Add(key, item.Value);
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
        
        private Tuple<Dictionary<string, int>, bool> SortAssetsByStatsDelegate(Dictionary<string, object> funcParams)
        {
            var countsAggregationsDictionary = new ConcurrentDictionary<string, ConcurrentDictionary<string, int>>();
            var result = new Dictionary<string, int>();
            var success = true;
            var assetIds = new List<long>();
            var orderBy = OrderBy.NONE;
            var partnerId = int.Parse(funcParams["partnerId"].ToString());

            try
            {
                // extract from funcParams
                if (funcParams.ContainsKey("orderBy"))
                {
                    orderBy = (OrderBy) funcParams["orderBy"];
                }

                // if we don't have missing keys - all ids should be sent
                if (funcParams.ContainsKey(LayeredCache.MISSING_KEYS) && funcParams[LayeredCache.MISSING_KEYS] != null)
                {
                    assetIds = ((List<string>)funcParams[LayeredCache.MISSING_KEYS]).Select(long.Parse).ToList();
                }
                else if (funcParams.ContainsKey("assetIds"))
                {
                    assetIds = (List<long>) funcParams["assetIds"];
                }

                GetAssetStatsValuesFromElasticSearch(assetIds, orderBy, null, null, null, countsAggregationsDictionary, partnerId);

                var statsDictionary = countsAggregationsDictionary["stats"];

                // fill dictionary of asset-id..stats-value (if it doesn't exist in ES, fill it with a 0)
                foreach (var assetId in assetIds)
                {
                    var dictionaryKey = LayeredCacheKeys.GetAssetStatsSortKey(assetId.ToString(), orderBy.ToString());

                    if (!statsDictionary.ContainsKey(assetId.ToString()))
                    {
                        result[dictionaryKey] = 0;
                    }
                    else
                    {
                        result[dictionaryKey] = statsDictionary[assetId.ToString()];
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
        
        private void GetAssetStatsValuesFromElasticSearch(
            IEnumerable<long> assetIds,
            OrderBy orderBy,
            DateTime? startDate, 
            DateTime? endDate,
            ConcurrentDictionary<string, List<StatisticsAggregationResult>> ratingsAggregationsDictionary,
            ConcurrentDictionary<string, ConcurrentDictionary<string, int>> countsAggregationsDictionary,
            int partnerId)
        {
            #region Define Aggregation Query

            var filteredQuery = new FilteredQuery
            {
                PageIndex = 0,
                PageSize = 1,
                Filter = new QueryFilter()
            };

            BaseFilterCompositeType filter = new FilterCompositeType(CutWith.AND);

            filter.AddChild(new ESTerm(true)
            {
                Key = "group_id",
                Value = partnerId.ToString()
            });

            #region define date filter

            if ((startDate.HasValue && !startDate.Equals(DateTime.MinValue)) ||
                (endDate.HasValue && !endDate.Equals(DateTime.MaxValue)))
            {
                var dateRange = new ESRange(false)
                {
                    Key = "action_date"
                };

                if (startDate != null && startDate.HasValue && !startDate.Equals(DateTime.MinValue))
                {
                    var sMin = startDate.Value.ToString(Utils.ES_DATE_FORMAT);
                    dateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, sMin));
                }

                if (endDate != null && endDate.HasValue && !endDate.Equals(DateTime.MaxValue))
                {
                    var sMax = endDate.Value.ToString(Utils.ES_DATE_FORMAT);
                    dateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, sMax));
                }

                filter.AddChild(dateRange);
            }

            #endregion

            #region define action filter

            var actionName = string.Empty;
            switch (orderBy)
            {
                case OrderBy.VIEWS:
                {
                    actionName = NamingHelper.STAT_ACTION_FIRST_PLAY;
                    break;
                }
                case OrderBy.RATING:
                {
                    actionName = NamingHelper.STAT_ACTION_RATES;
                    break;
                }
                case OrderBy.VOTES_COUNT:
                {
                    actionName = NamingHelper.STAT_ACTION_RATES;
                    break;
                }
                case OrderBy.LIKE_COUNTER:
                {
                    actionName = NamingHelper.STAT_ACTION_LIKE;
                    break;
                }
                default:
                {
                    break;
                }
            }

            var actionTerm = new ESTerm(false)
            {
                Key = "action",
                Value = actionName
            };

            filter.AddChild(actionTerm);

            #endregion

            #region Define IDs term

            var idsTerm = new ESTerms(true)
            {
                Key = "media_id"
            };

            idsTerm.Value.Add("0");

            filter.AddChild(idsTerm);

            #endregion

            filteredQuery.Filter.FilterSettings = filter;

            ESBaseAggsItem aggregations;

            // Ratings is a special case, because it is not based on count, but on average instead
            if (orderBy == OrderBy.RATING)
            {
                aggregations = new ESBaseAggsItem()
                {
                    Name = "stats",
                    Field = "media_id",
                    Type = eElasticAggregationType.terms
                };

                aggregations.SubAggrgations.Add(new ESBaseAggsItem()
                {
                    Name = "sub_stats",
                    Type = eElasticAggregationType.stats,
                    Field = NamingHelper.STAT_ACTION_RATE_VALUE_FIELD
                });
            }
            else
            {
                aggregations = new ESBaseAggsItem()
                {
                    Name = "stats",
                    Field = "media_id",
                    Type = eElasticAggregationType.terms
                };

                aggregations.SubAggrgations.Add(new ESBaseAggsItem()
                {
                    Name = NamingHelper.SUB_SUM_AGGREGATION_NAME,
                    Type = eElasticAggregationType.sum,
                    Field = NamingHelper.STAT_ACTION_COUNT_VALUE_FIELD,
                    Missing = 1
                });
            }

            filteredQuery.Aggregations.Add(aggregations);

            #endregion

            #region Split call of aggregations query to pieces

            var aggregationsSize = ApplicationConfiguration.Current.ElasticSearchConfiguration.StatSortBulkSize.Value;

            //Start MultiThread Call
            var tasks = new List<Task>();

            // Split the request to small pieces, to avoid timeout exceptions
            var count = assetIds.Count();
            for (var assetIndex = 0; assetIndex < count; assetIndex += aggregationsSize)
            {
                idsTerm.Value.Clear();

                // Convert partial Ids to strings
                idsTerm.Value.AddRange(assetIds.Skip(assetIndex).Take(aggregationsSize).Select(id => id.ToString()));

                var aggregationsRequestBody = filteredQuery.ToString();
                var index = NamingHelper.GetStatisticsIndexName(partnerId);

                try
                {
                    var contextData = new LogContextData();
                    // Create a task for the search and merge of partial aggregations
                    var task = Task.Run(() =>
                    {
                        contextData.Load();
                        // Get aggregations results
                        var aggregationsResults = _elasticSearchApi.Search(index, Utils.ES_STATS_TYPE, ref aggregationsRequestBody);

                        if (orderBy == OrderBy.RATING)
                        {
                            if (ratingsAggregationsDictionary != null)
                            {
                                // Parse string into dictionary
                                var partialDictionary = ESAggregationsResult.DeserializeStatisticsAggregations(aggregationsResults, "sub_stats");

                                // Run on partial dictionary and merge into main dictionary
                                foreach (var mainPart in partialDictionary)
                                {
                                    if (!ratingsAggregationsDictionary.ContainsKey(mainPart.Key))
                                    {
                                        ratingsAggregationsDictionary[mainPart.Key] = new List<StatisticsAggregationResult>();
                                    }

                                    foreach (var singleResult in mainPart.Value)
                                    {
                                        ratingsAggregationsDictionary[mainPart.Key].Add(singleResult);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (countsAggregationsDictionary != null)
                            {
                                // Parse string into dictionary
                                var partialDictionary = ESAggregationsResult.DeserializeAggrgations<string>(aggregationsResults, NamingHelper.SUB_SUM_AGGREGATION_NAME);

                                // Run on partial dictionary and merge into main dictionary
                                foreach (var mainPart in partialDictionary)
                                {
                                    if (!countsAggregationsDictionary.ContainsKey(mainPart.Key))
                                    {
                                        countsAggregationsDictionary[mainPart.Key] = new ConcurrentDictionary<string, int>();
                                    }

                                    foreach (var singleResult in mainPart.Value)
                                    {
                                        countsAggregationsDictionary[mainPart.Key][singleResult.Key] = singleResult.Value;
                                    }
                                }
                            }
                        }
                    });

                    tasks.Add(task);
                }
                catch (Exception ex)
                {
                    _log.ErrorFormat("Error in SortAssetsByStatsWithSortValues, Exception: {0}", ex);
                }
            }

            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in SortAssetsByStatsWithSortValues (WAIT ALL), Exception: {0}", ex);
            }

            #endregion
        }
    }
}
