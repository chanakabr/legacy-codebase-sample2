using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiLogic.IndexManager.Helpers;
using ApiLogic.IndexManager.Models;
using ApiLogic.IndexManager.NestData;
using ApiObjects;
using ApiObjects.SearchObjects;
using ElasticSearch.NEST;
using Nest;
using Phx.Lib.Appconfig;

namespace ApiLogic.IndexManager.Sorting
{
    public class StartDateAssociationTagsSortStrategyV7 : IStartDateAssociationTagsSortStrategy
    {
        private readonly IElasticClient _elasticClient;

        private static readonly Lazy<IStartDateAssociationTagsSortStrategy> LazyValue =
            new Lazy<IStartDateAssociationTagsSortStrategy>(
                () => new StartDateAssociationTagsSortStrategyV7(NESTFactory.GetInstance(ApplicationConfiguration.Current)),
                LazyThreadSafetyMode.PublicationOnly);
        
        public static IStartDateAssociationTagsSortStrategy Instance => LazyValue.Value;
    
        public StartDateAssociationTagsSortStrategyV7(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        public IEnumerable<(long id, string sortValue)> SortAssetsByStartDate(
            IEnumerable<ExtendedUnifiedSearchResult> extendedUnifiedSearchResults,
            LanguageObj languageObj,
            OrderDir orderDirection,
            Dictionary<int, string> associationTags,
            Dictionary<int, int> parentMediaTypes,
            int partnerId)
        {
            return SortAssetsByStartDateInternal(extendedUnifiedSearchResults.ToDictionary(x => x, x => x.DocAdapter),
                    extendedUnifiedSearchResults,
                    languageObj,
                    orderDirection,
                    associationTags,
                    parentMediaTypes,
                    partnerId)
                .Select(x => (x.item.AssetId, x.sortValue))
                .ToArray();
        }

        public IEnumerable<(long id, string sortValue)> SortAssetsByStartDate(
            IEnumerable<ExtendedUnifiedSearchResult> extendedUnifiedSearchResults,
            OrderDir orderDirection,
            UnifiedSearchDefinitions unifiedSearchDefinitions)
        {
            return SortAssetsByStartDate(extendedUnifiedSearchResults,
                unifiedSearchDefinitions.langauge,
                orderDirection,
                unifiedSearchDefinitions.associationTags,
                unifiedSearchDefinitions.parentMediaTypes,
                unifiedSearchDefinitions.groupId);
        }

        private IEnumerable<(T item, string sortValue)> SortAssetsByStartDateInternal<T>(
            IReadOnlyDictionary<T, EsAssetAdapter> resultsToHits,
            IEnumerable<T> searchResults,
            LanguageObj languageObj,
            OrderDir orderDirection,
            Dictionary<int, string> associationTags,
            Dictionary<int, int> parentMediaTypes,
            int partnerId)
        {
            if (searchResults == null || !searchResults.Any())
            {
                return new List<(T item, string sortValue)>();
            }

            bool shouldSearch = false;
            var idToStartDate = new Dictionary<T, DateTime>();
            var nameToTypeToId = new Dictionary<string, Dictionary<int, List<T>>>();
            var typeToNames = new Dictionary<int, List<string>>();

            #region Map documents name and initial start dates

            // Create mappings for later on
            foreach (var searchResult in searchResults)
            {
                var asset = resultsToHits[searchResult];
                idToStartDate.Add(searchResult, asset.StartDate);

                var mediaTypeId = asset.MediaTypeId;
                if (mediaTypeId > 0)
                {
                    // Unused variable.
                    // var mediaId = asset.Fields.Value<object>("media_id");

                    var name = asset.Name;
                    if (!nameToTypeToId.ContainsKey(name))
                    {
                        nameToTypeToId[name] = new Dictionary<int, List<T>>();
                    }

                    if (!nameToTypeToId[name].ContainsKey(mediaTypeId))
                    {
                        nameToTypeToId[name][mediaTypeId] = new List<T>();
                    }

                    nameToTypeToId[name][mediaTypeId].Add(searchResult);

                    if (!typeToNames.ContainsKey(mediaTypeId))
                    {
                        typeToNames[mediaTypeId] = new List<string>();
                    }

                    typeToNames[mediaTypeId].Add(name);
                }
            }

            #endregion

            #region Define Aggregations Search Query

            List<QueryContainer> tagsShoulds = new List<QueryContainer>();
            // Filter data only to contain documents that have the specifiic tag
            foreach (var item in associationTags)
            {
                if (parentMediaTypes.ContainsKey(item.Key) &&
                    typeToNames.ContainsKey(parentMediaTypes[item.Key]))
                {
                    shouldSearch = true;
                    var tagsTerms = new TermsQuery()
                    {
                        Field = $"tags.{languageObj.Code}.{item.Value.ToLower()}",
                        Terms = typeToNames[parentMediaTypes[item.Key]]
                    };

                    tagsShoulds.Add(tagsTerms);
                }
            }

            var tagsQuery = new BoolQuery()
            {
                Should = tagsShoulds
            };

            if (shouldSearch)
            {
                BoolQuery searchQuery = BuildSortAssetsByStartDateQuery(tagsQuery);
                AggregationDictionary aggs = BuildSortAssetsByStartDateAggs(associationTags, languageObj);

                #endregion

                #region Get Aggregations Results

                string index = NamingHelper.GetMediaIndexAlias(partnerId);

                var searchResponse = _elasticClient.Search<NestMedia>(searchRequest => searchRequest
                    // the hits themselves don't matter to us, we only want the aggs
                    .Size(0)
                    .Index(index)
                    .Query(selector => searchQuery)
                    .Aggregations(aggs)
                );
                ProcessSortByStartDateResponse(associationTags, parentMediaTypes, idToStartDate, nameToTypeToId, searchResponse);

                #endregion
            }

            // Sort the list of key value pairs by the value (the start date)
            var sortedDictionary = idToStartDate.OrderBy(pair => pair.Value).ThenBy(pair => pair.Key);

            #region Create final, sorted, list

            var sortedList = new List<(T item, string sortValue)>();
            var alreadyContainedIds = new HashSet<T>();

            foreach (var currentId in sortedDictionary)
            {
                var item = (currentId.Key, currentId.Value.ToString("s"));
                // Depending on direction - if it is ascending, insert Id at start. Otherwise at end
                if (orderDirection == ApiObjects.SearchObjects.OrderDir.DESC)
                {
                    sortedList.Insert(0, item);
                }
                else
                {
                    sortedList.Add(item);
                }

                alreadyContainedIds.Add(currentId.Key);
            }

            // Add all ids that don't have stats
            foreach (var searchResult in searchResults)
            {
                if (!alreadyContainedIds.Contains(searchResult))
                {
                    var item = (searchResult, (string) null);
                    // Depending on direction - if it is ascending, insert Id at start. Otherwise at end
                    if (orderDirection == ApiObjects.SearchObjects.OrderDir.ASC)
                    {
                        sortedList.Insert(0, item);
                    }
                    else
                    {
                        sortedList.Add(item);
                    }
                }
            }

            #endregion

            return sortedList;
        }

        private static BoolQuery BuildSortAssetsByStartDateQuery(BoolQuery tagsQuery)
        {
            var descriptor = new QueryContainerDescriptor<NestMedia>();
            var rootQuerymusts = new List<QueryContainer>();

            var isActiveTerm = descriptor.Term(field => field.IsActive, true);

            string nowSearchString = DateTime.UtcNow.ToString(ElasticSearch.Common.Utils.ES_DATE_FORMAT);

            var startDateRange = descriptor.DateRange(selector =>
                selector.Field(field => field.StartDate).LessThanOrEquals(DateTime.UtcNow));

            var endDateRange = descriptor.DateRange(selector =>
                selector.Field(field => field.EndDate).GreaterThanOrEquals(DateTime.UtcNow));

            // Filter associated media by:
            // is_active = 1
            // start_date < NOW
            // end_date > NOW
            // tag is actually the current series
            rootQuerymusts.Add(isActiveTerm);
            rootQuerymusts.Add(startDateRange);
            rootQuerymusts.Add(endDateRange);
            rootQuerymusts.Add(tagsQuery);

            BoolQuery searchQuery = new BoolQuery()
            {
                Must = rootQuerymusts
            };

            return searchQuery;
        }
        
        private static AggregationDictionary BuildSortAssetsByStartDateAggs(Dictionary<int, string> associationTags, LanguageObj language)
        {
            var aggsDesctiptor = new AggregationContainerDescriptor<NestMedia>();
            var descriptor = new QueryContainerDescriptor<NestMedia>();
            AggregationDictionary aggs = new AggregationDictionary();

            // Create an aggregation search object for each association tag we have
            foreach (var associationTag in associationTags)
            {
                var filter = descriptor.Term(field => field.MediaTypeId, associationTag.Key);

                var currentAggregation = new FilterAggregation(associationTag.Value)
                {
                    Filter = filter
                };

                string subName1 = $"{associationTag.Value}_sub1";
                var subAggregation1 = new TermsAggregation(subName1)
                {
                    Field = $"tags.{language.Code}.{associationTag.Value.ToLower()}",
                };

                string subName2 = $"{associationTag.Value}_sub2";
                var subAggregation2 = new MaxAggregation(subName2, "start_date");

                subAggregation1.Aggregations = new AggregationDictionary();
                subAggregation1.Aggregations.Add(subName2, subAggregation2);
                currentAggregation.Aggregations = new AggregationDictionary();
                currentAggregation.Aggregations.Add(subName1, subAggregation1);

                aggs.Add(associationTag.Value, currentAggregation);
            }

            return aggs;
        }
        
        private static void ProcessSortByStartDateResponse<T>(Dictionary<int, string> associationTags, 
            Dictionary<int, int> parentMediaTypes, 
            Dictionary<T, DateTime> idToStartDate, 
            Dictionary<string, Dictionary<int, List<T>>> nameToTypeToId, 
            ISearchResponse<NestMedia> searchResponse)
        {
            if (searchResponse.IsValid && searchResponse.Aggregations != null && searchResponse.Aggregations.Count > 0)
            {
                foreach (var associationTag in associationTags)
                {
                    int parentMediaType = parentMediaTypes[associationTag.Key];

                    var currentResult = searchResponse.Aggregations.Filter(associationTag.Value);

                    if (currentResult != null)
                    {
                        var firstSub = currentResult.Terms($"{associationTag.Value}_sub1");

                        if (firstSub != null)
                        {
                            foreach (var bucket in firstSub.Buckets)
                            {
                                var subBucket = bucket.Max($"{associationTag.Value}_sub2");

                                if (subBucket != null)
                                {
                                    // "series name" is the bucket's key
                                    string tagValue = bucket.Key;

                                    if (nameToTypeToId.ContainsKey(tagValue) && nameToTypeToId[tagValue].ContainsKey(parentMediaType))
                                    {
                                        foreach (var unifiedSearchResult in nameToTypeToId[tagValue][parentMediaType])
                                        {
                                            var maximumDateEpoch = subBucket.Value.HasValue ? subBucket.Value.Value : 0;
                                            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                                            var maximumDate = epoch.AddMilliseconds(maximumDateEpoch).ToUniversalTime();

                                            idToStartDate[unifiedSearchResult] = maximumDate;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
