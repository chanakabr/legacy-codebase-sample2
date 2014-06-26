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

        protected string m_sSeriesTagType;
        protected string[] m_lSeriesMediaTypeId;
        protected Dictionary<string, string> m_dGroupSeriesByName;

        public BaseSeriesBuzzImpl(int nGroupID, string lSeriesTagType, string[] lSeriesMediaTypeId, DateTime dtPeriod, TimeSpan tsInterval, int Weight, List<string> lActions, List<string> lAssetTypes, List<int> lFormulaWeights)
            : base(nGroupID, dtPeriod, tsInterval, Weight, lActions, lAssetTypes, lFormulaWeights)
        {
            m_sSeriesTagType = lSeriesTagType;
            m_lSeriesMediaTypeId = lSeriesMediaTypeId;
            m_dGroupSeriesByName = new Dictionary<string, string>();
        }

        protected override void PreProcess()
        {
            Dictionary<string, BuzzCalculator.ItemsStats> dSeriesBucket = new Dictionary<string, ItemsStats>();
            List<Hits> lSeriesHits = GetGroupSeries();

            if (lSeriesHits != null && lSeriesHits.Count > 0)
            {
                foreach (Hits hit in lSeriesHits)
                {
                    if (string.IsNullOrEmpty(hit.Id) && hit.Fields.ContainsKey("name"))
                    {
                        m_dGroupSeriesByName[hit.Fields["name"]] = hit.Id;
                    }
                }
            }

            List<Hits> lEpisodeHits= GetEpisodesSerieName();

            if (lEpisodeHits != null && lEpisodeHits.Count > 0)
            {
                string seriesName, id, seriesId;
                foreach (Hits hit in lEpisodeHits)
                {
                    id = hit.Id;
                    hit.Fields.TryGetValue(m_sSeriesTagType, out seriesName);
                    if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(seriesName))
                    {
                        if (m_dGroupSeriesByName.ContainsKey(seriesName))
                        {
                            seriesId = m_dGroupSeriesByName[seriesName];

                            if (!dSeriesBucket.ContainsKey(seriesId))
                                dSeriesBucket[seriesId] = new ItemsStats() { sMediaID = seriesId };

                            if (m_dCurrentBuzzCount.ContainsKey(id))
                            {
                                dSeriesBucket[seriesId] = ItemsStats.MergeItems(dSeriesBucket[seriesId], m_dCurrentBuzzCount[id]);
                            }
                        }
                        
                    }
                }
            }

            foreach (string seriesMediaId in dSeriesBucket.Keys)
            {
                if (m_dPreviousBuzzCount.ContainsKey(seriesMediaId))
                {
                    dSeriesBucket[seriesMediaId].nSampleCumulativeCount += m_dPreviousBuzzCount[seriesMediaId].nSampleCumulativeCount;
                }
            }

            m_dCurrentBuzzCount = dSeriesBucket;
        }

        protected override void PostProcess()
        {
            Dictionary<string, BuzzCalculator.ItemsStats> dItemsStats = m_oBuzzCalc.m_dItemStats;

            try
            {
                Couchbase.CouchbaseClient client = CouchbaseManager.CouchbaseManager.GetInstance(CouchbaseManager.eCouchbaseBucket.STATISTICS);
                bool bRes = client.StoreJson(Enyim.Caching.Memcached.StoreMode.Set, GetGroupKey(), dItemsStats);
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Error", string.Format("caught error when storing item stats in couchbase. ex={0};stack={1}",ex.Message, ex.StackTrace), "BuzzFeeder");
            }
        }

        protected List<Hits> GetGroupSeries()
        {
            FilteredQuery filteredQuery = new FilteredQuery() { PageIndex = 0, PageSize = 100000 };
            filteredQuery.ReturnFields.Clear();
            filteredQuery.ReturnFields.Add(ESMediaFields.TAGS_FILL.Fill("name"));


            FilterCompositeType filter = new FilterCompositeType(ApiObjects.SearchObjects.CutWith.AND);
            ESTerm isActiveTerm = new ESTerm(true) { Key = ESMediaFields.IS_ACTIVE, Value = "1" };

            ESTerms mediaTypeTerms = new ESTerms(true) { Key = ESMediaFields.MEDIA_TYPE_ID };
            mediaTypeTerms.Value.AddRange(m_lSeriesMediaTypeId);

            ESRange startDateRange = new ESRange(false) { Key = ESMediaFields.START_DATE };
            startDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, m_dtTimePeriod.ToString("yyyyMMddHHmmss")));

            ESRange endDateRange = new ESRange(false) { Key = ESMediaFields.START_DATE };
            endDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GT, (m_dtTimePeriod - m_tsInterval).ToString("yyyyMMddHHmmss")));

            filter.AddChild(isActiveTerm);
            filter.AddChild(mediaTypeTerms);
            filter.AddChild(startDateRange);
            filter.AddChild(endDateRange);

            filteredQuery.Filter = new QueryFilter() { FilterSettings = filter };

            string sQueryJson = filteredQuery.ToString();

            string retval = m_oESApi.Search(m_nGroupID.ToString(), ESMediaFields.MEDIA, ref sQueryJson);

            ESSearchResult searchResult = new ESSearchResult(retval);

            return searchResult.GetHits().Hits;
        }

        protected List<Hits> GetEpisodesSerieName()
        {
            FilteredQuery filteredQuery = new FilteredQuery() { PageIndex = 0, PageSize = 100000 };
            filteredQuery.ReturnFields.Clear();
            filteredQuery.ReturnFields.Add(ESMediaFields.TAGS_FILL.Fill(m_sSeriesTagType));


            FilterCompositeType filter = new FilterCompositeType(ApiObjects.SearchObjects.CutWith.AND);
            ESTerm isActiveTerm = new ESTerm(true) { Key = ESMediaFields.IS_ACTIVE, Value = "1" };
            ESExists seriesNameExists = new ESExists() { Value = ESMediaFields.TAGS_FILL.Fill(m_sSeriesTagType) };

            ESTerms mediaTypeTerms = new ESTerms(true) { Key = ESMediaFields.MEDIA_TYPE_ID };
            mediaTypeTerms.Value.AddRange(m_lAssetTypes);

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
