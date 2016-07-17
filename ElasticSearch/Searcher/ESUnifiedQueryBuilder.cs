using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects.SearchObjects;

namespace ElasticSearch.Searcher
{
    public class ESUnifiedQueryBuilder
    {
        #region Consts and readonlys 

        protected static readonly List<string> DEFAULT_RETURN_FIELDS = new List<string>(7) { 
                "\"_id\"", "\"_index\"", "\"_type\"", "\"_score\"", "\"group_id\"", "\"name\"", "\"cache_date\"",  "\"update_date\""};

        public static readonly string AND_CONDITION = "AND";
        public static readonly string OR_CONDITION = "OR";
        public static readonly string METAS = "METAS";
        public static readonly string TAGS = "TAGS";
        public static readonly string ES_DATE_FORMAT = "yyyyMMddHHmmss";

        public const string ENTITLED_ASSETS_FIELD = "entitled_assets";

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
        
        public UnifiedSearchDefinitions SearchDefinitions
        {
            get;
            set;
        }

        public int GroupID
        {
            get;
            set;
        }
        public int PageSize
        {
            get;
            set;
        }
        public int PageIndex
        {
            get;
            set;
        }

        public int From
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
        public BoolQuery SubscriptionsQuery
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
            string maxResults = Common.Utils.GetWSURL("MAX_RESULTS");

            if (!int.TryParse(maxResults, out MAX_RESULTS))
            {
                MAX_RESULTS = 100000;
            }
        }

        /// <summary>
        /// Regular constructor to initialize with definitions
        /// </summary>
        /// <param name="definitions"></param>
        public ESUnifiedQueryBuilder(UnifiedSearchDefinitions definitions, int groupId = 0)
        {
            this.SearchDefinitions = definitions;

            if (definitions != null)
            {
                this.GroupID = definitions.groupId;
            }
            else
            {
                this.GroupID = groupId;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Builds the request body for Elasticsearch search action
        /// </summary>
        /// <returns></returns>
        public virtual string BuildSearchQueryString(bool bIgnoreDeviceRuleID = false, bool bAddActive = true)
        {
            this.ReturnFields = DEFAULT_RETURN_FIELDS.ToList();
            this.ReturnFields.AddRange(this.SearchDefinitions.extraReturnFields.Select(field => string.Format("\"{0}\"", field)));

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

            if (PageSize <= 0)
            {
                PageSize = MAX_RESULTS;
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

            StringBuilder filteredQueryBuilder = new StringBuilder();

            filteredQueryBuilder.Append("{");
            filteredQueryBuilder.AppendFormat(" \"size\": {0}, ", PageSize);
            filteredQueryBuilder.AppendFormat(" \"from\": {0}, ", fromIndex);

            // TODO - find if the search is exact or not!
            bool bExact = false;

            // If not exact, order by score, and vice versa
            string sSort = GetSort(this.SearchDefinitions.order, !bExact);

            // Join return fields with commas
            if (ReturnFields.Count > 0)
            {
                filteredQueryBuilder.Append("\"fields\": [");

                filteredQueryBuilder.Append(ReturnFields.Aggregate((current, next) => string.Format("{0}, {1}", current, next)));

                filteredQueryBuilder.Append("], ");
            }

            filteredQueryBuilder.AppendFormat("{0}, ", sSort);
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
            fullQuery = filteredQueryBuilder.ToString();

            return fullQuery;
        }

        public void BuildInnerFilterAndQuery(out BaseFilterCompositeType filterPart, out IESTerm queryTerm, 
            bool ignoreDeviceRuleID = false, bool isActiveOnly = true)
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

            if (isActiveOnly)
            {
                ESTerm isActiveTerm = new ESTerm(true)
                {
                    Key = "is_active",
                    Value = "1"
                };
            
                globalFilter.AddChild(isActiveTerm);
            }

            #region Specific assets - included and excluded

            // If specific assets should return, filter their IDs.
            // Add an IN Clause (Terms) to the matching filter (media, EPG etc.)
            if (this.SearchDefinitions.specificAssets != null)
            {
                foreach (var item in this.SearchDefinitions.specificAssets)
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
                string nowPlusOffsetDateString = DateTime.UtcNow.AddDays(this.SearchDefinitions.epgDaysOffest).ToString("yyyyMMddHHmmss");
                string nowMinusOffsetDateString = DateTime.UtcNow.AddDays(-this.SearchDefinitions.epgDaysOffest).ToString("yyyyMMddHHmmss");

                if (this.SearchDefinitions.defaultStartDate)
                {
                    ESRange epgStartDateRange = new ESRange(false)
                    {
                        Key = "start_date"
                    };

                    epgStartDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, nowMinusOffsetDateString));
                    epgStartDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, nowPlusOffsetDateString));

                    epgDatesFilter.AddChild(epgStartDateRange);
                }

                if (this.SearchDefinitions.defaultEndDate)
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
                    epgSearchEndDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GT, DateTime.UtcNow.ToString("yyyyMMddHHmmss")));
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
            }

            // Media specific filters - user types, media types etc.
            if (this.SearchDefinitions.shouldSearchMedia)
            {
                #region User Types

                ESTerms userTypeTerm = new ESTerms(true);
                userTypeTerm.Key = "user_types";
                userTypeTerm.Value.Add("0");

                if (this.SearchDefinitions.userTypeID > 0)
                {
                    userTypeTerm.Value.Add(this.SearchDefinitions.userTypeID.ToString());
                }

                mediaFilter.AddChild(userTypeTerm);

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

                mediaFilter.AddChild(groupWPComposite);

                #endregion

                #region Device types

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

                if (!ignoreDeviceRuleID)
                    mediaFilter.AddChild(deviceRulesTerms);

                #endregion

                #region Media Dates ranges

                FilterCompositeType mediaDatesFilter = new FilterCompositeType(CutWith.AND);

                string nowDateString = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                string maximumDateString = DateTime.MaxValue.ToString("yyyyMMddHHmmss");

                ESRange mediaStartDateRange = new ESRange(false);

                if (this.SearchDefinitions.shouldUseStartDate)
                {
                    mediaStartDateRange.Key = "start_date";
                    string minimumDateString = DateTime.MinValue.ToString("yyyyMMddHHmmss");
                    mediaStartDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, minimumDateString));
                    mediaStartDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, nowDateString));
                }

                mediaDatesFilter.AddChild(mediaStartDateRange);

                ESRange mediaEndDateRange = new ESRange(false);
                mediaEndDateRange.Key = (this.SearchDefinitions.shouldUseFinalEndDate) ? "final_date" : "end_date";
                mediaEndDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, nowDateString));
                mediaEndDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, maximumDateString));

                mediaDatesFilter.AddChild(mediaEndDateRange);

                if (!mediaDatesFilter.IsEmpty())
                {
                    mediaFilter.AddChild(mediaDatesFilter);
                }

                #endregion

                #region Regions

                // region term 
                if (SearchDefinitions.regionIds != null && SearchDefinitions.regionIds.Count > 0)
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
            }

            // Recordings specific filters
            if (this.SearchDefinitions.shouldSearchRecordings)
            {
                // ? nothing for now?
            }

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
                        leaf.operand == ApiObjects.ComparisonOperator.WordStartsWith)
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
                                    (current as BooleanLeaf).operand == ApiObjects.ComparisonOperator.WordStartsWith)
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

            // Eventual filter will be:
            //
            // AND: [
            //          global,
            //          unified = OR
            //              [
            //                  epg, 
            //                  media
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
        public static string BuildGetUpdateDatesString(List<KeyValuePair<ApiObjects.eAssetTypes, string>> assets)
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
                        recordingIdsTerm.Value.Add(item.Value);
                        shouldSearchRecordings = true;
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

        /// <summary>
        /// Connects with "OR" several, different queries alltogther.
        /// </summary>
        /// <param name="unifiedSearchDefinitions"></param>
        /// <returns></returns>
        public string BuildMultiSearchQueryString(List<UnifiedSearchDefinitions> unifiedSearchDefinitions)
        {
            this.ReturnFields = DEFAULT_RETURN_FIELDS.ToList();

            this.ReturnFields.Add("\"epg_id\"");
            this.ReturnFields.Add("\"media_id\"");
            this.ReturnFields.Add("\"recording_id\"");

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

            FilteredQuery filteredQuery = new FilteredQuery(true)
            {
                PageIndex = 0,
                PageSize = 0
            };

            filteredQuery.Filter = new QueryFilter();
            filteredQuery.Filter.FilterSettings = new FilterCompositeType(CutWith.OR);

            BoolQuery boolquery = new BoolQuery();

            ESUnifiedQueryBuilder innerQueryBuilder = new ESUnifiedQueryBuilder(null, this.GroupID);

            foreach (var definition in unifiedSearchDefinitions)
            {
                BaseFilterCompositeType filterPart;
                IESTerm queryTerm;

                innerQueryBuilder.SearchDefinitions = definition;
                innerQueryBuilder.BuildInnerFilterAndQuery(out filterPart, out queryTerm);

                filteredQuery.Filter.FilterSettings.AddChild(filterPart);
                boolquery.AddChild(queryTerm, CutWith.OR);
            }

            filteredQuery.Query = boolquery;

            PageSize = MAX_RESULTS;

            int fromIndex = 0;

            StringBuilder filteredQueryBuilder = new StringBuilder();

            filteredQueryBuilder.Append("{");
            filteredQueryBuilder.AppendFormat(" \"size\": {0}, ", PageSize);
            filteredQueryBuilder.AppendFormat(" \"from\": {0}, ", fromIndex);

            // TODO - find if the search is exact or not!
            bool bExact = false;

            // If not exact, order by score, and vice versa
            string sSort = GetSort(this.SearchDefinitions.order, !bExact);

            // Join return fields with commas
            if (ReturnFields.Count > 0)
            {
                filteredQueryBuilder.Append("\"fields\": [");

                filteredQueryBuilder.Append(ReturnFields.Aggregate((current, next) => string.Format("{0}, {1}", current, next)));

                filteredQueryBuilder.Append("], ");
            }

            filteredQueryBuilder.AppendFormat("{0}, ", sSort);
            filteredQueryBuilder.Append(" \"query\": { \"filtered\": {");

            if (filteredQuery.Query != null && !filteredQuery.Query.IsEmpty())
            {
                string queryPart = filteredQuery.Query.ToString();

                if (!string.IsNullOrEmpty(queryPart))
                {
                    filteredQueryBuilder.AppendFormat(" \"query\": {0},", queryPart.ToString());
                }
            }

            filteredQueryBuilder.Append(filteredQuery.Filter.ToString());
            filteredQueryBuilder.Append(" } } }");
            fullQuery = filteredQueryBuilder.ToString();

            return fullQuery;
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
        protected IESTerm ConvertToQuery(BooleanPhraseNode root)
        {
            IESTerm term = null;

            // If it is a leaf, this is the stop condition: Simply convert to ESTerm
            if (root.type == BooleanNodeType.Leaf)
            {
                BooleanLeaf leaf = root as BooleanLeaf;

                // Special case - if this is the entitled assets leaf, we build a specific term for it
                if (leaf.field == ENTITLED_ASSETS_FIELD)
                {
                    term = BuildEntitledAssetsQuery();
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
                    else
                    {
                        value = leaf.value.ToString().ToLower();
                    }

                    // "Match" when search is not exact (contains)
                    if (leaf.operand == ApiObjects.ComparisonOperator.Contains ||
                        leaf.operand == ApiObjects.ComparisonOperator.WordStartsWith)
                    {
                        string field = string.Empty;

                        if (leaf.operand == ApiObjects.ComparisonOperator.WordStartsWith)
                        {
                            field = string.Format("{0}.autocomplete", leaf.field);
                        }
                        else
                        {
                            field = string.Format("{0}.analyzed", leaf.field);
                        }

                        term = new ESMatchQuery(ESMatchQuery.eMatchQueryType.match)
                        {
                            Field = field,
                            eOperator = CutWith.AND,
                            Query = value
                        };
                    }
                    // "bool" with "must_not" when no contains
                    else if (leaf.operand == ApiObjects.ComparisonOperator.NotContains)
                    {
                        string field = string.Format("{0}.analyzed", leaf.field);

                        term = new BoolQuery();

                        (term as BoolQuery).AddNot(
                            new ESMatchQuery(ESMatchQuery.eMatchQueryType.match)
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
                            Key = leaf.field,
                            Value = value
                        };
                    }
                    else if (leaf.operand == ApiObjects.ComparisonOperator.Prefix)
                    {
                        term = new ESPrefix()
                        {
                            Key = leaf.field,
                            Value = value
                        };
                    }
                    else if (leaf.operand == ApiObjects.ComparisonOperator.In)
                    {
                        term = new ESTerms(false)
                        {
                            Key = leaf.field
                        };

                        (term as ESTerms).Value.AddRange(leaf.value as IEnumerable<string>);
                    }
                    else if (leaf.operand == ApiObjects.ComparisonOperator.NotIn)
                    {
                        term = new ESTerms(false)
                        {
                            Key = leaf.field,
                            isNot = true
                        };

                        (term as ESTerms).Value.AddRange(leaf.value as IEnumerable<string>);
                    }
                    // Other cases are "Range"
                    else
                    {
                        term = ConvertToRange(leaf.field, value, leaf.operand, isNumeric);
                    }
                }

                // If this leaf is relevant only to certain asset types - create a bool query connecting the types and the term
                if (leaf.assetTypes != null && leaf.assetTypes.Count > 0)
                {
                    BoolQuery fatherBool = new BoolQuery();
                    BoolQuery subBool = new BoolQuery();
                    BoolQuery notTypesBool = new BoolQuery();

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
                term = new BoolQuery();
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
                        // If it is a "NOT" phrase (not contains or not equals)
                        if (childNode.type == BooleanNodeType.Leaf &&
                            (((childNode as BooleanLeaf).operand == ApiObjects.ComparisonOperator.NotEquals) ||
                            ((childNode as BooleanLeaf).operand == ApiObjects.ComparisonOperator.NotContains)))
                        {
                            // If the cut is "AND", simply add the child to the "must_not" list. ES cuts "must" and "must_not" with an AND
                            if (cut == CutWith.AND)
                            {
                                (term as BoolQuery).AddNot(newChild);
                            }
                            else
                            {
                                // If the cut is "OR", we need to wrap it with a boolean query, so that the "should" clause still checks each
                                // term separately 
                                BoolQuery booleanWrapper = new BoolQuery();
                                booleanWrapper.AddNot(newChild);

                                (term as BoolQuery).AddChild(booleanWrapper, cut);
                            }
                        }
                        else
                        {
                            (term as BoolQuery).AddChild(newChild, cut);
                        }
                    }   
                }
            }

            return (term);
        }

        private IESTerm BuildEntitledAssetsQuery()
        {
            BoolQuery result = new BoolQuery();

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

                result.AddChild(channelsTerm, CutWith.OR);
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
                    result.AddChild(new ESTerm(true)
                    {
                        Key = "_uid",
                        Value = "-1"
                    }, 
                    CutWith.OR);
                }
                else
                {

                    // Connect all the channels in the entitled user's subscriptions
                    result.AddChild(this.SubscriptionsQuery, CutWith.OR);
                    result.AddChild(specificAssetsTerm, CutWith.OR);
                }
            }
            else
            {

            }

            // Get free assets only if requested in definitions
            if (entitlementSearchDefinitions.shouldGetFreeAssets)
            {
                result.AddChild(isFreeTerm, CutWith.OR);

                if (fileTypeTerm != null)
                {
                    result.AddChild(fileTypeTerm, CutWith.OR);
                }
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

            // Create the term according to the comparison operator
            if (leaf.operand == ApiObjects.ComparisonOperator.Equals)
            {
                term = new ESTerm(isNumeric)
                {
                    Key = leaf.field,
                    Value = value
                };
            }
            else if (leaf.operand == ApiObjects.ComparisonOperator.NotEquals)
            {
                term = new ESTerm(isNumeric)
                {
                    Key = leaf.field,
                    Value = value,
                    isNot = true
                };
            }
            else if (leaf.operand == ApiObjects.ComparisonOperator.Prefix)
            {
                term = new ESPrefix()
                {
                    Key = leaf.field,
                    Value = value
                };
            }
            else if (leaf.operand == ApiObjects.ComparisonOperator.In)
            {
                term = new ESTerms(false)
                {
                    Key = leaf.field
                };

                (term as ESTerms).Value.AddRange(leaf.value as IEnumerable<string>);
            }
            else if (leaf.operand == ApiObjects.ComparisonOperator.NotIn)
            {
                term = new ESTerms(false)
                {
                    Key = leaf.field,
                    isNot = true
                };

                (term as ESTerms).Value.AddRange(leaf.value as IEnumerable<string>);
            }
            else
            {
                term = ConvertToRange(leaf.field, value, leaf.operand, isNumeric);
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

        /// <summary>
        /// Returns the sort string for the query
        /// </summary>
        /// <param name="order"></param>
        /// <param name="shouldOrderByScore"></param>
        /// <returns></returns>
        protected string GetSort(OrderObj order, bool shouldOrderByScore)
        {
            StringBuilder sortBuilder = new StringBuilder();
            sortBuilder.Append(" \"sort\": [{");

            if (order.m_eOrderBy == OrderBy.META)
            {
                string sAnalyzedMeta = string.Format("metas.{0}", order.m_sOrderValue.ToLower());
                sortBuilder.AppendFormat("\"{0}\": ", sAnalyzedMeta);
                ReturnFields.Add(string.Format("\"{0}\"", sAnalyzedMeta));
            }
            else if (order.m_eOrderBy == OrderBy.ID)
            {
                sortBuilder.Append(" \"_uid\": ");
            }
            else if (order.m_eOrderBy == OrderBy.RELATED || order.m_eOrderBy == OrderBy.NONE)
            {
                sortBuilder.Append(" \"_score\": ");
            }
            else
            {
                sortBuilder.AppendFormat(" \"{0}\": ", Enum.GetName(typeof(OrderBy), order.m_eOrderBy).ToLower());
            }

            if (sortBuilder.Length > 0)
            {
                sortBuilder.Append(" {");
                sortBuilder.AppendFormat("\"order\": \"{0}\"", order.m_eOrderDir.ToString().ToLower());
                sortBuilder.Append("}}");
            }

            //we always add the score at the end of the sorting so that our records will be in best order when using wildcards in the query itself
            if (order.m_eOrderBy != OrderBy.ID &&
                shouldOrderByScore && order.m_eOrderBy != OrderBy.RELATED && order.m_eOrderBy != OrderBy.NONE)
            {
                sortBuilder.Append(", \"_score\"");
            }

            if (order.m_eOrderBy != OrderBy.ID)
            {
                // Always add sort by _uid to avoid ES weirdness of same sort-value 
                sortBuilder.Append(", { \"_uid\": { \"order\": \"desc\" } }");
            }

            sortBuilder.Append(" ]");

            return sortBuilder.ToString();
        }

        #endregion

    }

}
