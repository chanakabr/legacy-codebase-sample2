using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiLogic.IndexManager.Helpers;
using ApiObjects.SearchObjects;
using ConfigurationManager;
using ElasticSearch.Common;
using ElasticSearch.Searcher;

namespace ApiLogic.IndexManager.Sorting
{
    public class StartDateAssociationTagsSortStrategy : IStartDateAssociationTagsSortStrategy
    {
        private readonly IElasticSearchApi _elasticSearchApi;

        private static readonly Lazy<IStartDateAssociationTagsSortStrategy> LazyValue =
            new Lazy<IStartDateAssociationTagsSortStrategy>(
                () => new StartDateAssociationTagsSortStrategy(new ElasticSearchApi(ApplicationConfiguration.Current)),
                LazyThreadSafetyMode.PublicationOnly);

        public static IStartDateAssociationTagsSortStrategy Instance => LazyValue.Value;

        public StartDateAssociationTagsSortStrategy(IElasticSearchApi elasticSearchApi)
        {
            _elasticSearchApi = elasticSearchApi;
        }
        
        public List<long> SortAssetsByStartDate(
            IEnumerable<ElasticSearchApi.ESAssetDocument> assets, 
            OrderDir orderDirection, 
            Dictionary<int, string> associationTags,
            Dictionary<int, int> mediaTypeParent,
            int partnerId)
        {
            if (assets == null || !assets.Any())
            {
                return new List<long>();
            }

            var search = false;
            var idToStartDate = new Dictionary<string, DateTime>();
            var nameToTypeToId = new Dictionary<string, Dictionary<int, List<string>>>();
            var typeToNames = new Dictionary<int, List<string>>();

            #region Map documents name and initial start dates

            // Create mappings for later on
            foreach (var document in assets)
            {
                idToStartDate.Add(document.id, document.start_date);

                if (document.media_type_id > 0)
                {
                    if (!nameToTypeToId.ContainsKey(document.name))
                    {
                        nameToTypeToId[document.name] = new Dictionary<int, List<string>>();
                    }

                    if (!nameToTypeToId[document.name].ContainsKey(document.media_type_id))
                    {
                        nameToTypeToId[document.name][document.media_type_id] = new List<string>();
                    }

                    nameToTypeToId[document.name][document.media_type_id].Add(document.id);

                    if (!typeToNames.ContainsKey(document.media_type_id))
                    {
                        typeToNames[document.media_type_id] = new List<string>();
                    }

                    typeToNames[document.media_type_id].Add(document.name);
                }
            }

            #endregion

            #region Define Aggregations Search Query

            var filteredQuery = new FilteredQuery()
            {
                PageIndex = 0,
                PageSize = 1
            };

            filteredQuery.Filter = new QueryFilter();

            var filterSettings = new FilterCompositeType(CutWith.AND);

            var tagsFilter = new FilterCompositeType(CutWith.OR);

            // Filter data only to contain documents that have the specifiic tag
            foreach (var item in associationTags)
            {
                if (mediaTypeParent.ContainsKey(item.Key) &&
                    typeToNames.ContainsKey(mediaTypeParent[item.Key]))
                {
                    search = true;
                    var tagsTerms = new ESTerms(false)
                    {
                        Key = string.Format("tags.{0}", item.Value.ToLower())
                    };

                    tagsTerms.Value.AddRange(typeToNames[mediaTypeParent[item.Key]]);

                    tagsFilter.AddChild(tagsTerms);
                }
            }

            if (search)
            {
                var isActiveTerm = new ESTerm(true)
                {
                    Key = "is_active",
                    Value = "1"
                };

                var nowSearchString = DateTime.UtcNow.ToString(ElasticSearch.Common.Utils.ES_DATE_FORMAT);

                var startDateRange = new ESRange(false)
                {
                    Key = "start_date"
                };

                startDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, nowSearchString));

                var endDateRange = new ESRange(false)
                {
                    Key = "end_date"
                };

                // If we don't have any tag, use a "0=1" filter so query return 0 results instead of ALL results
                if (tagsFilter.IsEmpty())
                {
                    tagsFilter.AddChild(new ESTerm(true)
                    {
                        Key = "_id",
                        Value = "-1"
                    });
                }

                // Filter associated media by:
                // is_active = 1
                // start_date < NOW
                // end_date > NOW
                // tag is actually the current series
                filterSettings.AddChild(isActiveTerm);
                filterSettings.AddChild(startDateRange);
                filterSettings.AddChild(endDateRange);
                filterSettings.AddChild(tagsFilter);
                filteredQuery.Filter.FilterSettings = filterSettings;

                // Create an aggregation search object for each association tag we have
                foreach (var associationTag in associationTags)
                {
                    var filter = new ESTerm(true)
                    {
                        Key = "media_type_id",
                        // key of association tag is the child media type
                        Value = associationTag.Key.ToString()
                    };

                    var currentAggregation = new ESFilterAggregation(filter)
                    {
                        Name = associationTag.Value
                    };

                    var subAggregation1 = new ESBaseAggsItem()
                    {
                        Name = associationTag.Value + "_sub1",
                        Field = string.Format("tags.{0}", associationTag.Value).ToLower(),
                        Type = eElasticAggregationType.terms
                    };

                    var subAggregation2 = new ESBaseAggsItem()
                    {
                        Name = associationTag.Value + "_sub2",
                        Field = "start_date",
                        Type = eElasticAggregationType.stats
                    };

                    subAggregation1.SubAggrgations.Add(subAggregation2);
                    currentAggregation.SubAggrgations.Add(subAggregation1);

                    filteredQuery.Aggregations.Add(currentAggregation);
                }

                #endregion

                #region Get Aggregations Results

                var searchRequestBody = filteredQuery.ToString();
                var index = NamingHelper.GetMediaIndexAlias(partnerId);

                var searchResults = _elasticSearchApi.Search(index, "media", ref searchRequestBody);

                var aggregationsResult = ESAggregationsResult.FullParse(searchResults, filteredQuery.Aggregations);

                #endregion

                #region Process Aggregations Results

                if (aggregationsResult != null && aggregationsResult.Aggregations != null && aggregationsResult.Aggregations.Count > 0)
                {
                    foreach (var associationTag in associationTags)
                    {
                        var parentMediaType = mediaTypeParent[associationTag.Key];

                        if (aggregationsResult.Aggregations.ContainsKey(associationTag.Value))
                        {
                            var currentResult = aggregationsResult.Aggregations[associationTag.Value];

                            if (currentResult.Aggregations.TryGetValue(associationTag.Value + "_sub1", out var firstSub))
                            {
                                foreach (var bucket in firstSub.buckets)
                                {
                                    if (bucket.Aggregations.TryGetValue(associationTag.Value + "_sub2", out var subBucket))
                                    {
                                        // "series name" is the bucket's key
                                        var tagValue = bucket.key;
                                        if (nameToTypeToId.ContainsKey(tagValue) && nameToTypeToId[tagValue].ContainsKey(parentMediaType))
                                        {
                                            foreach (var assetId in nameToTypeToId[tagValue][parentMediaType])
                                            {
                                                var maximumStartDate = subBucket.max_as_string;
                                                idToStartDate[assetId] = DateTime.ParseExact(maximumStartDate, Utils.ES_DATE_FORMAT, null);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            // Sort the list of key value pairs by the value (the start date)
            var sortedDictionary = idToStartDate.OrderBy(pair => pair.Value).ThenBy(pair => pair.Key);

            #region Create final, sorted, list

            var sortedList = new List<long>();
            var alreadyContainedIds = new HashSet<int>();

            foreach (var currentId in sortedDictionary)
            {
                var id = int.Parse(currentId.Key);

                // Depending on direction - if it is ascending, insert Id at start. Otherwise at end
                if (orderDirection == OrderDir.DESC)
                {
                    sortedList.Insert(0, id);
                }
                else
                {
                    sortedList.Add(id);
                }

                alreadyContainedIds.Add(id);
            }

            // Add all ids that don't have stats
            foreach (var asset in assets)
            {
                var currentId = int.Parse(asset.id);

                if (!alreadyContainedIds.Contains(currentId))
                {
                    // Depending on direction - if it is ascending, insert Id at start. Otherwise at end
                    if (orderDirection == OrderDir.ASC)
                    {
                        sortedList.Insert(0, currentId);
                    }
                    else
                    {
                        sortedList.Add(currentId);
                    }
                }
            }

            #endregion

            return sortedList;
        }
    }
}
