using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects.SearchObjects;

namespace ElasticSearch.Searcher
{
    public class ESEpgQueryBuilder
    {
        public static readonly string AND_CONDITION = "AND";
        public static readonly string OR_CONDITION = "OR";
        public static readonly string METAS = "METAS";
        public static readonly string TAGS = "TAGS";
        protected readonly int MAX_RESULTS;

        public EpgSearchObj m_oEpgSearchObj { get; set; }
        public eQueryAnalyzer eAnalyzer { get; set; }
        public bool bAnalyzeWildcards { get; set; }
        public List<string> ReturnFields { get; protected set; }
        
        public ESEpgQueryBuilder()
        {
            ReturnFields = new List<string>() { "\"_id\"", "\"_index\"", "\"_type\"", "\"_score\"", "\"group_id\"", "\"epg_id\"", "\"name\", \"cache_date\"" };
            string sMaxResults = Common.Utils.GetWSURL("MAX_RESULTS");
            if (!int.TryParse(sMaxResults, out MAX_RESULTS))
                MAX_RESULTS = 100000;
        }

        public virtual string BuildSearchQueryString()
        {
            string sResult = string.Empty;

            if (m_oEpgSearchObj == null)
                return sResult;

            ESWildcard wildCard;
            BoolQuery mainBooleanQuery = new BoolQuery();
            BoolQuery textBooleanQuery = new BoolQuery();

            BoolQuery textBooleanQueryOR = new BoolQuery();
            BoolQuery textBooleanQueryAnd = new BoolQuery();

            //BoolQuery bQuerySingle;
            FilteredQuery filteredQuery = new FilteredQuery();

            //And OR List search rexts
            foreach (var kvp in m_oEpgSearchObj.m_lSearchOr)
            {
                //bQuerySingle = new BoolQuery();
                if (!m_oEpgSearchObj.m_bExact)
                {
                    wildCard = new ESWildcard() { Key = Common.Utils.EscapeValues(kvp.m_sKey), Value = string.Format("*{0}*", Common.Utils.EscapeValues(kvp.m_sValue)) };
                }
                else
                {
                    wildCard = new ESWildcard() { Key = Common.Utils.EscapeValues(kvp.m_sKey), Value = string.Format("{0}", Common.Utils.EscapeValues(kvp.m_sValue)) };
                }
                //bQuerySingle.AddChild(wildCard, CutWith.AND);

                textBooleanQueryOR.AddChild(wildCard, CutWith.OR);
            }

            mainBooleanQuery.AddChild(textBooleanQueryOR, CutWith.AND);

            if (m_oEpgSearchObj.m_bExact && m_oEpgSearchObj != null && m_oEpgSearchObj.m_lSearchAnd.Count > 0)
            {
                foreach (var kvp in m_oEpgSearchObj.m_lSearchAnd)
                {
                    //bQuerySingle = new BoolQuery();
                    wildCard = new ESWildcard() { Key = Common.Utils.EscapeValues(kvp.m_sKey), Value = string.Format("{0}", Common.Utils.EscapeValues(kvp.m_sValue)) };

                    //bQuerySingle.AddChild(wildCard, CutWith.AND);

                    textBooleanQueryAnd.AddChild(wildCard, CutWith.AND);
                }

                mainBooleanQuery.AddChild(textBooleanQueryAnd, CutWith.AND);
            }


            filteredQuery.Query = mainBooleanQuery;

            QueryFilter filter = new QueryFilter();
            BaseFilterCompositeType filterComposite = new FilterCompositeType(CutWith.AND);

            string sStartDate = this.m_oEpgSearchObj.m_dStartDate.ToString("yyyyMMddHHmmss");
            string sStartMin = this.m_oEpgSearchObj.m_dStartDate.AddDays(-1).ToString("yyyyMMddHHmmss");
            string sStartMax = this.m_oEpgSearchObj.m_dEndDate.AddDays(1).AddSeconds(-1).ToString("yyyyMMddHHmmss");

            ESRange minStartDateRange = new ESRange(false) { Key = "start_date" };
            minStartDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, sStartMin));
            minStartDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, sStartMax));

            ESRange maxStartDateRange = new ESRange(false) { Key = "end_date" };
            maxStartDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, sStartDate));

            ESTerm isActiveTerm = new ESTerm(true) { Key = "is_active", Value = "1" };


            filterComposite.AddChild(minStartDateRange);
            filterComposite.AddChild(maxStartDateRange);
            filterComposite.AddChild(isActiveTerm);

            FillFilterSettings(ref filter, filterComposite);
            //filter.FilterSettings = filterComposite;


            filteredQuery.Filter = filter;


            filteredQuery.PageSize = m_oEpgSearchObj.m_nPageSize;
            filteredQuery.PageIndex = m_oEpgSearchObj.m_nPageIndex;

            return filteredQuery.ToString();
        }

        private void FillFilterSettings(ref QueryFilter filter, BaseFilterCompositeType filterSettings)
        {
            if (IsUseIPNOFiltering())
            {
                filter.FilterSettings = new EpgChannelsFilterCompositeType(filterSettings, m_oEpgSearchObj.m_oEpgChannelIDs);
            }
            else
            {
                filter.FilterSettings = filterSettings;
            }

        }

        private bool IsUseIPNOFiltering()
        {
            return m_oEpgSearchObj != null && m_oEpgSearchObj.m_oEpgChannelIDs != null && m_oEpgSearchObj.m_oEpgChannelIDs.Count > 0;
        }

        public virtual string BuildEpgAutoCompleteQuery()
        {
            string sResult = string.Empty;

            if (m_oEpgSearchObj == null)
                return sResult;

            FilteredQuery filteredQuery = new FilteredQuery();
            MultiMatchQuery phrasePrefix = new MultiMatchQuery();

            if (m_oEpgSearchObj.m_lSearch.Count > 0)
            {
                #region build prefix query
                foreach (var kvp in m_oEpgSearchObj.m_lSearch)
                {
                    phrasePrefix.Fields.Add(kvp.m_sKey.ToLower());
                }

                phrasePrefix.Query = m_oEpgSearchObj.m_lSearch[0].m_sValue.ToLower(); // search value is identical in all search fields

                filteredQuery.Query = phrasePrefix;
                #endregion


            }

            #region build filter - is_active, start/end date
            QueryFilter filter = new QueryFilter();
            FilterCompositeType filterComposite = new FilterCompositeType(CutWith.AND);

            string sStartDate = this.m_oEpgSearchObj.m_dStartDate.ToString("yyyyMMddHHmmss");
            string sStartMin = this.m_oEpgSearchObj.m_dStartDate.AddDays(-1).ToString("yyyyMMddHHmmss");
            string sStartMax = this.m_oEpgSearchObj.m_dEndDate.AddDays(1).AddSeconds(-1).ToString("yyyyMMddHHmmss");

            ESRange minStartDateRange = new ESRange(false) { Key = "start_date" };
            minStartDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, sStartMin));
            minStartDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, sStartMax));

            ESRange maxStartDateRange = new ESRange(false) { Key = "end_date" };
            maxStartDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, sStartDate));

            ESTerm isActiveTerm = new ESTerm(true) { Key = "is_active", Value = "1" };


            filterComposite.AddChild(minStartDateRange);
            filterComposite.AddChild(maxStartDateRange);
            filterComposite.AddChild(isActiveTerm);
            filter.FilterSettings = filterComposite;


            filteredQuery.Filter = filter;
            #endregion

            filteredQuery.PageSize = m_oEpgSearchObj.m_nPageSize;
            filteredQuery.PageIndex = m_oEpgSearchObj.m_nPageIndex;

            sResult = filteredQuery.ToString();
            return sResult;
        }
    }
}