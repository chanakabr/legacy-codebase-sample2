using ApiObjects.SearchObjects;
using ApiObjects.Statistics;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KLogMonitor;
using System.Reflection;

namespace ElasticSearch.Searcher
{
    public class ESStatisticsQueryBuilder
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static readonly string AND_CONDITION = "AND";
        public static readonly string OR_CONDITION = "OR";

        protected readonly int MAX_RESULTS;

        public StatisticsActionSearchObj oSearchObject { get; set; }

        public int m_nGroupID { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public List<string> ReturnFields { get; protected set; }
        public eQueryType QueryType { get; set; }

        public ESStatisticsQueryBuilder()
            : this(0, null)
        {
        }

        public ESStatisticsQueryBuilder(int nGroupID, StatisticsActionSearchObj searchObject)
        {
            oSearchObject = searchObject;
            m_nGroupID = nGroupID;
            ReturnFields = new List<string>() { "\"_id\"", "\"_index\"", "\"_type\"", "\"_score\"", "\"group_id\"", "\"media_id\"", "\"action\"" };

            string sMaxResults = Common.Utils.GetWSURL("MAX_RESULTS");

            if (!int.TryParse(sMaxResults, out MAX_RESULTS))
                MAX_RESULTS = 10000;
        }

        public string BuildQuery()
        {
            if (oSearchObject == null)
                return string.Empty;

            FilteredQuery filteredQuery = new FilteredQuery();
            QueryFilter queryFilter = new QueryFilter();

            BaseFilterCompositeType filter = new FilterCompositeType(CutWith.AND);
            ESTerm mediaIdTerm = new ESTerm(true);
            mediaIdTerm.Key = "media_id";
            mediaIdTerm.Value = oSearchObject.MediaID.ToString();
            filter.AddChild(mediaIdTerm);

            ESTerm mediaTypeTerm = new ESTerm(false);
            mediaTypeTerm.Key = "media_type";
            mediaTypeTerm.Value = oSearchObject.MediaType;
            filter.AddChild(mediaTypeTerm);

            ESTerm DateTerm = new ESTerm(false);
            DateTerm.Key = "action_date";
            DateTerm.Value = oSearchObject.Date.ToString("yyyyMMddHHmmss");
            filter.AddChild(DateTerm);

            ESTerm ActionTerm = new ESTerm(false);
            ActionTerm.Key = "action";
            ActionTerm.Value = oSearchObject.Action;
            filter.AddChild(ActionTerm);

            queryFilter.FilterSettings = filter;
            filteredQuery.Filter = queryFilter;

            filteredQuery.Query = new ESMatchAllQuery();

            return filteredQuery.ToString();
        }



        private List<StatisticsView> DecodeSearchJsonObject(string sObj, ref int totalItems)
        {
            List<StatisticsView> documents = null;
            try
            {
                var jsonObj = JObject.Parse(sObj);

                if (jsonObj != null)
                {
                    JToken tempToken;
                    totalItems = ((tempToken = jsonObj.SelectToken("hits.total")) == null ? 0 : (int)tempToken);
                    if (totalItems > 0)
                    {
                        documents = jsonObj.SelectToken("hits.hits").Select(item => new StatisticsView()
                        {

                            ID = ((tempToken = item.SelectToken("_id")) == null ? string.Empty : (string)tempToken),
                            GroupID = ((tempToken = item.SelectToken("fields.group_id")) == null ? 0 : (int)tempToken),
                            MediaID = ((tempToken = item.SelectToken("fields.media_id")) == null ? 0 : (int)tempToken)
                        }).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Json Deserialization failed for ElasticSearch Media request. Execption={0}", ex.Message), ex);
            }

            return documents;
        }
    }
}
