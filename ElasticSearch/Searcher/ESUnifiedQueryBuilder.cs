using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects.SearchObjects;

namespace ElasticSearch.Searcher
{
    public class ESUnifiedQueryBuilder
    {
        #region Data Members

        protected static readonly List<string> DEFAULT_RETURN_FIELDS = 
            new List<string>(7) { "\"_id\"", "\"_index\"", "\"_type\"", "\"_score\"", "\"group_id\"", "\"name\"", "\"cache_date\"",  "\"update_date\""};

        public static readonly string AND_CONDITION = "AND";
        public static readonly string OR_CONDITION = "OR";
        public static readonly string METAS = "METAS";
        public static readonly string TAGS = "TAGS";
        protected readonly int MAX_RESULTS;

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

        public List<string> ReturnFields
        {
            get;
            set;
        }

        #endregion

        #region Ctor

        /// <summary>
        /// Regulat constructor to initialize with definitions
        /// </summary>
        /// <param name="definitions"></param>
        public ESUnifiedQueryBuilder(UnifiedSearchDefinitions definitions)
        {
            this.ReturnFields = DEFAULT_RETURN_FIELDS.ToList();
            this.ReturnFields.AddRange(definitions.extraReturnFields);

            if (definitions.shouldSearchEpg)
            {
                this.ReturnFields.Add("\"epg_id\"");
            }
            
            if (definitions.shouldSearchMedia)
            {
                this.ReturnFields.Add("\"media_id\"");
            }

            this.SearchDefinitions = definitions;

            this.GroupID = definitions.groupId;

            string maxResults = Common.Utils.GetWSURL("MAX_RESULTS");

            if (!int.TryParse(maxResults, out MAX_RESULTS))
            {
                MAX_RESULTS = 100000;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Builds the request body for Elasticsearch search action
        /// </summary>
        /// <returns></returns>
        public virtual string BuildSearchQueryString()
        {
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

            FilterCompositeType mediaFilter = new FilterCompositeType(CutWith.AND);
            mediaFilter.AddChild(mediaPrefixTerm);

            FilterCompositeType epgFilter = new FilterCompositeType(CutWith.AND);
            epgFilter.AddChild(epgPrefixTerm);

            // Filters which are relevant to both types
            FilterCompositeType globalFilter = new FilterCompositeType(CutWith.AND);

            // Or between media and epg
            FilterCompositeType unifiedFilter = new FilterCompositeType(CutWith.OR);

            if (this.SearchDefinitions.shouldSearchEpg)
            {
                unifiedFilter.AddChild(epgFilter);
            }

            if (this.SearchDefinitions.shouldSearchMedia)
            {
                unifiedFilter.AddChild(mediaFilter);
            }

            StringBuilder filteredQueryBuilder = new StringBuilder();
            string queryPart = string.Empty;

            ESTerm groupTerm = new ESTerm(true)
            {
                Key = "group_id",
                Value = this.SearchDefinitions.groupId.ToString()
            };

            ESTerm isActiveTerm = new ESTerm(true)
            {
                Key = "is_active",
                Value = "1"
            };

            globalFilter.AddChild(isActiveTerm);

            // Dates filter: 
            // If it is media, it should start before now and end after now
            // If it is EPG, it should start and end around the current week

            FilterCompositeType epgDatesFilter = null;

            // epg ranges
            if (this.SearchDefinitions.shouldSearchEpg)
            {
                #region Epg Dates ranges

                epgDatesFilter = new FilterCompositeType(CutWith.AND);

                string nowPlusOffsetDateString = DateTime.UtcNow.AddDays(this.SearchDefinitions.epgDaysOffest).ToString("yyyyMMddHHmmss");
                string nowMinusOffsetDateString = DateTime.UtcNow.AddDays(this.SearchDefinitions.epgDaysOffest).ToString("yyyyMMddHHmmss");

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

                if (!epgDatesFilter.IsEmpty())
                {
                    epgFilter.AddChild(epgDatesFilter);
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

                mediaFilter.AddChild(deviceRulesTerms);

                #endregion

                #region Media Dates ranges

                FilterCompositeType mediaDatesFilter = new FilterCompositeType(CutWith.AND);

                string nowDateString = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                string maximumDateString = DateTime.MaxValue.ToString("yyyyMMddHHmmss");

                if (this.SearchDefinitions.defaultStartDate)
                {
                    ESRange mediaStartDateRange = new ESRange(false);

                    if (this.SearchDefinitions.shouldUseStartDate)
                    {
                        mediaStartDateRange.Key = "start_date";
                        string minimumDateString = DateTime.MinValue.ToString("yyyyMMddHHmmss");
                        mediaStartDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, minimumDateString));
                        mediaStartDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, nowDateString));
                    }

                    mediaDatesFilter.AddChild(mediaStartDateRange);
                }

                if (this.SearchDefinitions.defaultEndDate)
                {
                    ESRange mediaEndDateRange = new ESRange(false);
                    mediaEndDateRange.Key = (this.SearchDefinitions.shouldUseFinalEndDate) ? "final_date" : "end_date";
                    mediaEndDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, nowDateString));
                    mediaEndDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, maximumDateString));

                    mediaDatesFilter.AddChild(mediaEndDateRange);
                }

                if (!mediaDatesFilter.IsEmpty())
                {
                    mediaFilter.AddChild(mediaDatesFilter);
                }

                #endregion
            }

            QueryFilter filterPart = new QueryFilter();

            #region Phrase Tree

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
                    if (leaf.operand == ApiObjects.ComparisonOperator.Contains)
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
                                if ((current as BooleanLeaf).operand == ApiObjects.ComparisonOperator.Contains)
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
                    var queryTerm = ConvertToQuery(queryNode);

                    if (!queryTerm.IsEmpty())
                    {
                        queryPart = queryTerm.ToString();
                    }
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

            filterPart.FilterSettings = filterParent;

            if (PageSize <= 0)
            {
                PageSize = MAX_RESULTS;
            }

            int fromIndex = (PageIndex <= 0) ? 0 : PageSize * PageIndex;

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

            if (!string.IsNullOrEmpty(queryPart))
            {
                filteredQueryBuilder.AppendFormat(" \"query\": {0},", queryPart.ToString());
            }

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

            if (definitions.shouldSearchEpg)
            {
                if (definitions.shouldSearchMedia)
                {
                    indexes = string.Format("{0},{0}_epg", groupId);
                }
                else
                {
                    indexes = string.Format("{0}_epg", groupId);
                }
            }
            else
            {
                indexes = groupId.ToString();
            }

            return indexes;
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
                composite.AddChild(ConvertToFilter(filterNode as BooleanLeaf));
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
        protected static IESTerm ConvertToQuery(BooleanPhraseNode root)
        {
            IESTerm term = null;

            // If it is a leaf, this is the stop condition: Simply convert to ESTerm
            if (root.type == BooleanNodeType.Leaf)
            {
                BooleanLeaf leaf = root as BooleanLeaf;
                string field = string.Format("{0}.analyzed", leaf.field);
                bool isNumeric = leaf.valueType == typeof(int) || leaf.valueType == typeof(long);

                // "Match" when search is not exact (contains)
                if (leaf.operand == ApiObjects.ComparisonOperator.Contains)
                {
                    term = new ESMatchQuery(ESMatchQuery.eMatchQueryType.match)
                    {
                        Field = field,
                        eOperator = CutWith.OR,
                        Query = leaf.value.ToString().ToLower()
                    };
                }
                // "Term" when search is equals/not equals
                else if (leaf.operand == ApiObjects.ComparisonOperator.Equals ||
                    leaf.operand == ApiObjects.ComparisonOperator.NotEquals)
                {
                    bool not = leaf.operand == ApiObjects.ComparisonOperator.NotEquals;

                    term = new ESTerm(isNumeric)
                    {
                        Key = leaf.field,
                        Value = leaf.value.ToString().ToLower(),
                        bNot = not
                    };
                }
                // Other cases are "Range"
                else
                {
                    term = ConvertToRange(leaf, isNumeric);
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

                Dictionary<string, List<BooleanLeaf>> rangesByName = new Dictionary<string, List<BooleanLeaf>>();

                // Add every child node to the boolean query. This is recursive!
                foreach (var childNode in (root as BooleanPhrase).nodes)
                {
                    IESTerm newChild = ConvertToQuery(childNode);
 
                    (term as BoolQuery).AddChild(newChild, cut);
                }
            }

            return (term);
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

            if (leaf.operand == ApiObjects.ComparisonOperator.Equals)
            {
                term = new ESTerm(isNumeric)
                {
                    Key = leaf.field,
                    Value = leaf.value.ToString().ToLower()
                };
            }
            else if (leaf.operand == ApiObjects.ComparisonOperator.NotEquals)
            {
                term = new ESTerm(isNumeric)
                {
                    Key = leaf.field,
                    Value = leaf.value.ToString().ToLower(),
                    bNot = true
                };
            }
            else
            {
                term = ConvertToRange(leaf, isNumeric);
            }

            return (term);
        }

        private static IESTerm ConvertToRange(BooleanLeaf leaf, bool isNumeric)
        {
            var term = new ESRange(isNumeric)
            {
                Key = leaf.field
            };

            eRangeComp rangeComparison = eRangeComp.GTE;

            switch (leaf.operand)
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

            (term as ESRange).Value.Add(new KeyValuePair<eRangeComp, string>(rangeComparison, leaf.value.ToString().ToLower()));
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
            StringBuilder sSort = new StringBuilder();
            sSort.Append(" \"sort\": [{");

            if (order.m_eOrderBy == OrderBy.META)
            {
                string sAnalyzedMeta = string.Format("metas.{0}", order.m_sOrderValue.ToLower());
                sSort.AppendFormat("\"{0}\": ", sAnalyzedMeta);
                ReturnFields.Add(string.Format("\"{0}\"", sAnalyzedMeta));

            }
            else if (order.m_eOrderBy == OrderBy.ID)
            {
                sSort.Append(" \"_uid\": ");
            }
            else if (order.m_eOrderBy == OrderBy.RELATED)
            {
                sSort.Append(" \"_score\": ");
            }
            else
            {
                sSort.AppendFormat(" \"{0}\": ", Enum.GetName(typeof(OrderBy), order.m_eOrderBy).ToLower());
            }

            if (sSort.Length > 0)
            {
                sSort.Append(" {");
                sSort.AppendFormat("\"order\": \"{0}\"", order.m_eOrderDir.ToString().ToLower());
                sSort.Append("}}");
            }

            //we always add the score at the end of the sorting so that our records will be in best order when using wildcards in the query itself
            if (shouldOrderByScore)
                sSort.Append(", \"_score\"");

            sSort.Append(" ]");

            return sSort.ToString();
        }

        #endregion
    }

}
