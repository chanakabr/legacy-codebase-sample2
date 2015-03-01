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
            this.ReturnFields.AddRange(definitions.m_ExtraReturnFields);

            switch (definitions.m_QueryType)
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

            this.GroupID = definitions.m_nGroupId;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Builds the request body for Elasticsearch search action
        /// </summary>
        /// <returns></returns>
        public virtual string BuildSearchQueryString()
        {
            string query = string.Empty;

            if (this.SearchDefinitions == null)
            {
                return query;
            }

            StringBuilder sbFilteredQuery = new StringBuilder();
            string sQuery = string.Empty;

            QueryFilter filter = new QueryFilter();

            BaseFilterCompositeType filterParent = new FilterCompositeType(CutWith.AND);

            ESTerm groupTerm = new ESTerm(true)
            {
                Key = "group_id",
                Value = this.SearchDefinitions.m_nGroupId.ToString()
            };

            ESTerms permittedWatcFilter = new ESTerms(true);
           
            if (!string.IsNullOrEmpty(this.SearchDefinitions.m_sPermittedWatchRules))
            {
                permittedWatcFilter.Key = "wp_type_id";
                List<string> permittedValues = permittedWatcFilter.Value;
                foreach (string value in this.SearchDefinitions.m_sPermittedWatchRules.Split(' '))
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

            string sNow = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            string sMax = DateTime.MaxValue.ToString("yyyyMMddHHmmss");

            ESRange startDateRange = new ESRange(false);
            
            if (this.SearchDefinitions.m_bUseStartDate)
            {
                startDateRange.Key = "start_date";
                string sMin = DateTime.MinValue.ToString("yyyyMMddHHmmss");
                startDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, sMin));
                startDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, sNow));
            }

            ESRange endDateRange = new ESRange(false);
            endDateRange.Key = (this.SearchDefinitions.m_bUseFinalEndDate) ? "final_date" : "end_date";
            endDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, sNow));
            endDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, sMax));


            ESTerms userTypeTerm = new ESTerms(true);
            userTypeTerm.Key = "user_types";
            userTypeTerm.Value.Add("0");
            
            if (this.SearchDefinitions.m_nUserTypeID > 0)
            {
                userTypeTerm.Value.Add(this.SearchDefinitions.m_nUserTypeID.ToString());
            }

            ESTerms mediaTypesTerms = new ESTerms(true);

            if (!string.IsNullOrEmpty(this.SearchDefinitions.m_sMediaTypes) && !
                this.SearchDefinitions.m_sMediaTypes.Equals("0"))
            {
                mediaTypesTerms.Key = "media_type_id";
                string[] mediaTypeArr = this.SearchDefinitions.m_sMediaTypes.Split(';');
                
                foreach (string mediaType in mediaTypeArr)
                {
                    if (!string.IsNullOrWhiteSpace(mediaType))
                    {
                        mediaTypesTerms.Value.Add(mediaType.Trim());
                    }
                }
            }

            FilterCompositeType oGroupWPComposite = new FilterCompositeType(CutWith.OR);

            oGroupWPComposite.AddChild(groupTerm);
            oGroupWPComposite.AddChild(permittedWatcFilter);

            filterParent.AddChild(oGroupWPComposite);
            filterParent.AddChild(isActiveTerm);
            filterParent.AddChild(startDateRange);
            filterParent.AddChild(endDateRange);
            //filterParent.AddChild(userTypeTerm);
            filterParent.AddChild(mediaTypesTerms);

            if (QueryType == eQueryType.EXACT)
            {
                if (this.SearchDefinitions.m_oOrder.m_eOrderBy != OrderBy.RELATED)
                {
                    FilterCompositeType andComposite = this.FilterMetasAndTagsConditions(this.SearchDefinitions.m_dAnd, CutWith.AND);
                    FilterCompositeType orComposite = this.FilterMetasAndTagsConditions(this.SearchDefinitions.m_dOr, CutWith.OR);
                    FilterCompositeType generatedComposite = 
                        this.FilterMetasAndTagsConditions(this.SearchDefinitions.m_lFilterTagsAndMetas, 
                            (CutWith)this.SearchDefinitions.m_eFilterTagsAndMetasCutWith);

                    filterParent.AddChild(andComposite);
                    filterParent.AddChild(orComposite);
                    filterParent.AddChild(generatedComposite);
                }
            }
            else if (QueryType == eQueryType.BOOLEAN)
            {
                BoolQuery oAndBoolQuery = this.QueryMetasAndTagsConditions(this.SearchDefinitions.m_dAnd, CutWith.AND);
                BoolQuery oOrBoolQuery = this.QueryMetasAndTagsConditions(this.SearchDefinitions.m_dOr, CutWith.OR);
                BoolQuery oMultiFilterBoolQuery = 
                    this.QueryMetasAndTagsConditions(this.SearchDefinitions.m_lFilterTagsAndMetas, (CutWith)this.SearchDefinitions.m_eFilterTagsAndMetasCutWith);

                BoolQuery oBoolQuery = new BoolQuery();
                oBoolQuery.AddChild(oAndBoolQuery, CutWith.AND);
                oBoolQuery.AddChild(oOrBoolQuery, CutWith.AND);
                oBoolQuery.AddChild(oMultiFilterBoolQuery, CutWith.AND);

                sQuery = oBoolQuery.ToString();

            }
            //else if (QueryType == eQueryType.PHRASE_PREFIX)
            //{
            //    MultiMatchQuery multiMatch = GetMultiMatchQuery();
            //    sQuery = multiMatch.ToString();
            //}

            filter.FilterSettings = filterParent;

            if (PageSize <= 0)
                PageSize = MAX_RESULTS;

            int fromIndex = (PageIndex <= 0) ? 0 : PageSize * PageIndex;

            sbFilteredQuery.Append("{");
            sbFilteredQuery.AppendFormat(" \"size\": {0}, ", PageSize);
            sbFilteredQuery.AppendFormat(" \"from\": {0}, ", fromIndex);

            bool bExact = (QueryType == eQueryType.EXACT);

            // If not exact, order by score, and vice versa
            string sSort = GetSort(this.SearchDefinitions.m_oOrder, !bExact);

            // Join return fields with commas
            if (ReturnFields.Count > 0)
            {
                sbFilteredQuery.Append("\"fields\": [");

                sbFilteredQuery.Append(ReturnFields.Aggregate((current, next) => string.Format("{0}, {1}", current, next)));

                sbFilteredQuery.Append("], ");
            }

            sbFilteredQuery.AppendFormat("{0}, ", sSort);
            sbFilteredQuery.Append(" \"query\": { \"filtered\": {");

            if (!string.IsNullOrEmpty(sQuery))
            {
                sbFilteredQuery.AppendFormat(" \"query\": {0},", sQuery.ToString());
            }
            sbFilteredQuery.Append(filter.ToString());
            sbFilteredQuery.Append(" } } }");
            query = sbFilteredQuery.ToString();

            return query;
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
