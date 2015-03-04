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

        public eQueryAnalyzer eAnalyzer
        {
            get;
            set;
        }
        public bool bAnalyzeWildcards
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

        public List<string> Types
        {
            get;
            set;
        }

        public List<string> ReturnFields
        {
            get;
            set;
        }
        public eQueryType QueryType
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

            switch (definitions.queryType)
            {
                case UnifiedQueryType.All:
                {
                    this.ReturnFields.Add("\"media_id\"");
                    this.ReturnFields.Add("\"epg_id\"");
                    break;
                }
                case UnifiedQueryType.Media:
                {
                    this.ReturnFields.Add("\"media_id\"");
                    break;
                }
                case UnifiedQueryType.EPG:
                {
                    this.ReturnFields.Add("\"epg_id\"");
                    break;
                }
                default:
                {
                    break;
                }
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
            string fullQuery = string.Empty;

            if (this.SearchDefinitions == null)
            {
                return fullQuery;
            }

            ESPrefix epgTypeTerm = new ESPrefix()
            {
                Key = "_type",
                Value = "epg"
            };

            ESPrefix mediaTypeTerm = new ESPrefix()
            {
                Key = "_type",
                Value = "media"
            };

            StringBuilder filteredQueryBuilder = new StringBuilder();
            string queryPart = string.Empty;

            ESTerm groupTerm = new ESTerm(true)
            {
                Key = "group_id",
                Value = this.SearchDefinitions.groupId.ToString()
            };

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

            ESTerm isActiveTerm = new ESTerm(true)
            {
                Key = "is_active",
                Value = "1"
            };

            // Dates filter: 
            // If it is media, it should start before now and end after now
            // If it is EPG, it should start and end around the current week
            // Media and EPG are connected by "OR"; inside it is connected with "AND"s
            FilterCompositeType mediaDatesFilter = new FilterCompositeType(CutWith.AND);
            FilterCompositeType epgDatesFilter = new FilterCompositeType(CutWith.AND);

            // media ranges

            string nowDateString = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            string maximumDateString = DateTime.MaxValue.ToString("yyyyMMddHHmmss");

            ESRange mediaStartDateRange = new ESRange(false);
            
            if (this.SearchDefinitions.shouldUseStartDate)
            {
                mediaStartDateRange.Key = "start_date";
                string sMin = DateTime.MinValue.ToString("yyyyMMddHHmmss");
                mediaStartDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, sMin));
                mediaStartDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, nowDateString));
            }

            ESRange mediaEndDateRange = new ESRange(false);
            mediaEndDateRange.Key = (this.SearchDefinitions.shouldUseFinalEndDate) ? "final_date" : "end_date";
            mediaEndDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, nowDateString));
            mediaEndDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, maximumDateString));

            mediaDatesFilter.AddChild(mediaStartDateRange);
            mediaDatesFilter.AddChild(mediaEndDateRange);
            mediaDatesFilter.AddChild(mediaTypeTerm);

            // epg ranges

            string nowPlusWeekDateString = DateTime.UtcNow.AddDays(7).ToString("yyyyMMddHHmmss");
            string nowMinusWeekDateString = DateTime.UtcNow.AddDays(-7).ToString("yyyyMMddHHmmss");

            ESRange epgStartDateRange = new ESRange(false)
            {
                Key = "start_date"
            };

            epgStartDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, nowMinusWeekDateString));
            epgStartDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, nowPlusWeekDateString));

            ESRange epgEndDateRange = new ESRange(false)
            {
                Key = "end_date"
            };

            epgEndDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, nowMinusWeekDateString));
            epgEndDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, nowPlusWeekDateString));

            epgDatesFilter.AddChild(epgStartDateRange);
            epgDatesFilter.AddChild(epgEndDateRange);
            epgDatesFilter.AddChild(epgTypeTerm);

            // connect media and epg with or
            FilterCompositeType datesFilter = new FilterCompositeType(CutWith.OR);
            datesFilter.AddChild(mediaDatesFilter);
            datesFilter.AddChild(epgDatesFilter);

            ESTerms userTypeTerm = new ESTerms(true);
            userTypeTerm.Key = "user_types";
            userTypeTerm.Value.Add("0");
            
            if (this.SearchDefinitions.userTypeID > 0)
            {
                userTypeTerm.Value.Add(this.SearchDefinitions.userTypeID.ToString());
            }

            ESTerms mediaTypesTerms = new ESTerms(true);

            if (!string.IsNullOrEmpty(this.SearchDefinitions.mediaTypes) && !
                this.SearchDefinitions.mediaTypes.Equals("0"))
            {
                mediaTypesTerms.Key = "media_type_id";
                string[] mediaTypeArr = this.SearchDefinitions.mediaTypes.Split(';');
                
                foreach (string mediaType in mediaTypeArr)
                {
                    if (!string.IsNullOrWhiteSpace(mediaType))
                    {
                        mediaTypesTerms.Value.Add(mediaType.Trim());
                    }
                }
            }

            // should be at least one of these three:
            // group_id = parent groupd id
            // permitted watch filter 
            // or it is EPG
            FilterCompositeType oGroupWPComposite = new FilterCompositeType(CutWith.OR);

            oGroupWPComposite.AddChild(groupTerm);
            oGroupWPComposite.AddChild(permittedWatchFilter);
            oGroupWPComposite.AddChild(epgTypeTerm);

            QueryFilter filterPart = new QueryFilter();
            BaseFilterCompositeType filterParent = new FilterCompositeType(CutWith.AND);

            filterParent.AddChild(oGroupWPComposite);
            filterParent.AddChild(isActiveTerm);
            filterParent.AddChild(datesFilter);
            //filterParent.AddChild(userTypeTerm);
            filterParent.AddChild(mediaTypesTerms);

            filterPart.FilterSettings = filterParent;

            // Use the and/or parts.
            // If it is exact search - no query, just use terms in filter
            if (QueryType == eQueryType.EXACT)
            {
                if (this.SearchDefinitions.order.m_eOrderBy != OrderBy.RELATED)
                {
                    FilterCompositeType andComposite = this.FilterMetasAndTagsConditions(this.SearchDefinitions.andList, CutWith.AND);
                    FilterCompositeType orComposite = this.FilterMetasAndTagsConditions(this.SearchDefinitions.orList, CutWith.OR);
                    FilterCompositeType generatedComposite =
                        this.FilterMetasAndTagsConditions(this.SearchDefinitions.filterTagsAndMetas,
                            (CutWith)this.SearchDefinitions.filterTagsAndMetasCutWith);

                    filterParent.AddChild(andComposite);
                    filterParent.AddChild(orComposite);
                    filterParent.AddChild(generatedComposite);
                }
            }
            // Not exact == boolean; Use query for and/or parts
            else if (QueryType == eQueryType.BOOLEAN)
            {
                BoolQuery oAndBoolQuery = this.QueryMetasAndTagsConditions(this.SearchDefinitions.andList, CutWith.AND);
                BoolQuery oOrBoolQuery = this.QueryMetasAndTagsConditions(this.SearchDefinitions.orList, CutWith.OR);
                BoolQuery oMultiFilterBoolQuery =
                    this.QueryMetasAndTagsConditions(this.SearchDefinitions.filterTagsAndMetas, (CutWith)this.SearchDefinitions.filterTagsAndMetasCutWith);

                BoolQuery oBoolQuery = new BoolQuery();
                oBoolQuery.AddChild(oAndBoolQuery, CutWith.AND);
                oBoolQuery.AddChild(oOrBoolQuery, CutWith.AND);
                oBoolQuery.AddChild(oMultiFilterBoolQuery, CutWith.AND);

                queryPart = oBoolQuery.ToString();

            }

            if (PageSize <= 0)
            {
                PageSize = MAX_RESULTS;
            }

            int fromIndex = (PageIndex <= 0) ? 0 : PageSize * PageIndex;

            filteredQueryBuilder.Append("{");
            filteredQueryBuilder.AppendFormat(" \"size\": {0}, ", PageSize);
            filteredQueryBuilder.AppendFormat(" \"from\": {0}, ", fromIndex);

            bool bExact = (QueryType == eQueryType.EXACT);

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
        public static string GetIndexes(UnifiedQueryType queryType, int groupId)
        {
            string indexes = string.Empty;

            switch (queryType)
            {
                case UnifiedQueryType.All:
                {
                    indexes = string.Format("{0},{0}_epg", groupId);

                    break;
                }
                case UnifiedQueryType.Media:
                {
                    indexes = groupId.ToString();

                    break;
                }
                case UnifiedQueryType.EPG:
                {
                    indexes = string.Format("{0}_epg", groupId);

                    break;
                }
                default:
                {
                    break;
                }
            }

            return indexes;
        }

        #endregion

        #region Protected and Private Methods

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

        protected FilterCompositeType FilterMetasAndTagsConditions(List<SearchValue> searchValues, CutWith cutWith)
        {
            if (searchValues == null)
            {
                return null;
            }

            FilterCompositeType parent = new FilterCompositeType(cutWith);

            foreach (SearchValue searchValue in searchValues)
            {
                string sSearchKey = Common.Utils.GetKeyNameWithPrefix(searchValue.m_sKey.ToLower(), searchValue.m_sKeyPrefix);

                if (searchValue.m_eInnerCutWith == ApiObjects.SearchObjects.CutWith.AND)
                {
                    FilterCompositeType composite = new FilterCompositeType(CutWith.AND);
                    
                    foreach (string value in searchValue.m_lValue)
                    {
                        composite.AddChild(new ESTerm(false)
                        {
                            Key = sSearchKey,
                            Value = value.ToLower()
                        });
                    }

                    parent.AddChild(composite);
                }
                else if (searchValue.m_eInnerCutWith == ApiObjects.SearchObjects.CutWith.OR)
                {
                    FilterCompositeType composite = new FilterCompositeType(CutWith.OR);
                    
                    if (searchValue.m_lValue.Count > 0)
                    {
                        ESTerms terms = new ESTerms(false);
                        terms.Key = sSearchKey;
                        terms.Value.AddRange(searchValue.m_lValue.ConvertAll(val => val.ToLower()));
                        composite.AddChild(terms);
                    }

                    parent.AddChild(composite);
                }
            }

            return parent;

        }

        protected BoolQuery QueryMetasAndTagsConditions(List<SearchValue> oSearchList, CutWith oAndOrCondition)
        {
            BoolQuery oBoolQuery = new BoolQuery();

            List<string> lMetasAndTagConditions = null;

            if (oSearchList != null && oSearchList.Count > 0)
            {
                lMetasAndTagConditions = new List<string>();

                foreach (SearchValue searchValue in oSearchList)
                {
                    BoolQuery oValueBoolQuery = new BoolQuery();
                    if (!string.IsNullOrEmpty(searchValue.m_sKey))
                    {
                        string sSearchKey = string.Concat(Common.Utils.GetKeyNameWithPrefix(searchValue.m_sKey.ToLower(), searchValue.m_sKeyPrefix),
                                                          ".analyzed");

                        foreach (string sValue in searchValue.m_lValue)
                        {
                            if (string.IsNullOrEmpty(sValue))
                                continue;

                            ESMatchQuery matchQuery = new ESMatchQuery(ESMatchQuery.eMatchQueryType.match)
                            {
                                eOperator = CutWith.AND,
                                Field = sSearchKey,
                                Query = sValue
                            };
                            oValueBoolQuery.AddChild(matchQuery, searchValue.m_eInnerCutWith);
                        }
                    }

                    oBoolQuery.AddChild(oValueBoolQuery, oAndOrCondition);
                }
            }

            return oBoolQuery;
        }

        #endregion
    }

}
