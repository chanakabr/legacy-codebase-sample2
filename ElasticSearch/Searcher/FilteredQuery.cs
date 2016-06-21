using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects.SearchObjects;

namespace ElasticSearch.Searcher
{
    public class FilteredQuery
    {
        public readonly int MAX_RESULTS;

        public QueryFilter Filter { get; set; }
        public IESTerm Query { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public List<string> ReturnFields { get; protected set; }
        public List<ESOrderObj> ESSort { get; protected set; }
        public bool m_bIsRoot { get; set; }

        public FilteredQuery(bool bIsRoot = true)
        {
            ReturnFields = new List<string>() { "\"_id\"", "\"_index\"", "\"_type\"", "\"_score\"", "\"group_id\"", "\"media_id\"", "\"epg_id\"", "\"name\"", "\"cache_date\"", "\"update_date\"" };
            ESSort = new List<ESOrderObj>();
            string sMaxResults = Common.Utils.GetWSURL("MAX_RESULTS");
            m_bIsRoot = bIsRoot;

            if (!int.TryParse(sMaxResults, out MAX_RESULTS))
                MAX_RESULTS = 100000;
        }

        public override string ToString()
        {
            string sResult = string.Empty;

            StringBuilder sbFilteredQuery = new StringBuilder();

            if (PageSize <= 0)
                PageSize = MAX_RESULTS;

            int fromIndex = (PageIndex <= 0) ? 0 : PageSize * PageIndex;

            if(m_bIsRoot)
                sbFilteredQuery.Append("{");

            sbFilteredQuery.AppendFormat(" \"size\": {0}, ", PageSize);
            sbFilteredQuery.AppendFormat(" \"from\": {0}, ", fromIndex);

            if (ReturnFields.Count > 0)
            {
                sbFilteredQuery.Append("\"fields\": [");
                //add the sort elements to the return fields, if they are not there
                foreach (ESOrderObj orderObj in ESSort)
                {
                    if (orderObj.m_sOrderValue != string.Empty)
                    {
                        bool doContinue = true;
                        for (int i = 0; i < ReturnFields.Count && doContinue; i++)
                        {
                            if (ReturnFields[i].Contains(orderObj.m_sOrderValue))
                                doContinue = false;
                        }
                        if (doContinue)
                            ReturnFields.Add(string.Format("\"{0}\"", orderObj.m_sOrderValue));
                    }
                }

                sbFilteredQuery.Append(string.Join(",", ReturnFields));

                sbFilteredQuery.Append("], ");
            }

            string sSort = GetSort(ESSort, false);
            
            if (!string.IsNullOrEmpty(sSort))
                sbFilteredQuery.AppendFormat("{0}, ", sSort);

            sbFilteredQuery.Append("\"query\": { \"filtered\": {");

            List<string> lQueryFilter = new List<string>();
            if (Query != null)
            {
                string sQuery = Query.ToString();
                if (!string.IsNullOrEmpty(sQuery))
                {
                    lQueryFilter.Add(string.Format(" \"query\": {0}", sQuery));

                }
            }

            if (Filter != null)
            {
                string sFilter = Filter.ToString();
                if (!string.IsNullOrEmpty(sFilter))
                {
                    lQueryFilter.Add(sFilter);
                }
            }

            if (lQueryFilter.Count > 0)
            {
                sbFilteredQuery.Append(lQueryFilter.Aggregate((current, next) => current + "," + next));
            }

            sbFilteredQuery.Append("}}");

            if (m_bIsRoot)
                sbFilteredQuery.Append("}");

            sResult = sbFilteredQuery.ToString();


            return sResult;
        }

        private string GetSort(List<ESOrderObj> lOrderObj, bool bOrderByScore)
        {
            string sRes = string.Empty;
            if (lOrderObj != null && lOrderObj.Count > 0)
            {
                StringBuilder sSort = new StringBuilder();
                sSort.Append(" \"sort\": [{");

                foreach (ESOrderObj oOrderObj in lOrderObj)
                {
                    if (oOrderObj.m_sOrderValue != string.Empty)
                    {
                        sSort.AppendFormat(" \"{0}\": ", oOrderObj.m_sOrderValue.ToLower());
                        sSort.Append(" {");
                        sSort.AppendFormat("\"order\": \"{0}\"", oOrderObj.m_eOrderDir.ToString().ToLower());
                        sSort.Append("}");
                    }
                }
                sSort.Append("}");

                //we always add the score at the end of the sorting so that our records will be in best order when using wildcards in the query itself
                if (bOrderByScore)
                    sSort.Append(", \"_score\"");

                sSort.Append(" ]");

                sRes = sSort.ToString();
            }

            return sRes;
        }


        public static string GetESSortValue(OrderObj oOrderObj)
        {
            string sRes;
            if (oOrderObj.m_eOrderBy == OrderBy.META)
            {
                sRes = string.Concat("metas.", oOrderObj.m_sOrderValue.ToLower());

            }
            else if (oOrderObj.m_eOrderBy == OrderBy.ID)
            {
                sRes = "_id";
            }
            else if (oOrderObj.m_eOrderBy == OrderBy.RELATED)
            {
                sRes = "_score";
            }
            else
            {
                sRes = Enum.GetName(typeof(OrderBy), oOrderObj.m_eOrderBy).ToLower();
            }

            return sRes;
        }
    }
}
