using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ApiObjects.SearchObjects;
using Phx.Lib.Appconfig;
using Newtonsoft.Json.Linq;
using Phx.Lib.Log;
using TVinciShared;
using OrderDir = ApiObjects.SearchObjects.OrderDir;
using ElasticSearch.Searcher;
using Nest;
using ApiLogic.IndexManager.Helpers;
using ApiLogic.IndexManager.QueryBuilders.ESV2QueryBuilders;
using ApiLogic.IndexManager.Sorting;
using ApiObjects;
using ElasticSearch.Utils;

namespace ApiLogic.IndexManager.QueryBuilders
{
    public class ESUnifiedQueryBuilder : BaseEsUnifiedQueryBuilder
    {
        #region Consts and readonlys 

        private static readonly KLogger log = new KLogger(nameof(ESUnifiedQueryBuilder));

        protected static readonly List<string> DEFAULT_RETURN_FIELDS = new List<string>(7) {
                "\"_id\"", "\"_index\"", "\"_type\"", "\"_score\"", "\"group_id\"", "\"name\"", "\"cache_date\"",  "\"update_date\""};

        public static readonly string AND_CONDITION = "AND";
        public static readonly string OR_CONDITION = "OR";
        public static readonly string METAS = "METAS";
        public static readonly string TAGS = "TAGS";
        public static readonly string ES_DATE_FORMAT = "yyyyMMddHHmmss";

        public static readonly int MissedHitBucketKey = 999;

        protected static readonly Dictionary<string, string> NONE_PHONETIC_LANGUAGES 
            = new Dictionary<string, string> { { "heb", @"[\u0590-\u05FF]+" } };

        private static eFieldType[] LanguageSpecificGroupByFieldTypes => new []
        {
            eFieldType.LanguageSpecificField, eFieldType.Tag, eFieldType.StringMeta, eFieldType.NonStringMeta
        };


        protected static readonly ESPrefix epgPrefixTerm = new ESPrefix()
        {
            Key = "_type",
            Value = "epg"
        };

        protected static readonly ESPrefix mediaPrefixTerm = new ESPrefix()
        {
            Key = "_type",
            Value = "media"
        };

        protected static readonly ESPrefix recordingPrefixTerm = new ESPrefix()
        {
            Key = "_type",
            Value = "recording"
        };

        #endregion

        #region Data Members

        protected static int MAX_RESULTS;

        public int GroupID
        {
            get;
            set;
        }

        public List<string> ReturnFields
        {
            get;
            set;
        }

        /// <summary>
        /// A search query/filter represeting the search for entitled assets
        /// </summary>
        public string EntitlementSearchQuery
        {
            get;
            set;
        }

        /// <summary>
        /// A bool query containing all
        /// </summary>
        public ElasticSearch.Searcher.BoolQuery SubscriptionsQuery
        {
            get;
            set;
        }

        public List<ESBaseAggsItem> Aggregations
        {
            get;
            set;
        }

        #endregion

        #region Ctor

        /// <summary>
        /// Static constructor that initailizes TCM consts
        /// </summary>
        static ESUnifiedQueryBuilder()
        {
            MAX_RESULTS = ApplicationConfiguration.Current.ElasticSearchConfiguration.MaxResults.Value;

            if (MAX_RESULTS == 0)
            {
                MAX_RESULTS = 10000;
            }
        }

        /// <summary>
        /// Regular constructor to initialize with definitions
        /// </summary>
        /// <param name="definitions"></param>
        public ESUnifiedQueryBuilder(
            IEsSortingService esSortingService,
            ISortingAdapter sortingAdapter,
            IUnifiedQueryBuilderInitializer queryInitializer,
            UnifiedSearchDefinitions definitions,
            int groupId = 0)
            : base(esSortingService, sortingAdapter, queryInitializer)
        {
            this.SearchDefinitions = definitions;
            this.GroupID = definitions?.groupId ?? groupId;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Builds the request body for Elasticsearch search action
        /// </summary>
        /// <returns></returns>
        public virtual string BuildSearchQueryString(
            bool bIgnoreDeviceRuleID = false,
            bool bAddActive = true,
            bool addMissingToGroupByAgg = false)
        {
            this.ReturnFields = DEFAULT_RETURN_FIELDS.ToList();
            SearchDefinitions.extraReturnFields.UnionWith(EsSortingService.BuildExtraReturnFields(OrderByFields));
            this.ReturnFields.AddRange(SearchDefinitions.extraReturnFields.Select(field => string.Format("\"{0}\"", field)));

            string epg_id_field = "\"epg_id\"";

            if (this.SearchDefinitions.shouldSearchEpg)
            {
                this.ReturnFields.Add(epg_id_field);
            }

            if (this.SearchDefinitions.shouldSearchMedia)
            {
                this.ReturnFields.Add("\"media_id\"");
            }

            if (this.SearchDefinitions.shouldSearchRecordings)
            {
                if (!this.ReturnFields.Contains(epg_id_field))
                {
                    this.ReturnFields.Add(epg_id_field);
                }

                this.ReturnFields.Add("\"recording_id\"");
            }

            var doc_id_field = "\"document_id\"";

            if (this.SearchDefinitions.isEpgV2)
            {
                if (!this.ReturnFields.Contains(doc_id_field))
                {
                    this.ReturnFields.Add(doc_id_field);
                }
            }

            // Also return the language specific name field
            if (this.SearchDefinitions.langauge != null &&
                !this.SearchDefinitions.langauge.IsDefault)
            {
                this.ReturnFields.Add(string.Format("\"name_{0}\"", this.SearchDefinitions.langauge.Code));
            }

            this.ReturnFields = this.ReturnFields.Distinct().ToList();

            // This is a query-filter.
            // First comes query
            // Then comes filter
            // Query is for non-exact phrases
            // Filter is for exact phrases

            // filtered query :
            //  {
            //      { 
            //          query : {},
            //          filter : {}
            //      }
            // }

            string fullQuery = string.Empty;

            if (this.SearchDefinitions == null)
            {
                return fullQuery;
            }

            BaseFilterCompositeType filterRoot;
            IESTerm queryTerm;

            BuildInnerFilterAndQuery(out filterRoot, out queryTerm, bIgnoreDeviceRuleID, bAddActive);

            QueryFilter filterPart = new QueryFilter()
            {
                FilterSettings = filterRoot
            };

            int pageSize = this.PageSize;

            if (this.GetAllDocuments)
            {
                pageSize = MAX_RESULTS;
            }
            else if (SearchDefinitions.topHitsCount > 0)
            {
                pageSize = 0;
            }

            int fromIndex = 0;

            // If we have a defined offset for search, use it
            if (this.From > 0)
            {
                fromIndex = this.From;
            }
            else
            {
                // Otherwise, we calculate the "from" by "page size times page index";
                fromIndex = (PageIndex <= 0) ? 0 : PageSize * PageIndex;
            }

            // TODO: make it interface
            if (fromIndex + pageSize > ApplicationConfiguration.Current.ElasticSearchConfiguration.MaxResults.Value)
            {
                log.WarnFormat("changing page size and index to 0 because size*index + size > max results configured, sent size: {0}, sent index: {1}", PageSize, PageIndex);
                fromIndex = 0;
                pageSize = 0;
            }

            StringBuilder filteredQueryBuilder = new StringBuilder();

            filteredQueryBuilder.Append("{");

            filteredQueryBuilder.AppendFormat(" \"size\": {0}, ", pageSize);
            filteredQueryBuilder.AppendFormat(" \"from\": {0}, ", fromIndex);

            // Join return fields with commas
            if (ReturnFields.Count > 0)
            {
                filteredQueryBuilder.Append("\"fields\": [");

                filteredQueryBuilder.Append(ReturnFields.Aggregate((current, next) => string.Format("{0}, {1}", current, next)));

                filteredQueryBuilder.Append("], ");
            }

            var isBoostScoreValues = this.SearchDefinitions.boostScoreValues != null && this.SearchDefinitions.boostScoreValues.Count > 0;
            var isPriorityGroupMappingsDefined = this.SearchDefinitions.PriorityGroupsMappings != null && this.SearchDefinitions.PriorityGroupsMappings.Any();
            bool functionScoreSort = isBoostScoreValues || isPriorityGroupMappingsDefined;
            var sortString = EsSortingService.GetSorting(OrderByFields, functionScoreSort);

            filteredQueryBuilder.AppendFormat("{0}, ", sortString);

            if (this.SearchDefinitions.groupBy != null && this.SearchDefinitions.groupBy.Count > 0)
            {
                this.Aggregations = new List<ESBaseAggsItem>();
                ESBaseAggsItem currentAggregation = null;

                string aggregationsOrder = string.Empty;
                string aggregationsOrderDirection = string.Empty;

                if (this.SearchDefinitions.groupByOrder != null && this.SearchDefinitions.groupByOrder.HasValue)
                {
                    switch (this.SearchDefinitions.groupByOrder.Value)
                    {
                        case AggregationOrder.Default:
                            break;
                        case AggregationOrder.Count_Asc:
                            {
                                aggregationsOrder = "_count";
                                aggregationsOrderDirection = "asc";
                                break;
                            }
                        case AggregationOrder.Count_Desc:
                            {
                                aggregationsOrder = "_count";
                                aggregationsOrderDirection = "desc";
                                break;
                            }
                        case AggregationOrder.Value_Asc:
                            {
                                aggregationsOrder = "_term";
                                aggregationsOrderDirection = "asc";
                                break;
                            }
                        case AggregationOrder.Value_Desc:
                            {
                                aggregationsOrder = "_term";
                                aggregationsOrderDirection = "desc";
                                break;
                            }
                        default:
                            break;
                    }
                }

                foreach (var groupBy in this.SearchDefinitions.groupBy)
                {
                    if (currentAggregation == null)
                    {
                        int size = 0;

                        if (this.GetAllDocuments && !ShouldPageGroups)
                        {
                            size = -1;
                        }
                        else if (this.SearchDefinitions.topHitsCount > 0 || !string.IsNullOrEmpty(SearchDefinitions.distinctGroup?.Key))
                        {
                            size = this.SearchDefinitions.pageSize * (this.SearchDefinitions.pageIndex + 1);
                        }

                        currentAggregation = new ESBaseAggsItem()
                        {
                            Field = groupBy.Value.ToLower(),
                            Name = groupBy.Key,
                            Size = size,
                            Type = eElasticAggregationType.terms,
                            Order = aggregationsOrder,
                            OrderDirection = aggregationsOrderDirection
                        };
                        if (addMissingToGroupByAgg)
                        {
                            currentAggregation.Missing = MissedHitBucketKey;
                            this.SearchDefinitions.topHitsCount = 10000; //allow missed bucket max results
                        }

                        // Get top hit as well if necessary
                        if (SearchDefinitions.topHitsCount > 0 || !string.IsNullOrEmpty(SearchDefinitions.distinctGroup?.Key))
                        {
                            int topHitsSize = -1;

                            if (this.SearchDefinitions.topHitsCount > 0)
                            {
                                topHitsSize = this.SearchDefinitions.topHitsCount;
                            }
                            else if (this.SearchDefinitions.topHitsCount == 0)
                            {
                                topHitsSize = MAX_RESULTS;
                            }

                            currentAggregation.SubAggrgations = new List<ESBaseAggsItem>
                            {
                                new ESTopHitsAggregation(EsSortingService)
                                {
                                    Name = ESTopHitsAggregation.DEFAULT_NAME,
                                    Size = topHitsSize,
                                    // order just like regular search
                                    EsOrderByFields = OrderByFields,
                                    SourceIncludes = this.ReturnFields
                                }
                            };

                            var orderAggregationParameters = GetOrderAggregationParameters(OrderByFields);
                            if (orderAggregationParameters?.OrderAggregation != null)
                            {
                                currentAggregation.SubAggrgations.Add(orderAggregationParameters.OrderAggregation);
                            }

                            if (!string.IsNullOrEmpty(orderAggregationParameters?.DistinctOrder))
                            {
                                currentAggregation.Order = orderAggregationParameters?.DistinctOrder;
                                currentAggregation.OrderDirection = orderAggregationParameters?.DistinctDirection;
                            }

                            ESBaseAggsItem cardinality = new ESBaseAggsItem()
                            {
                                Field = groupBy.Value.ToLower(),
                                Name = string.Format("{0}_count", groupBy.Key),
                                Type = eElasticAggregationType.cardinality,
                            };

                            this.Aggregations.Add(cardinality);
                        }

                        this.Aggregations.Add(currentAggregation);
                    }
                    else
                    {
                        var subAggregation = new ESBaseAggsItem()
                        {
                            Field = groupBy.Value.ToLower(),
                            Name = groupBy.Key,
                            Size = 0,
                            Type = eElasticAggregationType.terms,
                            Order = aggregationsOrder,
                            OrderDirection = aggregationsOrderDirection
                        };

                        currentAggregation.SubAggrgations.Add(subAggregation);

                        currentAggregation = subAggregation;
                    }
                }

                filteredQueryBuilder.Append("\"aggs\": {");

                foreach (ESBaseAggsItem item in this.Aggregations)
                {
                    filteredQueryBuilder.AppendFormat("{0},", item.ToString());
                }

                filteredQueryBuilder.Remove(filteredQueryBuilder.Length - 1, 1);
                filteredQueryBuilder.Append("},");
            }

            if (this.SearchDefinitions.PriorityGroupsMappings != null && this.SearchDefinitions.PriorityGroupsMappings.Any())
            {
                var priorityQueryBuilder = new PriorityQueryBuilder(this, this.SearchDefinitions.PriorityGroupsMappings);
                var functionScore = priorityQueryBuilder.Build(queryTerm, filterPart);

                filteredQueryBuilder.Append(" \"query\" :");
                filteredQueryBuilder.Append(functionScore);
                filteredQueryBuilder.Append("}");
            }
            else if (this.SearchDefinitions.boostScoreValues == null || this.SearchDefinitions.boostScoreValues.Count == 0)
            {
                filteredQueryBuilder.Append(" \"query\": { \"filtered\": {");

                if (queryTerm != null && !queryTerm.IsEmpty())
                {
                    string queryPart = queryTerm.ToString();

                    if (!string.IsNullOrEmpty(queryPart))
                    {
                        filteredQueryBuilder.AppendFormat(" \"query\": {0},", queryPart.ToString());
                    }
                }

                filteredQueryBuilder.Append(filterPart.ToString());
                filteredQueryBuilder.Append(" } } }");
            }
            else
            {
                StringBuilder esFilteredQueryBuilder = new StringBuilder();
                esFilteredQueryBuilder.Append("{ \"filtered\": {");

                if (queryTerm != null && !queryTerm.IsEmpty())
                {
                    string queryPart = queryTerm.ToString();

                    if (!string.IsNullOrEmpty(queryPart))
                    {
                        esFilteredQueryBuilder.AppendFormat(" \"query\": {0},", queryPart.ToString());
                    }
                }

                esFilteredQueryBuilder.Append(filterPart.ToString());
                esFilteredQueryBuilder.Append(" } }");

                ESFunctionScore functionScore = BuildFunctionScore(esFilteredQueryBuilder.ToString());

                filteredQueryBuilder.Append(" \"query\" :");
                filteredQueryBuilder.Append(functionScore.ToString());
                filteredQueryBuilder.Append("}");
            }

            fullQuery = filteredQueryBuilder.ToString();

            return fullQuery;
        }

        public virtual QueryContainer BuildSearchQuery(bool ignoreDeviceRuleId = false, bool shouldAddIsActive = true, bool shouldAddMissingToGroupByAgg = false)
        {
            return null;
        }

        private ESFunctionScore BuildFunctionScore(string filteredQuery)
        {
            var functions = new List<ESFunctionScoreFunction>();

            foreach (var item in this.SearchDefinitions.boostScoreValues)
            {
                var key = GetElasticsearchFieldName(true, this.SearchDefinitions.langauge, item.Key, item.Type);
                var term = new ESTerm(false)
                {
                    Key = key,
                    Value = item.Value
                };

                functions.Add(new ESFunctionScoreFunction(term)
                {
                    weight = 100
                });
            }

            return new ESFunctionScore()
            {
                query = filteredQuery,
                functions = functions,
                score_mode = eFunctionScoreScoreMode.sum,
                boost_mode = eFunctionScoreBoostMode.replace
            };
        }

        public void BuildInnerFilterAndQuery(out BaseFilterCompositeType filterPart, out IESTerm queryTerm,
            bool ignoreDeviceRuleID, bool isActiveOnly = true, bool shouldMinimizeQuery = false)
        {
            // Eventual filter will be:
            //
            // AND: [
            //          global,
            //          unified = OR
            //              [
            //                  epg, 
            //                  media,
            //                  recording
            //              ]
            //      ]

            FilterCompositeType mediaFilter = new FilterCompositeType(CutWith.AND);
            mediaFilter.AddChild(mediaPrefixTerm);

            FilterCompositeType epgFilter = new FilterCompositeType(CutWith.AND);
            epgFilter.AddChild(epgPrefixTerm);

            FilterCompositeType recordingFilter = new FilterCompositeType(CutWith.AND);
            recordingFilter.AddChild(recordingPrefixTerm);

            #region Initialize unified filter for the three types

            // Or between media and epg and recording
            FilterCompositeType unifiedFilter = new FilterCompositeType(CutWith.OR);

            if (this.SearchDefinitions.shouldSearchEpg)
            {
                unifiedFilter.AddChild(epgFilter);
            }

            if (this.SearchDefinitions.shouldSearchMedia)
            {
                unifiedFilter.AddChild(mediaFilter);
            }

            if (this.SearchDefinitions.shouldSearchRecordings)
            {
                unifiedFilter.AddChild(recordingFilter);
            }

            #endregion

            // Filters which are relevant to all three types
            FilterCompositeType globalFilter = new FilterCompositeType(CutWith.AND);

            ESTerm groupTerm = new ESTerm(true)
            {
                Key = "group_id",
                Value = this.SearchDefinitions.groupId.ToString()
            };

            if (isActiveOnly && !shouldMinimizeQuery)
            {
                ESTerm isActiveTerm = new ESTerm(true)
                {
                    Key = "is_active",
                    Value = "1"
                };

                globalFilter.AddChild(isActiveTerm);
            }

            if (this.SearchDefinitions.exactGroupId != 0)
            {
                ESTerm exectGroupTerm = new ESTerm(true)
                {
                    Key = "group_id",
                    Value = this.SearchDefinitions.exactGroupId.ToString()
                };
                globalFilter.AddChild(exectGroupTerm);
            }

            #region Specific assets - included and excluded

            // If specific assets should return, filter their IDs.
            // Add an IN Clause (Terms) to the matching filter (media, EPG etc.)
            if (this.SearchDefinitions.specificAssets != null)
            {
                foreach (var item in this.SearchDefinitions.specificAssets)
                {
                    // only if it has more than one value
                    if (item.Value != null && item.Value.Count > 0)
                    {
                        ESTerms idsTerm = new ESTerms(true)
                        {
                            Key = "_id"
                        };
                        

                        idsTerm.Value.AddRange(item.Value);

                        switch (item.Key)
                        {
                            case ApiObjects.eAssetTypes.UNKNOWN:
                                break;
                            case ApiObjects.eAssetTypes.EPG:
                                {
                                    epgFilter.AddChild(idsTerm);
                                    break;
                                }
                            case ApiObjects.eAssetTypes.NPVR:
                                {
                                    recordingFilter.AddChild(idsTerm);
                                    break;
                                }
                            case ApiObjects.eAssetTypes.MEDIA:
                                {
                                    mediaFilter.AddChild(idsTerm);
                                    break;
                                }
                            default:
                                break;
                        }
                    }
                }
            }

            // If specific assets should return, filter their IDs.
            // Add an IN Clause (Terms) to the matching filter (media, EPG etc.)
            if (this.SearchDefinitions.excludedAssets != null)
            {
                foreach (var item in this.SearchDefinitions.excludedAssets)
                {
                    ESTerms idsTerm = new ESTerms(true)
                    {
                        Key = "_id",
                        isNot = true
                    };

                    idsTerm.Value.AddRange(item.Value);

                    switch (item.Key)
                    {
                        case ApiObjects.eAssetTypes.UNKNOWN:
                            break;
                        case ApiObjects.eAssetTypes.EPG:
                            {
                                epgFilter.AddChild(idsTerm);
                                break;
                            }
                        case ApiObjects.eAssetTypes.NPVR:
                            {
                                recordingFilter.AddChild(idsTerm);
                                break;
                            }
                        case ApiObjects.eAssetTypes.MEDIA:
                            {
                                mediaFilter.AddChild(idsTerm);
                                break;
                            }
                        default:
                            break;
                    }
                }
            }

            #endregion

            // Dates filter: 
            // If it is media, it should start before now and end after now
            // If it is EPG, it should start and end around the current week

            // epg ranges
            if (this.SearchDefinitions.shouldSearchEpg)
            {
                #region Epg Dates ranges

                FilterCompositeType epgDatesFilter = new FilterCompositeType(CutWith.AND);

                // Now +- days offset
                string nowPlusOffsetDateString = SystemDateTime.UtcNow.AddDays(this.SearchDefinitions.epgDaysOffest).ToString("yyyyMMddHHmmss");
                string nowMinusOffsetDateString = SystemDateTime.UtcNow.AddDays(-this.SearchDefinitions.epgDaysOffest).ToString("yyyyMMddHHmmss");

                if (this.SearchDefinitions.shouldUseStartDateForEpg)
                {
                    ESRange epgStartDateRange = new ESRange(false)
                    {
                        Key = "start_date"
                    };

                    epgStartDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, nowMinusOffsetDateString));
                    epgStartDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, nowPlusOffsetDateString));

                    epgDatesFilter.AddChild(epgStartDateRange);
                }

                if (this.SearchDefinitions.shouldUseEndDateForEpg)
                {
                    ESRange epgEndDateRange = new ESRange(false)
                    {
                        Key = "end_date"
                    };

                    epgEndDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, nowMinusOffsetDateString));
                    epgEndDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, nowPlusOffsetDateString));

                    epgDatesFilter.AddChild(epgEndDateRange);
                }

                if (SearchDefinitions.shouldUseSearchEndDate)
                {
                    // by search_end_date - for buffer issues - MUST BE LT (nor Equal)          
                    ESRange epgSearchEndDateRange = new ESRange(false)
                    {
                        Key = "search_end_date"
                    };
                    epgSearchEndDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GT, SystemDateTime.UtcNow.ToString("yyyyMMddHHmmss")));
                    epgDatesFilter.AddChild(epgSearchEndDateRange);
                }

                if (!epgDatesFilter.IsEmpty())
                {
                    epgFilter.AddChild(epgDatesFilter);
                }

                #endregion

                #region Parental Rules

                if (this.SearchDefinitions.epgParentalRulesTags.Count > 0)
                {
                    FilterCompositeType epgParentalRulesTagsComposite = new FilterCompositeType(CutWith.AND);

                    // Run on all tags and their values
                    foreach (KeyValuePair<string, List<string>> tagValues in this.SearchDefinitions.epgParentalRulesTags)
                    {
                        // Create a Not-in terms for each of the tags
                        ESTerms currentTag = new ESTerms(false);

                        currentTag.isNot = true;
                        currentTag.Key = string.Concat("tags.", tagValues.Key.ToLower());

                        foreach (var value in tagValues.Value)
                        {
                            currentTag.Value.Add(value.ToLower());
                        }

                        // Connect each terms with "AND"
                        epgParentalRulesTagsComposite.AddChild(currentTag);
                    }

                    if (!epgParentalRulesTagsComposite.IsEmpty())
                    {
                        epgFilter.AddChild(epgParentalRulesTagsComposite);
                    }
                }

                #endregion

                #region Regions

                // region term 
                if (SearchDefinitions.regionIds != null && SearchDefinitions.regionIds.Count > 0)
                {
                    ESTerms regionsTerms = new ESTerms(true)
                    {
                        Key = "regions"
                    };

                    regionsTerms.Value.AddRange(SearchDefinitions.regionIds.Select(region => region.ToString()));

                    epgFilter.AddChild(regionsTerms);
                }

                #endregion

                if (!SearchDefinitions.ShouldSearchAutoFill)
                {
                    ESTerm autofill = new ESTerm(true)
                    {
                        Key = "is_auto_fill",
                        Value = "1",
                        isNot = true
                    };

                    epgFilter.AddChild(autofill);
                }
            }

            // Media specific filters - user types, media types etc.
            if (this.SearchDefinitions.shouldSearchMedia)
            {
                #region User Types

                if (!shouldMinimizeQuery)
                {
                    ESTerms userTypeTerm = new ESTerms(true);
                    userTypeTerm.Key = "user_types";
                    userTypeTerm.Value.Add("0");

                    if (this.SearchDefinitions.userTypeID > 0)
                    {
                        userTypeTerm.Value.Add(this.SearchDefinitions.userTypeID.ToString());
                    }

                    mediaFilter.AddChild(userTypeTerm);
                }

                #endregion

                #region Media Types

                ESTerms mediaTypesTerms = new ESTerms(true);

                mediaTypesTerms.Key = "media_type_id";

                foreach (int mediaType in this.SearchDefinitions.mediaTypes)
                {
                    mediaTypesTerms.Value.Add(mediaType.ToString());
                }

                mediaFilter.AddChild(mediaTypesTerms);

                #endregion

                #region Watch Permissions rules

                // group_id = parent groupd id OR media has permitted watch filter 
                FilterCompositeType groupWPComposite = new FilterCompositeType(CutWith.OR);

                ESTerms permittedWatchFilter = new ESTerms(true);

                if (!string.IsNullOrEmpty(this.SearchDefinitions.permittedWatchRules))
                {
                    permittedWatchFilter.Key = "wp_type_id";
                    List<string> permittedValues = permittedWatchFilter.Value;
                    foreach (string value in this.SearchDefinitions.permittedWatchRules.Split(' '))
                    {
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            permittedValues.Add(value);
                        }
                    }
                }

                groupWPComposite.AddChild(groupTerm);
                groupWPComposite.AddChild(permittedWatchFilter);

                if (!shouldMinimizeQuery)
                {
                    mediaFilter.AddChild(groupWPComposite);
                }

                #endregion

                #region Device types
                
                if (!ignoreDeviceRuleID && !shouldMinimizeQuery)
                {
                    ESTerms deviceRulesTerms = new ESTerms(true)
                    {
                        Key = "device_rule_id"
                    };

                    deviceRulesTerms.Value.Add("0");

                    if (this.SearchDefinitions.deviceRuleId != null &&
                        this.SearchDefinitions.deviceRuleId.Length > 0)
                    {

                        foreach (int deviceRuleId in this.SearchDefinitions.deviceRuleId)
                        {
                            deviceRulesTerms.Value.Add(deviceRuleId.ToString());
                        }
                    }


                    mediaFilter.AddChild(deviceRulesTerms);
                }
                #endregion

                #region Media Dates ranges

                FilterCompositeType mediaDatesFilter = BuildMediaDatesComposite();

                // Geo availability enabled
                if (SearchDefinitions.countryId > 0)
                {
                    FilterCompositeType allowedEmpty = new FilterCompositeType(CutWith.AND);

                    ESTerm emptyAllowedCountryTerm = new ESTerm(true)
                    {
                        Key = "allowed_countries",
                        Value = "0"
                    };

                    // allowed_countries = 0 and dates filter
                    allowedEmpty.AddChild(emptyAllowedCountryTerm);
                    allowedEmpty.AddChild(mediaDatesFilter);

                    FilterCompositeType allowed = new FilterCompositeType(CutWith.OR);

                    ESTerm allowedCountryTerm = new ESTerm(true)
                    {
                        Key = "allowed_countries",
                        Value = SearchDefinitions.countryId.ToString()
                    };

                    // allowed_countries = countryId or allowedEmpty
                    allowed.AddChild(allowedCountryTerm);
                    allowed.AddChild(allowedEmpty);

                    FilterCompositeType blocked = new FilterCompositeType(CutWith.AND);

                    ESTerm blockedCountryTerm = new ESTerm(true)
                    {
                        Key = "blocked_countries",
                        Value = SearchDefinitions.countryId.ToString(),
                        isNot = true
                    };

                    // blocked_countries != countryId and allowed
                    blocked.AddChild(blockedCountryTerm);
                    blocked.AddChild(allowed);

                    mediaFilter.AddChild(blocked);
                }
                else if (!mediaDatesFilter.IsEmpty()) // No geo availability - handle dates without country consideration 
                {
                    mediaFilter.AddChild(mediaDatesFilter);
                }

                #endregion

                #region Regions

                // region term 
                if (!SearchDefinitions.isAllowedToViewInactiveAssets && SearchDefinitions.regionIds != null && SearchDefinitions.regionIds.Count > 0)
                {
                    FilterCompositeType regionComposite = new FilterCompositeType(CutWith.OR);
                    FilterCompositeType emptyRegionAndComposite = new FilterCompositeType(CutWith.AND);

                    ESTerms regionsTerms = new ESTerms(true)
                    {
                        Key = "regions"
                    };

                    regionsTerms.Value.AddRange(SearchDefinitions.regionIds.Select(region => region.ToString()));

                    ESTerm emptyRegionTerm = new ESTerm(true)
                    {
                        Key = "regions",
                        Value = "0"
                    };

                    ESTerms linearMediaTypes = new ESTerms(true)
                    {
                        Key = "media_type_id",
                        isNot = true
                    };

                    linearMediaTypes.Value.AddRange(SearchDefinitions.linearChannelMediaTypes);

                    // region = 0 and it is NOT linear media
                    emptyRegionAndComposite.AddChild(emptyRegionTerm);
                    emptyRegionAndComposite.AddChild(linearMediaTypes);

                    // It is either in the desired region or it is in region 0 and not linear media
                    regionComposite.AddChild(regionsTerms);
                    regionComposite.AddChild(emptyRegionAndComposite);

                    mediaFilter.AddChild(regionComposite);
                }

                #endregion

                #region Geo Block Rules

                // region term 
                if (SearchDefinitions.geoBlockRules != null && SearchDefinitions.geoBlockRules.Count > 0)
                {
                    ESTerms geoBlockTerms = new ESTerms(true)
                    {
                        Key = "geo_block_rule_id"
                    };

                    geoBlockTerms.Value.AddRange(SearchDefinitions.geoBlockRules.Select(rule => rule.ToString()));

                    mediaFilter.AddChild(geoBlockTerms);
                }

                #endregion

                #region Parental Rules

                if (this.SearchDefinitions.mediaParentalRulesTags.Count > 0)
                {
                    FilterCompositeType mediaParentalRulesTagsComposite = new FilterCompositeType(CutWith.AND);

                    // Run on all tags and their values
                    foreach (KeyValuePair<string, List<string>> tagValues in this.SearchDefinitions.mediaParentalRulesTags)
                    {
                        // Create a Not-in terms for each of the tags
                        ESTerms currentTag = new ESTerms(false);

                        currentTag.isNot = true;
                        currentTag.Key = string.Concat("tags.", tagValues.Key.ToLower());

                        foreach (var value in tagValues.Value)
                        {
                            currentTag.Value.Add(value.ToLower());
                        }


                        // Connect each terms with "AND"
                        mediaParentalRulesTagsComposite.AddChild(currentTag);
                    }

                    if (!mediaParentalRulesTagsComposite.IsEmpty())
                    {
                        mediaFilter.AddChild(mediaParentalRulesTagsComposite);
                    }
                }

                #endregion

                #region ObjectVirtualAsset

                // region ObjectVirtualAsset 
                if ((SearchDefinitions.ksqlAssetTypes?.Count == 0 || SearchDefinitions.ksqlAssetTypes.Contains("media"))
                    && SearchDefinitions.mediaTypes?.Count == 0
                    && SearchDefinitions.ObjectVirtualAssetIds?.Count > 0)
                {
                    ESTerms objectVirtualAssetIds = new ESTerms(true)
                    {
                        Key = "media_type_id",
                        isNot = true
                    };

                    objectVirtualAssetIds.Value.AddRange(SearchDefinitions.ObjectVirtualAssetIds.Select(x => x.ToString()));

                    mediaFilter.AddChild(objectVirtualAssetIds);
                }

                #endregion

            }

            
            #region Excluded CRIDs

            if (this.SearchDefinitions.shouldSearchRecordings || this.SearchDefinitions.shouldSearchEpg)
            {

                if (this.SearchDefinitions.excludedCrids != null && this.SearchDefinitions.excludedCrids.Count > 0)
                {
                    ESTerms idsTerm = new ESTerms(false)
                    {
                        Key = "crid",
                        isNot = true,
                    };

                    idsTerm.Value.AddRange(this.SearchDefinitions.excludedCrids.ConvertAll(x => x.ToLower()));
                    recordingFilter.AddChild(idsTerm);
                    epgFilter.AddChild(idsTerm);
                }
            }

            #endregion

            #region Phrase Tree

            queryTerm = null;

            if (this.SearchDefinitions.filterPhrase != null)
            {
                BooleanPhraseNode queryNode = null;
                BooleanPhraseNode filterNode = null;
                BooleanPhraseNode root = this.SearchDefinitions.filterPhrase;

                // Easiest case - only one node
                if (root.type == BooleanNodeType.Leaf)
                {
                    var leaf = root as BooleanLeaf;
                    // If it is contains - it is not exact and thus belongs to query
                    if (leaf.operand == ApiObjects.ComparisonOperator.Contains || leaf.operand == ApiObjects.ComparisonOperator.NotContains ||
                        leaf.operand == ApiObjects.ComparisonOperator.WordStartsWith || leaf.operand == ApiObjects.ComparisonOperator.Phonetic ||
                        leaf.operand == ApiObjects.ComparisonOperator.Exists || leaf.operand == ApiObjects.ComparisonOperator.NotExists ||
                        leaf.operand == ApiObjects.ComparisonOperator.PhraseStartsWith ||
                        leaf.shouldLowercase)
                    {
                        queryNode = leaf;
                    }
                    // Otherwise it is an exact search and belongs to a filter
                    else
                    {
                        filterNode = leaf;
                    }
                }
                // If it is a phrase, we must understand which of its child nodes belongs to the query part or to the filter part
                // We do that with a DFS check of the leafs - at least one leaf that is not-exact means it is query. Otherwise it's filter
                else if (root.type == BooleanNodeType.Parent)
                {
                    var phraseRoot = root as BooleanPhrase;

                    List<BooleanPhraseNode> filterRoots = new List<BooleanPhraseNode>();
                    List<BooleanPhraseNode> queryRoots = new List<BooleanPhraseNode>();

                    // Check which of the phrase first level nodes are designed to be queries or filter.
                    // A node will be a query if one of its descendents is a not-exact search (Contains)
                    foreach (var node in phraseRoot.nodes)
                    {
                        Stack<BooleanPhraseNode> stack = new Stack<BooleanPhraseNode>();
                        stack.Push(node);
                        bool isCurrentDone = false;
                        // Go DFS with stack until we find one "contains" leaf or until no more nodes are left
                        while (stack.Count > 0 && !isCurrentDone)
                        {
                            BooleanPhraseNode current = stack.Pop();
                            // If it is a leaf, check if it is a not-exact leaf or not
                            if (current.type == BooleanNodeType.Leaf)
                            {
                                // If yes, this means the root-ancestor is a query and not a filter
                                if ((current as BooleanLeaf).operand == ApiObjects.ComparisonOperator.Contains ||
                                    (current as BooleanLeaf).operand == ApiObjects.ComparisonOperator.NotContains ||
                                    (current as BooleanLeaf).operand == ApiObjects.ComparisonOperator.WordStartsWith ||
                                    (current as BooleanLeaf).operand == ApiObjects.ComparisonOperator.PhraseStartsWith ||
                                    (current as BooleanLeaf).operand == ApiObjects.ComparisonOperator.Phonetic ||
                                    (current as BooleanLeaf).operand == ApiObjects.ComparisonOperator.Exists ||
                                    (current as BooleanLeaf).operand == ApiObjects.ComparisonOperator.NotExists ||
                                    (current as BooleanLeaf).shouldLowercase)
                                {
                                    queryRoots.Add(node);

                                    isCurrentDone = true;
                                }
                            }
                            else if (current.type == BooleanNodeType.Parent)
                            {
                                // If it is a boolean phrase, push all children to stack
                                (current as BooleanPhrase).nodes.ForEach(child =>
                                    stack.Push(child));
                            }
                        }

                        // If we didn't find any descendent that is a not exact search,
                        // everything is exact and thus it is a filter
                        if (!isCurrentDone)
                        {
                            filterRoots.Add(node);
                        }
                    }

                    // If this is an OR operation and we have at least one query root - all of them shall be in query, none in filter
                    if ((phraseRoot.operand == ApiObjects.eCutType.Or) &&
                        (queryRoots.Count > 0))
                    {
                        queryRoots.AddRange(filterRoots);
                        filterRoots.Clear();
                    }

                    if (queryRoots.Count > 0)
                    {
                        queryNode = new BooleanPhrase(queryRoots, phraseRoot.operand);
                    }

                    if (filterRoots.Count > 0)
                    {
                        filterNode = new BooleanPhrase(filterRoots, phraseRoot.operand);
                    }
                }

                if (queryNode != null)
                {
                    queryTerm = ConvertToQuery(queryNode);
                }

                if (filterNode != null)
                {
                    BaseFilterCompositeType filterComposite = ConvertToFilter(filterNode);
                    globalFilter.AddChild(filterComposite);
                }
            }


            #endregion

            #region Asset User Rule

            if (SearchDefinitions.assetUserBlockRulePhrase != null)
            {
                IESTerm notPhraseQuery = this.ConvertToQuery(SearchDefinitions.assetUserBlockRulePhrase);

                if (queryTerm == null)
                {
                    queryTerm = new ElasticSearch.Searcher.BoolQuery();
                    (queryTerm as ElasticSearch.Searcher.BoolQuery).AddNot(notPhraseQuery);
                }
                else
                {
                    ElasticSearch.Searcher.BoolQuery boolQuery = queryTerm as ElasticSearch.Searcher.BoolQuery;

                    if (boolQuery != null)
                    {
                        boolQuery.AddNot(notPhraseQuery);
                    }
                    else
                    {
                        boolQuery = new ElasticSearch.Searcher.BoolQuery();
                        boolQuery.AddChild(queryTerm, CutWith.AND);
                        boolQuery.AddNot(notPhraseQuery);

                        queryTerm = boolQuery;
                    }
                }
            }

            if (SearchDefinitions.assetUserRuleFilterPhrase != null)
            {
                IESTerm phraseQuery = this.ConvertToQuery(SearchDefinitions.assetUserRuleFilterPhrase);

                if (queryTerm == null)
                {
                    queryTerm = new ElasticSearch.Searcher.BoolQuery();
                    (queryTerm as ElasticSearch.Searcher.BoolQuery).AddChild(phraseQuery, CutWith.AND);
                }
                else
                {
                    ElasticSearch.Searcher.BoolQuery boolQuery = queryTerm as ElasticSearch.Searcher.BoolQuery;

                    if (boolQuery != null)
                    {
                        boolQuery.AddChild(phraseQuery, CutWith.AND);
                    }
                    else
                    {
                        boolQuery = new ElasticSearch.Searcher.BoolQuery();
                        boolQuery.AddChild(queryTerm, CutWith.AND);
                        boolQuery.AddChild(phraseQuery, CutWith.AND);

                        queryTerm = boolQuery;
                    }
                }
            }

            #endregion
            // Eventual filter will be:
            //
            // AND: [
            //          global,
            //          unified = OR
            //              [
            //                  epg, 
            //                  media,
            //                  recording
            //              ]
            //      ]
            BaseFilterCompositeType filterParent = new FilterCompositeType(CutWith.AND);

            filterParent.AddChild(unifiedFilter);
            filterParent.AddChild(globalFilter);

            filterPart = filterParent;
        }

        /// <summary>
        /// Build the request body for Elasticsearch getting update dates string.
        /// Basically it filters like this:
        /// (media id IN (...) OR epg id IN (...))
        /// </summary>
        /// <returns></returns>
        public static string BuildGetUpdateDatesString(List<KeyValuePair<ApiObjects.eAssetTypes, string>> assets, bool shouldIgnoreRecordings = false)
        {
            if (assets == null)
            {
                return string.Empty;
            }

            string fullQuery = string.Empty;

            StringBuilder filteredQueryBuilder = new StringBuilder();

            filteredQueryBuilder.Append("{");
            filteredQueryBuilder.AppendFormat(" \"size\": {0}, ", MAX_RESULTS);
            filteredQueryBuilder.AppendFormat(" \"from\": {0}, ", 0);

            // Return fields - id and update date only
            filteredQueryBuilder.Append("\"fields\": [\"_id\", \"update_date\"], ");

            // Queried filter
            filteredQueryBuilder.Append(" \"query\": { \"filtered\": {");

            bool shouldSearchRecordings = false;
            bool shouldSearchEpg = false;
            bool shouldSearchMedia = false;

            QueryFilter filterPart = new QueryFilter();
            BaseFilterCompositeType filterParent = new FilterCompositeType(CutWith.AND);

            ESPrefix epgPrefixTerm = new ESPrefix()
            {
                Key = "_type",
                Value = "epg"
            };

            ESPrefix mediaPrefixTerm = new ESPrefix()
            {
                Key = "_type",
                Value = "media"
            };

            ESPrefix recordingsPrefixTerm = new ESPrefix()
            {
                Key = "_type",
                Value = "recording"
            };

            FilterCompositeType mediaFilter = new FilterCompositeType(CutWith.AND);
            mediaFilter.AddChild(mediaPrefixTerm);

            FilterCompositeType epgFilter = new FilterCompositeType(CutWith.AND);
            epgFilter.AddChild(epgPrefixTerm);

            FilterCompositeType recordingsFilter = new FilterCompositeType(CutWith.AND);
            recordingsFilter.AddChild(recordingsPrefixTerm);

            ESTerms mediaIdsTerm = new ESTerms(true)
            {
                Key = "_id"
            };

            ESTerms epgIdsTerm = new ESTerms(true)
            {
                Key = "_id"
            };

            ESTerms recordingIdsTerm = new ESTerms(true)
            {
                Key = "_id"
            };

            // Add Ids to relevant Terms part
            foreach (var item in assets)
            {
                switch (item.Key)
                {
                    case ApiObjects.eAssetTypes.UNKNOWN:
                        break;
                    case ApiObjects.eAssetTypes.EPG:
                        {
                            epgIdsTerm.Value.Add(item.Value);
                            shouldSearchEpg = true;
                            break;
                        }
                    case ApiObjects.eAssetTypes.NPVR:
                        {
                            if (!shouldIgnoreRecordings)
                            {
                                recordingIdsTerm.Value.Add(item.Value);
                                shouldSearchRecordings = true;
                            }
                            break;
                        }
                    case ApiObjects.eAssetTypes.MEDIA:
                        {
                            mediaIdsTerm.Value.Add(item.Value);
                            shouldSearchMedia = true;
                            break;
                        }
                    default:
                        break;
                }
            }

            // Or between media and epg
            FilterCompositeType unifiedFilter = new FilterCompositeType(CutWith.OR);

            if (shouldSearchEpg)
            {
                epgFilter.AddChild(epgIdsTerm);
                unifiedFilter.AddChild(epgFilter);
            }

            if (shouldSearchMedia)
            {
                mediaFilter.AddChild(mediaIdsTerm);
                unifiedFilter.AddChild(mediaFilter);
            }

            if (shouldSearchRecordings)
            {
                recordingsFilter.AddChild(recordingIdsTerm);
                unifiedFilter.AddChild(recordingsFilter);
            }

            filterParent.AddChild(unifiedFilter);

            filterPart.FilterSettings = filterParent;
            filteredQueryBuilder.Append(filterPart.ToString());
            filteredQueryBuilder.Append(" } } }");

            fullQuery = filteredQueryBuilder.ToString();

            return fullQuery;
        }

        /// <summary>
        /// Builds a partial string of indexes for the URL of the ES request
        /// </summary>
        /// <param name="queryType"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public static string GetIndexes(UnifiedSearchDefinitions definitions, int groupId)
        {
            string indexes = string.Empty;
            StringBuilder builder = new StringBuilder();

            if (definitions.shouldSearchMedia)
            {
                builder.Append(groupId);
                builder.Append(',');
            }

            if (definitions.shouldSearchEpg)
            {
                builder.AppendFormat("{0}_epg,", groupId);
            }

            if (definitions.shouldSearchRecordings)
            {
                builder.AppendFormat("{0}_recording,", groupId);
            }

            // remove last ,
            if (builder.Length > 0)
            {
                builder.Remove(builder.Length - 1, 1);
            }

            indexes = builder.ToString();

            return indexes;
        }

        /// <summary>
        /// Builds a partial string of indexes for the URL of the ES request
        /// </summary>
        /// <param name="queryType"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public static string GetIndexes(List<UnifiedSearchDefinitions> definitions, int groupId)
        {
            string indexes = string.Empty;

            bool shouldSearchEpg = false;
            bool shouldSearchMedia = true;
            bool shouldSearchRecordings = false;

            foreach (var definition in definitions)
            {
                if (definition.shouldSearchEpg)
                {
                    shouldSearchEpg = true;
                }

                if (definition.shouldSearchMedia)
                {
                    shouldSearchMedia = true;
                }

                if (definition.shouldSearchRecordings)
                {
                    shouldSearchRecordings = true;
                }
            }

            StringBuilder builder = new StringBuilder();

            if (shouldSearchMedia)
            {
                builder.Append(groupId);
                builder.Append(',');
            }

            if (shouldSearchEpg)
            {
                builder.AppendFormat("{0}_epg,", groupId);
            }

            if (shouldSearchRecordings)
            {
                builder.AppendFormat("{0}_recording,", groupId);
            }

            // remove last ,
            if (builder.Length > 0)
            {
                builder.Remove(builder.Length - 1, 1);
            }

            indexes = builder.ToString();

            return indexes;
        }

        public static string GetTypes(UnifiedSearchDefinitions definitions)
        {
            string types = string.Empty;

            StringBuilder builder = new StringBuilder();

            string media = "media";
            string epg = "epg";
            string recording = "recording";

            // If language isn't default
            if (definitions.langauge != null &&
                !definitions.langauge.IsDefault)
            {
                if (definitions.shouldSearchMedia)
                {
                    builder.AppendFormat("{0}_{1},", media, definitions.langauge.Code);
                }

                if (definitions.shouldSearchEpg)
                {
                    builder.AppendFormat("{0}_{1},", epg, definitions.langauge.Code);
                }

                if (definitions.shouldSearchRecordings)
                {
                    builder.AppendFormat("{0}_{1},", recording, definitions.langauge.Code);
                }

                // remove last ,
                if (builder.Length > 0)
                {
                    builder.Remove(builder.Length - 1, 1);
                }
            }
            else
            {
                if (definitions.shouldSearchMedia)
                {
                    builder.AppendFormat("{0},", media);
                }

                if (definitions.shouldSearchEpg)
                {
                    builder.AppendFormat("{0},", epg);
                }

                if (definitions.shouldSearchRecordings)
                {
                    builder.AppendFormat("{0},", recording);
                }

                // remove last ,
                if (builder.Length > 0)
                {
                    builder.Remove(builder.Length - 1, 1);
                }
            }

            types = builder.ToString();

            return types;
        }

        public static string GetTypes(List<UnifiedSearchDefinitions> definitions)
        {
            string media = "media";
            string epg = "epg";
            string recording = "recording";

            HashSet<string> typesList = new HashSet<string>();
            StringBuilder finalBuilder = new StringBuilder();

            foreach (var definition in definitions)
            {
                // If language isn't default
                if (definition.langauge != null &&
                    !definition.langauge.IsDefault)
                {
                    if (definition.shouldSearchMedia)
                    {
                        typesList.Add(string.Format("{0}_{1}", media, definition.langauge.Code));
                    }

                    if (definition.shouldSearchEpg)
                    {
                        typesList.Add(string.Format("{0}_{1}", epg, definition.langauge.Code));
                    }

                    if (definition.shouldSearchRecordings)
                    {
                        typesList.Add(string.Format("{0}_{1}", recording, definition.langauge.Code));
                    }
                }
                else
                {
                    if (definition.shouldSearchMedia)
                    {
                        typesList.Add(media);
                    }

                    if (definition.shouldSearchEpg)
                    {
                        typesList.Add(epg);
                    }

                    if (definition.shouldSearchRecordings)
                    {
                        typesList.Add(recording);
                    }
                }
            }

            foreach (var type in typesList)
            {
                finalBuilder.Append(type);
                finalBuilder.Append(',');
            }

            // Remove last ','
            if (finalBuilder.Length > 0)
            {
                finalBuilder.Remove(finalBuilder.Length - 1, 1);
            }

            return finalBuilder.ToString();
        }

        internal static string GetElasticsearchFieldName(bool isLanguageSpecific, ApiObjects.LanguageObj language, string key, eFieldType type)
        {
            if (isLanguageSpecific && language != null && !language.IsDefault)
            {
                key = $"{key}_{language.Code}";
            }

            string value = null;
            switch (type)
            {
                case eFieldType.Default:
                case eFieldType.LanguageSpecificField:
                    value = key;
                    break;
                case eFieldType.StringMeta:
                    value = $"metas.{key}";
                    break;
                case eFieldType.NonStringMeta:
                    value = $"metas.{key}";
                    break;
                case eFieldType.Tag:
                    value = $"tags.{key}";
                    break;
                default:
                    break;
            }

            return value;
        }

        #endregion

        #region Protected and Private Methods

        /// <summary>
        /// Recursively convert a phrase tree into a filter composite
        /// </summary>
        /// <param name="filterNode"></param>
        /// <returns></returns>
        protected BaseFilterCompositeType ConvertToFilter(BooleanPhraseNode filterNode)
        {
            FilterCompositeType composite = null;

            // Leaf is stop condition: Convert it to a term and send it back
            if (filterNode.type == BooleanNodeType.Leaf)
            {
                composite = new FilterCompositeType(CutWith.AND);

                BooleanLeaf leaf = filterNode as BooleanLeaf;
                IESTerm leafTerm = ConvertToFilter(leaf);

                if (leaf.field == NamingHelper.AUTO_FILL_FIELD)
                {
                    return composite;
                }

                // If this leaf is relevant only to certain asset types - create a bool query connecting the types and the term
                if (leaf.assetTypes != null && leaf.assetTypes.Count > 0)
                {
                    FilterCompositeType newComposite = new FilterCompositeType(CutWith.OR);
                    FilterCompositeType subComposite = new FilterCompositeType(CutWith.AND);
                    FilterCompositeType typesComposite = new FilterCompositeType(CutWith.OR);
                    FilterCompositeType typesCompositeNot = new FilterCompositeType(CutWith.AND);

                    // Create a prefix term for each asset type
                    foreach (var assetType in leaf.assetTypes)
                    {
                        ESPrefix prefix = new ESPrefix()
                        {
                            Key = "_type",
                            Value = assetType.ToString().ToLower()
                        };
                        ESPrefix prefixNot = new ESPrefix()
                        {
                            Key = "_type",
                            Value = assetType.ToString().ToLower(),
                            isNot = true
                        };

                        // the prefixes are SHOULD (or) [at least one of the following....)
                        typesComposite.AddChild(prefix);

                        typesCompositeNot.AddChild(prefixNot);
                    }

                    subComposite.AddChild(typesComposite);
                    subComposite.AddChild(leafTerm);

                    newComposite.AddChild(subComposite);
                    newComposite.AddChild(typesCompositeNot);

                    composite.AddChild(newComposite);
                }
                else
                {
                    composite.AddChild(leafTerm);
                }
            }
            else if (filterNode.type == BooleanNodeType.Parent)
            {
                CutWith cut = CutWith.AND;

                // Simply conversion of enums: I could use casting but I don't want to trust it. 
                if ((filterNode as BooleanPhrase).operand == ApiObjects.eCutType.Or)
                {
                    cut = CutWith.OR;
                }

                composite = new FilterCompositeType(cut);

                // Add every child node to the filter composite. This is recursive!
                foreach (var childNode in (filterNode as BooleanPhrase).nodes)
                {
                    composite.AddChild(ConvertToFilter(childNode));
                }
            }

            return (composite);
        }

        /// <summary>
        /// Recursively converts a phrase tree into a Boolean query or ESTerm
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public IESTerm ConvertToQuery(BooleanPhraseNode root)
        {
            IESTerm term = null;

            // If it is a leaf, this is the stop condition: Simply convert to ESTerm
            if (root.type == BooleanNodeType.Leaf)
            {
                BooleanLeaf leaf = root as BooleanLeaf;
                if (leaf == null)
                {
                    return term;
                }
                
                var leafField = GetElasticsearchFieldName(leaf.isLanguageSpecific, this.SearchDefinitions.langauge, leaf.field, leaf.fieldType);

                // Special case - if this is the entitled assets leaf, we build a specific term for it
                if (leafField == NamingHelper.ENTITLED_ASSETS_FIELD)
                {
                    term = BuildEntitledAssetsQuery();
                }
                else if (leafField == NamingHelper.USER_INTERESTS_FIELD)
                {
                    term = BuildUserInterestsQuery();
                }
                else if (leafField == NamingHelper.ASSET_TYPE)
                {
                    term = BuildAssetTypeQuery(leaf);
                }
                else if (leafField == NamingHelper.RECORDING_ID)
                {
                    term = BuildRecordingIdTerm(leaf);
                }
                else
                {
                    bool isNumeric = leaf.valueType == typeof(int) || leaf.valueType == typeof(long);

                    string value = string.Empty;

                    // First find out the value to use in the filter body 
                    if (isNumeric)
                    {
                        value = leaf.value.ToString();
                    }
                    else if (leaf.valueType == typeof(DateTime))
                    {
                        DateTime date = Convert.ToDateTime(leaf.value);

                        if (date != null)
                        {
                            value = date.ToString(ES_DATE_FORMAT);
                        }
                    }
                    else if (leaf.value is IEnumerable<string>)
                    {
                        leaf.value = (leaf.value as IEnumerable<string>).Select(item => item.ToLower());
                    }
                    else
                    {
                        value = leaf.value.ToString().ToLower();
                    }

                    // "Match" when search is not exact (contains)
                    if (leaf.operand == ApiObjects.ComparisonOperator.Contains ||
                        leaf.operand == ApiObjects.ComparisonOperator.Equals ||
                        leaf.operand == ApiObjects.ComparisonOperator.WordStartsWith ||
                        leaf.operand == ApiObjects.ComparisonOperator.PhraseStartsWith ||
                        leaf.operand == ApiObjects.ComparisonOperator.Phonetic)
                    {
                        string field = string.Empty;
                        var isFuzzySearch = false;

                        if (leaf.operand == ApiObjects.ComparisonOperator.WordStartsWith)
                        {
                            field = string.Format("{0}.autocomplete", leafField);
                        }
                        else if (leaf.operand == ApiObjects.ComparisonOperator.PhraseStartsWith)
                        {
                            field = string.Format("{0}.phrase_autocomplete", leafField);
                        }
                        else if (leaf.operand == ApiObjects.ComparisonOperator.Contains)
                        {
                            field = string.Format("{0}.analyzed", leafField);
                        }
                        else if (leaf.operand == ApiObjects.ComparisonOperator.Phonetic)
                        {
                            if (leaf.valueType == typeof(string) && !IndexManagerCommonHelpers.IsLanguagePhoneticSupported(leaf.value.ToString()))
                            {
                                isFuzzySearch = true;
                                field = leafField;
                            }
                            else
                            {
                                field = string.Format("{0}.phonetic", leafField);
                            }
                        }
                        else if (leaf.operand == ApiObjects.ComparisonOperator.Equals &&
                            leaf.shouldLowercase)
                        {
                            field = string.Format("{0}.lowercase", leafField);
                        }
                        else
                        {
                            field = leafField;
                        }

                        if (isFuzzySearch)
                        {
                            term = new ESFuzzyQuery(field, value) { eOperator = CutWith.AND };
                        }
                        else
                        {
                            term = new ESMatchQuery(null)
                            {
                                Field = field,
                                eOperator = CutWith.AND,
                                Query = value
                            };
                        }
                    }
                    // "bool" with "must_not" when no contains
                    else if (leaf.operand == ApiObjects.ComparisonOperator.NotContains)
                    {
                        string field = string.Format("{0}.analyzed", leafField);

                        term = new ElasticSearch.Searcher.BoolQuery();

                        (term as ElasticSearch.Searcher.BoolQuery).AddNot(
                            new ESMatchQuery(null)
                            {
                                Field = field,
                                eOperator = CutWith.AND,
                                Query = value
                            });
                    }
                    // "bool" with "must_not" when no equals
                    else if (leaf.operand == ApiObjects.ComparisonOperator.NotEquals && leaf.shouldLowercase)
                    {
                        string field = string.Format("{0}.lowercase", leafField);

                        term = new ElasticSearch.Searcher.BoolQuery();

                        (term as ElasticSearch.Searcher.BoolQuery).AddNot(
                            new ESMatchQuery(null)
                            {
                                Field = field,
                                eOperator = CutWith.AND,
                                Query = value
                            });
                    }
                    // "Term" when search is equals/not equals
                    else if (leaf.operand == ApiObjects.ComparisonOperator.Equals ||
                        leaf.operand == ApiObjects.ComparisonOperator.NotEquals)
                    {
                        term = new ESTerm(isNumeric)
                        {
                            Key = leafField,
                            Value = value
                        };
                    }
                    else if (leaf.operand == ApiObjects.ComparisonOperator.Prefix)
                    {
                        term = new ESPrefix()
                        {
                            Key = leafField,
                            Value = value
                        };
                    }
                    else if (leaf.operand == ApiObjects.ComparisonOperator.In)
                    {
                        term = new ESTerms(false)
                        {
                            Key = leafField
                        };

                        (term as ESTerms).Value.AddRange(leaf.value as IEnumerable<string>);
                    }
                    else if (leaf.operand == ApiObjects.ComparisonOperator.NotIn)
                    {
                        term = new ESTerms(false)
                        {
                            Key = leafField,
                            isNot = true
                        };

                        (term as ESTerms).Value.AddRange(leaf.value as IEnumerable<string>);
                    }
                    else if (leaf.operand == ApiObjects.ComparisonOperator.Exists)
                    {
                        term = new ESExists()
                        {
                            Value = leafField
                        };
                    }
                    else if (leaf.operand == ApiObjects.ComparisonOperator.NotExists)
                    {
                        term = new ESExists()
                        {
                            Value = leafField,
                            isNot = true
                        };
                    }
                    // Other cases are "Range"
                    else
                    {
                        string rangeValue = value;
                        if (this.SearchDefinitions.numericEpgMetas.Contains(leaf.field) && this.SearchDefinitions.shouldSearchEpg)
                        {
                            leafField = $"padded_{leafField}";
                            rangeValue = Media.PadValue(rangeValue);

                        }

                        term = ConvertToRange(leafField, rangeValue, leaf.operand, isNumeric);
                    }
                }

                // If this leaf is relevant only to certain asset types - create a bool query connecting the types and the term
                if (leaf.assetTypes != null && leaf.assetTypes.Count > 0)
                {
                    ElasticSearch.Searcher.BoolQuery fatherBool = new ElasticSearch.Searcher.BoolQuery();
                    ElasticSearch.Searcher.BoolQuery subBool = new ElasticSearch.Searcher.BoolQuery();
                    ElasticSearch.Searcher.BoolQuery notTypesBool = new ElasticSearch.Searcher.BoolQuery();

                    // the original term is a MUST (and)
                    subBool.AddChild(term, CutWith.AND);

                    // Create a prefix term for each asset type
                    foreach (var assetType in leaf.assetTypes)
                    {
                        ESPrefix prefix = new ESPrefix()
                        {
                            Key = "_type",
                            Value = assetType.ToString().ToLower()
                        };

                        // the prefixes are SHOULD (or) [at least one of the following....)
                        subBool.AddChild(prefix, CutWith.OR);

                        notTypesBool.AddNot(prefix);
                    }

                    fatherBool.AddChild(subBool, CutWith.OR);
                    fatherBool.AddChild(notTypesBool, CutWith.OR);

                    term = fatherBool;
                }
            }
            // If it is a phrase, join all children in a bool query with the corresponding operand
            else if (root.type == BooleanNodeType.Parent)
            {
                term = new ElasticSearch.Searcher.BoolQuery();
                CutWith cut = CutWith.AND;

                // Simply conversion of enums: I could use casting but I don't want to trust it. 
                if ((root as BooleanPhrase).operand == ApiObjects.eCutType.Or)
                {
                    cut = CutWith.OR;
                }

                Dictionary<string, ESRange> rangesByName = new Dictionary<string, ESRange>();

                // Add every child node to the boolean query. This is recursive!
                foreach (var childNode in (root as BooleanPhrase).nodes)
                {
                    IESTerm newChild = ConvertToQuery(childNode);

                    // If we are handling ranges, I want to merge ranges of the same field to avoid complicated queries
                    if (newChild.eType == eTermType.RANGE)
                    {
                        string field = ((BooleanLeaf)childNode).field;

                        if (!rangesByName.ContainsKey(field))
                        {
                            rangesByName.Add(field, newChild as ESRange);
                        }
                        else
                        {
                            rangesByName[field].Value.AddRange((newChild as ESRange).Value);

                            // Set the new child to null because the condition has been merged with an older child
                            newChild = null;
                        }
                    }

                    if (newChild != null)
                    {
                        // If it is a SIMPLE NOT PHRASE
                        if (childNode.type == BooleanNodeType.Leaf &&
                            (((childNode as BooleanLeaf).operand == ApiObjects.ComparisonOperator.NotEquals) &&
                            (!(childNode as BooleanLeaf).shouldLowercase)))
                        {
                            // If the cut is "AND", simply add the child to the "must_not" list. ES cuts "must" and "must_not" with an AND
                            if (cut == CutWith.AND)
                            {
                                (term as ElasticSearch.Searcher.BoolQuery).AddNot(newChild);
                            }
                            else
                            {
                                // If the cut is "OR", we need to wrap it with a boolean query, so that the "should" clause still checks each
                                // term separately 
                                ElasticSearch.Searcher.BoolQuery booleanWrapper = new ElasticSearch.Searcher.BoolQuery();
                                booleanWrapper.AddNot(newChild);

                                (term as ElasticSearch.Searcher.BoolQuery).AddChild(booleanWrapper, cut);
                            }
                        }
                        else
                        {
                            (term as ElasticSearch.Searcher.BoolQuery).AddChild(newChild, cut);
                        }
                    }
                }
            }

            return (term);
        }

        private void HandleLanguageSpecificLeafField(BooleanLeaf leaf)
        {
            leaf.field = GetElasticsearchFieldName(leaf.isLanguageSpecific, this.SearchDefinitions.langauge, leaf.field, leaf.fieldType);
        }

        private IESTerm BuildRecordingIdTerm(BooleanLeaf leaf)
        {
            IESTerm result = null;

            if (leaf.operand == ApiObjects.ComparisonOperator.Equals)
            {
                string recordingId = "0";
                string domainRecordingId = leaf.value.ToString();
                if (SearchDefinitions.domainRecordingIdToRecordingIdMapping.ContainsKey(domainRecordingId))
                {
                    recordingId = SearchDefinitions.domainRecordingIdToRecordingIdMapping[domainRecordingId];
                }

                result = new ESTerm(true)
                {
                    Key = NamingHelper.RECORDING_ID,
                    Value = recordingId
                };
            }
            else if (leaf.operand == ApiObjects.ComparisonOperator.In)
            {
                List<string> domainRecordingIds = Convert.ToString(leaf.value).Split(',').ToList();
                List<string> recordingIds = new List<string>();

                foreach (string domainRecordingId in domainRecordingIds)
                {
                    string recordingId = "0";
                    if (SearchDefinitions.domainRecordingIdToRecordingIdMapping.ContainsKey(domainRecordingId))
                    {
                        recordingId = SearchDefinitions.domainRecordingIdToRecordingIdMapping[domainRecordingId];
                        recordingIds.Add(recordingId);
                    }
                    else
                    {
                        recordingIds.Add("0");
                    }
                }

                result = new ESTerms(true)
                {
                    Key = NamingHelper.RECORDING_ID
                };

                (result as ESTerms).Value.AddRange(recordingIds);
            }

            return result;
        }

        private IESTerm BuildAssetTypeQuery(BooleanLeaf leaf)
        {
            IESTerm result = null;

            string loweredValue = leaf.value.ToString().ToLower();

            if (loweredValue == "media")
            {
                result = mediaPrefixTerm;
            }
            else if (loweredValue == "epg")
            {
                result = epgPrefixTerm;
            }
            else if (loweredValue == "recording")
            {
                result = recordingPrefixTerm;
            }
            else
            {
                int assetType;

                if (int.TryParse(loweredValue, out assetType))
                {
                    result = new ESTerm(true)
                    {
                        Key = "media_type_id",
                        Value = loweredValue
                    };
                }
            }

            return result;
        }

        private IESTerm BuildUserInterestsQuery()
        {
            IESTerm result = new ElasticSearch.Searcher.BoolQuery();

            var userPreferences = this.SearchDefinitions.userPreferences;

            // Check that user has any preferences at all
            if ((this.SearchDefinitions.userPreferences != null) &&
                (((this.SearchDefinitions.userPreferences.Metas != null) &&
                (this.SearchDefinitions.userPreferences.Metas.Count > 0)) ||
                ((this.SearchDefinitions.userPreferences.Tags != null) &&
                (this.SearchDefinitions.userPreferences.Tags.Count > 0))))
            {
                if (userPreferences.Tags != null)
                {
                    foreach (var tag in userPreferences.Tags)
                    {
                        ESTerms terms = new ESTerms(false)
                        {
                            Key = string.Format("tags.{0}", tag.Key.ToLower())
                        };

                        terms.Value.AddRange(tag.Value.Select(s => s.ToLower()));

                        (result as ElasticSearch.Searcher.BoolQuery).AddChild(terms, CutWith.OR);
                    }
                }

                if (userPreferences.Metas != null)
                {
                    foreach (var meta in userPreferences.Metas)
                    {
                        ESTerms terms = new ESTerms(false)
                        {
                            Key = string.Format("metas.{0}", meta.Key.ToLower())
                        };

                        terms.Value.AddRange(meta.Value.Select(s => s.ToLower()));

                        (result as ElasticSearch.Searcher.BoolQuery).AddChild(terms, CutWith.OR);
                    }
                }
            }
            else
            {
                // If user has no preferences at all
                result = new ESTerm(true)
                {
                    Key = "_id",
                    Value = "-1"
                };
            }

            return result;
        }

        private IESTerm BuildEntitledAssetsQuery()
        {
            IESTerm result = null;

            ElasticSearch.Searcher.BoolQuery boolQuery = new ElasticSearch.Searcher.BoolQuery();

            BaseFilterCompositeType assetsFilter = new FilterCompositeType(CutWith.OR);

            var entitlementSearchDefinitions = this.SearchDefinitions.entitlementSearchDefinitions;

            #region Free Assets

            if (entitlementSearchDefinitions.freeAssets != null)
            {
                // Build terms of free assets
                foreach (var item in entitlementSearchDefinitions.freeAssets)
                {
                    FilterCompositeType idsFilter = new FilterCompositeType(CutWith.AND);

                    ESTerms idsTerm = new ESTerms(true)
                    {
                        Key = "_id"
                    };

                    idsTerm.Value.AddRange(item.Value);

                    idsFilter.AddChild(idsTerm);

                    switch (item.Key)
                    {
                        case ApiObjects.eAssetTypes.EPG:
                            {
                                idsFilter.AddChild(epgPrefixTerm);
                                break;
                            }
                        case ApiObjects.eAssetTypes.MEDIA:
                            {
                                idsFilter.AddChild(mediaPrefixTerm);
                                break;
                            }
                        case ApiObjects.eAssetTypes.UNKNOWN:
                        case ApiObjects.eAssetTypes.NPVR:
                        default:
                            break;
                    }

                    assetsFilter.AddChild(idsFilter);
                }
            }

            #endregion

            // Alternative: just check the is_free member
            ESTerm isFreeTerm = new ESTerm(true)
            {
                Key = "is_free",
                Value = "1"
            };

            #region Specific Assets

            if (entitlementSearchDefinitions.entitledPaidForAssets != null)
            {
                // Build terms of assets (PPVs) the user purchased and is entitled to watch
                foreach (var item in entitlementSearchDefinitions.entitledPaidForAssets)
                {
                    FilterCompositeType idsFilter = new FilterCompositeType(CutWith.AND);

                    ESTerms idsTerm = new ESTerms(true)
                    {
                        Key = "_id"
                    };

                    idsTerm.Value.AddRange(item.Value);

                    idsFilter.AddChild(idsTerm);

                    switch (item.Key)
                    {
                        case ApiObjects.eAssetTypes.EPG:
                            {
                                idsFilter.AddChild(epgPrefixTerm);
                                break;
                            }
                        case ApiObjects.eAssetTypes.MEDIA:
                            {
                                idsFilter.AddChild(mediaPrefixTerm);
                                break;
                            }
                        case ApiObjects.eAssetTypes.UNKNOWN:
                        case ApiObjects.eAssetTypes.NPVR:
                        default:
                            break;
                    }

                    assetsFilter.AddChild(idsFilter);
                }
            }

            ESFilteredQuery specificAssetsTerm = new ESFilteredQuery()
            {
                Filter = new QueryFilter()
                {
                    FilterSettings = assetsFilter
                }
            };

            #endregion

            ESTerms fileTypeTerm = null;

            if (entitlementSearchDefinitions.fileTypes != null &&
                entitlementSearchDefinitions.fileTypes.Count > 0)
            {
                fileTypeTerm = new ESTerms(true)
                {
                    Key = "free_file_types",
                };
                var fileTypes = entitlementSearchDefinitions.fileTypes.Select(t => t.ToString());
                fileTypeTerm.Value.AddRange(fileTypes);
            }

            // EPG Channel IDs
            if (entitlementSearchDefinitions.epgChannelIds != null)
            {
                ESTerms channelsTerm = new ESTerms(true)
                {
                    Key = "epg_channel_id"
                };

                channelsTerm.Value.AddRange(entitlementSearchDefinitions.epgChannelIds.Select(i => i.ToString()));

                var splittedTerms = channelsTerm.Split();

                foreach (var terms in splittedTerms)
                {
                    boolQuery.AddChild(terms, CutWith.OR);
                }
            }

            //
            if (entitlementSearchDefinitions.shouldGetPurchasedAssets)
            {
                // if we want ONLY ENTITLED assets
                // and user has no entitlements:
                // create an empty, dummy query
                if (!entitlementSearchDefinitions.shouldGetFreeAssets &&
                    (this.SubscriptionsQuery == null ||
                    (this.SubscriptionsQuery.IsEmpty() && specificAssetsTerm.Filter.FilterSettings.IsEmpty())))
                {
                    boolQuery.AddChild(new ESTerm(true)
                    {
                        Key = "_id",
                        Value = "-1"
                    },
                    CutWith.OR);
                }
                else
                {
                    // Connect all the channels in the entitled user's subscriptions
                    boolQuery.AddChild(this.SubscriptionsQuery, CutWith.OR);
                    boolQuery.AddChild(specificAssetsTerm, CutWith.OR);
                }
            }
            else
            {

            }

            // Get free assets only if requested in definitions
            if (entitlementSearchDefinitions.shouldGetFreeAssets)
            {
                boolQuery.AddChild(isFreeTerm, CutWith.OR);

                if (fileTypeTerm != null)
                {
                    boolQuery.AddChild(fileTypeTerm, CutWith.OR);
                }
            }

            // if we want to get the assets the user is NOT entitled to, we will invert everything we have done up until now
            if (entitlementSearchDefinitions.shouldSearchNotEntitled)
            {
                var notBoolQuery = new ElasticSearch.Searcher.BoolQuery();
                notBoolQuery.AddNot(boolQuery);
                result = notBoolQuery;
            }
            else
            {
                result = boolQuery;
            }

            return result;
        }

        /// <summary>
        /// Converts a leaf to an ESTerm or ESRange, according to its operator
        /// </summary>
        /// <param name="leaf"></param>
        /// <returns></returns>
        protected IESTerm ConvertToFilter(BooleanLeaf leaf)
        {
            IESTerm term = null;

            bool isNumeric = leaf.valueType == typeof(int) || leaf.valueType == typeof(long);
            string value = string.Empty;

            string leafField = leaf.field;
            HandleLanguageSpecificLeafField(leaf);

            // First find out the value to use in the filter body 
            if (isNumeric)
            {
                value = leaf.value.ToString();
            }
            else if (leaf.valueType == typeof(DateTime))
            {
                DateTime date = Convert.ToDateTime(leaf.value);

                if (date != null)
                {
                    value = date.ToString(ES_DATE_FORMAT);
                }
            }
            else if (leaf.value is IEnumerable<string>)
            {
                leaf.value = (leaf.value as IEnumerable<string>).Select(item => item.ToLower());
            }
            else
            {
                value = leaf.value.ToString().ToLower();
            }

            if (leaf.field == NamingHelper.RECORDING_ID)
            {
                List<string> domainRecordingIds = new List<string>();

                if (leaf.value is IEnumerable<string>)
                {
                    domainRecordingIds = (leaf.value as IEnumerable<string>).Select(item => item.ToLower()).ToList();
                }
                else
                {
                    domainRecordingIds = leaf.value.ToString().ToLower().Split(',').ToList(); ;
                }

                List<string> recordingIds = new List<string>();

                foreach (string domainRecordingId in domainRecordingIds)
                {
                    string recordingId = "0";
                    if (SearchDefinitions.domainRecordingIdToRecordingIdMapping.ContainsKey(domainRecordingId))
                    {
                        recordingId = SearchDefinitions.domainRecordingIdToRecordingIdMapping[domainRecordingId];
                        recordingIds.Add(recordingId);
                    }
                    else;
                    {
                        recordingIds.Add("0");
                    }
                }

                term = new ESTerms(true)
                {
                    Key = NamingHelper.RECORDING_ID
                };

                (term as ESTerms).Value.AddRange(recordingIds);
                return (term);
            }
            else if (leaf.field == NamingHelper.ASSET_TYPE)
            {
                leaf.field = "_type";
            }

            // Create the term according to the comparison operator
            switch (leaf.operand)
            {
                case ApiObjects.ComparisonOperator.Equals:
                    {
                        term = new ESTerm(isNumeric)
                        {
                            Key = leaf.field,
                            Value = value
                        };

                        break;
                    }
                case ApiObjects.ComparisonOperator.NotEquals:
                    {
                        term = new ESTerm(isNumeric)
                        {
                            Key = leaf.field,
                            Value = value,
                            isNot = true
                        };

                        break;
                    }
                case ApiObjects.ComparisonOperator.GreaterThanOrEqual:
                case ApiObjects.ComparisonOperator.GreaterThan:
                case ApiObjects.ComparisonOperator.LessThanOrEqual:
                case ApiObjects.ComparisonOperator.LessThan:
                    {
                        string rangeValue = value;
                        if (this.SearchDefinitions.numericEpgMetas.Contains(leafField) && this.SearchDefinitions.shouldSearchEpg)
                        {
                            leaf.field = GetElasticsearchFieldName(leaf.isLanguageSpecific, this.SearchDefinitions.langauge, 
                                $"padded_{leafField}", leaf.fieldType);
                            rangeValue = Media.PadValue(rangeValue);
                        }

                        term = ConvertToRange(leaf.field, rangeValue, leaf.operand, isNumeric);

                        break;
                    }
                case ApiObjects.ComparisonOperator.In:
                    {
                        term = new ESTerms(false)
                        {
                            Key = leaf.field
                        };

                        (term as ESTerms).Value.AddRange(leaf.value as IEnumerable<string>);

                        break;
                    }
                case ApiObjects.ComparisonOperator.NotIn:
                    {
                        term = new ESTerms(false)
                        {
                            Key = leaf.field,
                            isNot = true
                        };

                        (term as ESTerms).Value.AddRange(leaf.value as IEnumerable<string>);

                        break;
                    }
                case ApiObjects.ComparisonOperator.Prefix:
                    {
                        term = new ESPrefix()
                        {
                            Key = leaf.field,
                            Value = value
                        };

                        break;
                    }
                case ApiObjects.ComparisonOperator.Exists:
                    {
                        term = new ESExists()
                        {
                            Value = leaf.field
                        };

                        break;
                    }
                case ApiObjects.ComparisonOperator.NotExists:
                    {
                        term = new ESExists()
                        {
                            Value = leaf.field,
                            isNot = true
                        };

                        break;
                    }
                default:
                    break;
            }

            return (term);
        }

        private static IESTerm ConvertToRange(string field, string value, ApiObjects.ComparisonOperator comparisonOperator, bool isNumeric)
        {
            var term = new ESRange(isNumeric)
            {
                Key = field
            };

            eRangeComp rangeComparison = eRangeComp.GTE;

            switch (comparisonOperator)
            {
                case ApiObjects.ComparisonOperator.GreaterThanOrEqual:
                    {
                        rangeComparison = eRangeComp.GTE;
                        break;
                    }
                case ApiObjects.ComparisonOperator.GreaterThan:
                    {
                        rangeComparison = eRangeComp.GT;
                        break;
                    }
                case ApiObjects.ComparisonOperator.LessThanOrEqual:
                    {
                        rangeComparison = eRangeComp.LTE;
                        break;
                    }
                case ApiObjects.ComparisonOperator.LessThan:
                    {
                        rangeComparison = eRangeComp.LT;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            (term as ESRange).Value.Add(new KeyValuePair<eRangeComp, string>(rangeComparison, value));
            return term;
        }

        public static EsOrderAggregation GetOrderAggregationParameters(
            IReadOnlyCollection<IEsOrderByField> esOrderByFields)
        {
            var result = new EsOrderAggregation
            {
                DistinctOrder = "_term",
                DistinctDirection = "asc"
            };

            if (esOrderByFields?.Count != 1 || !(esOrderByFields.Single() is EsOrderByField esOrderByField))
            {
                return result;
            }

            var orderAggregation = GetOrderAggregation(esOrderByField);
            if (orderAggregation != null)
            {
                result.DistinctOrder = orderAggregation.Name;
                result.DistinctDirection = esOrderByField.OrderByDirection == OrderDir.DESC ? "desc" : "asc";
                result.OrderAggregation = orderAggregation;
            }

            return result;
        }

        private static ESBaseAggsItem GetOrderAggregation(EsOrderByField esOrderByField)
        {
            var result = new ESBaseAggsItem
            {
                Name = "order_aggregation",
                Type = esOrderByField.OrderByDirection == OrderDir.DESC
                    ? eElasticAggregationType.max
                    : eElasticAggregationType.min,
                Size = 0
            };

            switch (esOrderByField.OrderByField)
            {
                case OrderBy.START_DATE:
                    result.Field = "start_date";
                    break;
                case OrderBy.CREATE_DATE:
                    result.Field = "create_date";
                    break;
                case OrderBy.RELATED:
                case OrderBy.NONE:
                    result.Script = "_score";
                    break;
            }

            return !string.IsNullOrEmpty(result.Field) || !string.IsNullOrEmpty(result.Script)
                ? result
                : null;
        }

        private FilterCompositeType BuildMediaDatesComposite()
        {
            FilterCompositeType mediaDatesFilter = new FilterCompositeType(CutWith.AND);

            DateTime now = SystemDateTime.UtcNow;
            now = now.AddSeconds(-now.Second);
            string nowDateString = now.ToString("yyyyMMddHHmmss");
            string maximumDateString = DateTime.MaxValue.ToString("yyyyMMddHHmmss");

            ESRange mediaStartDateRange = new ESRange(false);

            if (this.SearchDefinitions.shouldUseStartDateForMedia)
            {
                mediaStartDateRange.Key = this.SearchDefinitions.shouldUseCatalogStartDateForMedia ? "catalog_start_date" : "start_date";
                string minimumDateString = DateTime.MinValue.ToString("yyyyMMddHHmmss");
                mediaStartDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, minimumDateString));
                mediaStartDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, nowDateString));
            }

            mediaDatesFilter.AddChild(mediaStartDateRange);

            if (!this.SearchDefinitions.shouldIgnoreEndDate)
            {
                ESRange mediaEndDateRange = new ESRange(false);
                mediaEndDateRange.Key = (this.SearchDefinitions.shouldUseFinalEndDate) ? "final_date" : "end_date";
                mediaEndDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, nowDateString));
                mediaEndDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, maximumDateString));

                mediaDatesFilter.AddChild(mediaEndDateRange);
            }

            return mediaDatesFilter;
        }

        #endregion

        public override void SetPagingForUnifiedSearch() => QueryInitializer.SetPagingForUnifiedSearch(this);

        public override void SetGroupByValuesForUnifiedSearch()
        {
            if (SearchDefinitions.groupBy != null && SearchDefinitions.groupBy.Any())
            {
                foreach (var groupBy in SearchDefinitions.groupBy)
                {
                    SetGroupByValue(groupBy, SearchDefinitions.langauge);
                }

                if (SearchDefinitions.distinctGroup != null)
                {
                    SetGroupByValue(SearchDefinitions.distinctGroup, SearchDefinitions.langauge);
                    SearchDefinitions.extraReturnFields.Add(SearchDefinitions.distinctGroup.Value);
                }
            }
        }

        private static void SetGroupByValue(GroupByDefinition groupBy, LanguageObj language)
        {
            var key = groupBy.Key.ToLower();
            var type = groupBy.Type;
            var isLanguageSpecific = LanguageSpecificGroupByFieldTypes.Contains(groupBy.Type);
            string value = GetElasticsearchFieldName(isLanguageSpecific, language, key, type);

            if (groupBy.Type == eFieldType.Tag || groupBy.Type == eFieldType.StringMeta)
            {
                value = $"{value}.lowercase";
            }

            groupBy.Value = value;
        }
    }
}
