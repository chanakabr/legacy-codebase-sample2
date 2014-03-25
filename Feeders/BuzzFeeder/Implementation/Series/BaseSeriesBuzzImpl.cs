using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ElasticSearch.Searcher;
using Newtonsoft.Json.Linq;
using Couchbase.Extensions;
using BuzzFeeder.BuzzCalculator;
using ApiObjects.SearchObjects;
using ElasticSearch.Common.SearchResults;

namespace BuzzFeeder.Implementation.Series
{
    public abstract class BaseSeriesBuzzImpl : BaseBuzzImpl
    {
        
        public BaseSeriesBuzzImpl(int nGroupID, DateTime dtPeriod, TimeSpan tsInterval, List<string> lActions, List<string> lMediaTypes)
            : base(nGroupID, dtPeriod, tsInterval, lActions, lMediaTypes)
        {
        }

        protected override void PreProcess()
        {
            Dictionary<string, BuzzCalculator.ItemsStats> dSeriesBucket = new Dictionary<string, ItemsStats>();
            List<Hits> lHits= GetSeries();

            if (lHits != null && lHits.Count > 0)
            {
                string seriesName, id;
                foreach (Hits hit in lHits)
                {
                    id = hit.Id;
                    hit.Fields.TryGetValue("series_name", out seriesName);
                    if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(seriesName))
                    {
                        if (!dSeriesBucket.ContainsKey(seriesName))
                            dSeriesBucket[seriesName] = new ItemsStats() { sMediaID = seriesName };

                        if (m_dCurrentBuzzCount.ContainsKey(id))
                        {
                            dSeriesBucket[seriesName] = ItemsStats.MergeItems(dSeriesBucket[seriesName], m_dCurrentBuzzCount[id]);
                        }
                    }
                }
            }

            foreach (string seriesName in dSeriesBucket.Keys)
            {
                if (m_dPreviousBuzzCount.ContainsKey(seriesName))
                {
                    dSeriesBucket[seriesName].nSampleCumulativeCount += m_dPreviousBuzzCount[seriesName].nSampleCumulativeCount;
                }
            }

            m_dCurrentBuzzCount = dSeriesBucket;
        }

        protected override void PostProcess()
        {
            Dictionary<string, BuzzCalculator.ItemsStats> dItemsStats = m_oBuzzCalc.m_dItemStats;

            try
            {
                Couchbase.CouchbaseClient client = CouchbaseManager.CouchbaseManager.GetInstance(CouchbaseManager.eCouchbaseBucket.DEFAULT);
                bool bRes = client.StoreJson(Enyim.Caching.Memcached.StoreMode.Set, GetGroupKey(), dItemsStats);
            }
            catch (Exception ex)
            {
            }
        }

        protected List<Hits> GetSeries()
        {
            FilteredQuery filteredQuery = new FilteredQuery() { PageIndex = 0, PageSize = 100000 };
            filteredQuery.ReturnFields.Clear();
            filteredQuery.ReturnFields.Add(ESMediaFields.TAGS_FILL.Fill("series name"));


            FilterCompositeType filter = new FilterCompositeType(ApiObjects.SearchObjects.CutWith.AND);
            ESTerm isActiveTerm = new ESTerm(true) { Key = ESMediaFields.IS_ACTIVE, Value = "1" };
            ESExists seriesNameExists = new ESExists() { Value = ESMediaFields.TAGS_FILL.Fill("series name") };

            ESTerms mediaTypeTerms = new ESTerms(true) { Key = ESMediaFields.MEDIA_TYPE_ID };
            mediaTypeTerms.Value.AddRange(m_lMediaTypes);

            ESRange startDateRange = new ESRange(false) { Key = ESMediaFields.START_DATE };
            startDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, m_dtTimePeriod.ToString("yyyyMMddHHmmss")));

            ESRange endDateRange = new ESRange(false) { Key = ESMediaFields.START_DATE };
            endDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GT, (m_dtTimePeriod - m_tsInterval).ToString("yyyyMMddHHmmss")));

            filter.AddChild(isActiveTerm);
            filter.AddChild(seriesNameExists);
            filter.AddChild(mediaTypeTerms);
            filter.AddChild(startDateRange);
            filter.AddChild(endDateRange);

            filteredQuery.Filter = new QueryFilter() { FilterSettings = filter };

            string sQueryJson = filteredQuery.ToString();

            string retval = m_oESApi.Search(m_nGroupID.ToString(), ESMediaFields.MEDIA, ref sQueryJson);

            ESSearchResult searchResult = new ESSearchResult(retval);

            return  searchResult.GetHits().Hits;
        }

        protected class SeriesESResult
        {
            public string sMediaID;
            public string sSerieName;
        }
    }
}
