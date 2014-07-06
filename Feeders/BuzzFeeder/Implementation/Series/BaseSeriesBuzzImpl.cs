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
        protected string[] m_lEpisodeMediaTypeId;
        protected Dictionary<string, string> m_dGroupSeriesByName;

        public BaseSeriesBuzzImpl(int nGroupID, string lSeriesTagType, string[] lSeriesMediaTypeId, DateTime dtPeriod, TimeSpan tsInterval, int Weight, List<string> lActions, List<string> lAssetTypes, List<int> lFormulaWeights)
            : base(nGroupID, dtPeriod, tsInterval, Weight, lActions, lAssetTypes, lFormulaWeights)
        {
            m_sSeriesTagType = lSeriesTagType;
            m_lEpisodeMediaTypeId = lSeriesMediaTypeId;
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
                    if (!string.IsNullOrEmpty(hit.Id) && hit.Fields.ContainsKey("name"))
                    {
                        m_dGroupSeriesByName[hit.Fields["name"]] = hit.Id;
                    }
                }
            }


            List<SeriesESResult> lEpisodeHits = GetEpisodesSerieName();

            if (lEpisodeHits != null && lEpisodeHits.Count > 0)
            {
                string id, seriesId;
                foreach (SeriesESResult hit in lEpisodeHits)
                {
                    id = hit.sMediaID;
                    if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(hit.sSerieName))
                    {
                        if (m_dGroupSeriesByName.ContainsKey(hit.sSerieName))
                        {
                            seriesId = m_dGroupSeriesByName[hit.sSerieName];

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

            //get previous sample count
            m_dPreviousBuzzCount = GetPreviousSample(dSeriesBucket.Keys.ToList());

            foreach (string seriesMediaId in dSeriesBucket.Keys)
            {
                if (m_dPreviousBuzzCount.ContainsKey(seriesMediaId))
                {
                    dSeriesBucket[seriesMediaId].nSampleCumulativeCount += m_dPreviousBuzzCount[seriesMediaId].nSampleCumulativeCount;
                }
            }

            m_dCurrentBuzzCount = dSeriesBucket;
        }

        protected override Dictionary<string, ItemsStats> GetCurrentSample()
        {
            Dictionary<string, ItemsStats> dCurSample = new Dictionary<string, ItemsStats>();

            DateTime t2 = m_dtTimePeriod;
            DateTime t1 = m_dtTimePeriod - m_tsInterval;

            #region create ES query to retrive all item counts
            #region create facets
            ESTermsFacet.ESTermsFacetItem curPeriodicalGrowthFacet = CreateItemPeriodicalGrowthFacet("cur_items_periodical_count", t1, t2);
            #endregion

            ESTermsFacet groupedFacet = new ESTermsFacet();
            groupedFacet.AddTermFacet(curPeriodicalGrowthFacet);

            groupedFacet.Query = new FilteredQuery(false) { PageIndex = 0, PageSize = 0 };
            groupedFacet.Query.Filter = new QueryFilter();

            BaseFilterCompositeType filterParent = new FilterCompositeType(CutWith.AND);

            ESTerm groupIdTerm = new ESTerm(true) { Key = "group_id", Value = m_nGroupID.ToString() };

            ESTerms actions = new ESTerms(false) { Key = "action" };
            actions.Value.AddRange(m_lActions);

            ESTerms assetTypes = new ESTerms(false) { Key = "media_type" };
            assetTypes.Value.AddRange(m_lEpisodeMediaTypeId);

            filterParent.AddChild(groupIdTerm);
            filterParent.AddChild(actions);
            filterParent.AddChild(assetTypes);

            groupedFacet.Query.Filter.FilterSettings = filterParent;

            string facetJSON = groupedFacet.ToString();
            #endregion

            string retval = m_oESApi.Search(ElasticSearch.Common.Utils.GetGroupStatisticsIndex(m_nGroupID), ElasticSearch.Common.Utils.ES_STATS_TYPE, ref facetJSON);

            Dictionary<string, Dictionary<string, int>> dFacetResults = ESTermsFacet.FacetResults(ref retval);

            #region fill buzz count dictionary
            if (dFacetResults != null && dFacetResults.Count > 0)
            {
                #region get current sample item periodical count
                if (dFacetResults.ContainsKey("cur_items_periodical_count"))
                {
                    int nCount;
                    foreach (string itemID in dFacetResults["cur_items_periodical_count"].Keys)
                    {
                        nCount = dFacetResults["cur_items_periodical_count"][itemID];
                        dCurSample[itemID] = new ItemsStats() { nSampleCount = nCount, nSampleCumulativeCount = nCount, sMediaID = itemID };
                    }
                    dFacetResults["cur_item_periodical_growth"] = null;
                }
                #endregion
            }
            #endregion

            return dCurSample;
        }

        protected override void PostProcess()
        {
            Dictionary<string, BuzzCalculator.ItemsStats> dItemsStats = m_oBuzzCalc.m_dItemStats;

            try
            {
                Couchbase.CouchbaseClient client = CouchbaseManager.CouchbaseManager.GetInstance(CouchbaseManager.eCouchbaseBucket.STATISTICS);
                string groupKey = GetGroupKey();
                foreach (string episodeId in dItemsStats.Keys)
                {
                    bool bRes = client.StoreJson(Enyim.Caching.Memcached.StoreMode.Set, string.Concat(groupKey, "_", episodeId), dItemsStats[episodeId]);
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Error", string.Format("caught error when storing item stats in couchbase. ex={0};stack={1}", ex.Message, ex.StackTrace), "BuzzFeeder");
            }
        }

        protected List<Hits> GetGroupSeries()
        {
            FilteredQuery filteredQuery = new FilteredQuery() { PageIndex = 0, PageSize = 100000 };
            filteredQuery.ReturnFields.Clear();
            filteredQuery.ReturnFields.Add(string.Format("\"{0}\"", ESMediaFields.NAME));


            FilterCompositeType filter = new FilterCompositeType(ApiObjects.SearchObjects.CutWith.AND);
            ESTerm isActiveTerm = new ESTerm(true) { Key = ESMediaFields.IS_ACTIVE, Value = "1" };

            ESTerms mediaTypeTerms = new ESTerms(true) { Key = ESMediaFields.MEDIA_TYPE_ID };
            mediaTypeTerms.Value.AddRange(m_lAssetTypes);

            ESRange startDateRange = new ESRange(false) { Key = ESMediaFields.START_DATE };
            startDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LT, m_dtTimePeriod.ToString("yyyyMMddHHmmss")));

            ESRange endDateRange = new ESRange(false) { Key = ESMediaFields.END_DATE };
            endDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, (m_dtTimePeriod - m_tsInterval).ToString("yyyyMMddHHmmss")));

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

        protected List<SeriesESResult> GetEpisodesSerieName()
        {
            FilteredQuery filteredQuery = new FilteredQuery() { PageIndex = 0, PageSize = 100000 };
            filteredQuery.ReturnFields.Clear();
            filteredQuery.ReturnFields.Add(string.Format("\"{0}\"", ESMediaFields.TAGS_FILL.Fill(m_sSeriesTagType)));


            FilterCompositeType filter = new FilterCompositeType(ApiObjects.SearchObjects.CutWith.AND);
            ESTerm isActiveTerm = new ESTerm(true) { Key = ESMediaFields.IS_ACTIVE, Value = "1" };
            ESExists seriesNameExists = new ESExists() { Value = ESMediaFields.TAGS_FILL.Fill(m_sSeriesTagType) };

            ESTerms mediaTypeTerms = new ESTerms(true) { Key = ESMediaFields.MEDIA_TYPE_ID };
            mediaTypeTerms.Value.AddRange(m_lEpisodeMediaTypeId);

            ESRange startDateRange = new ESRange(false) { Key = ESMediaFields.START_DATE };
            startDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LT, m_dtTimePeriod.ToString("yyyyMMddHHmmss")));

            ESRange endDateRange = new ESRange(false) { Key = ESMediaFields.END_DATE };
            endDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, (m_dtTimePeriod - m_tsInterval).ToString("yyyyMMddHHmmss")));

            filter.AddChild(isActiveTerm);
            filter.AddChild(seriesNameExists);
            filter.AddChild(mediaTypeTerms);
            filter.AddChild(startDateRange);
            filter.AddChild(endDateRange);

            filteredQuery.Filter = new QueryFilter() { FilterSettings = filter };

            string sQueryJson = filteredQuery.ToString();

            string retval = m_oESApi.Search(m_nGroupID.ToString(), ESMediaFields.MEDIA, ref sQueryJson);

            List<SeriesESResult> lSeriesResult = new List<SeriesESResult>();
            try
            {
                var jsonObj = JObject.Parse(retval);

                if (jsonObj != null)
                {
                    JToken tempToken;
                    int totalItems = ((tempToken = jsonObj.SelectToken("hits.total")) == null ? 0 : (int)tempToken);
                    if (totalItems > 0)
                    {
                        var res = jsonObj.SelectToken("hits.hits").Select(item => new SeriesESResult()
                        {
                            
                            sMediaID = (tempToken = item.SelectToken(ESMediaFields.ID)) == null ? "" : (string)tempToken,
                            sSerieName = (tempToken = item["fields"]["tags.series name"]) == null ? "" : tempToken.Select(t => (string)t).First()
                        });

                        lSeriesResult.AddRange(res);
                    }
                }
            }
            catch
            {
            }

            return lSeriesResult;
        }

        protected class SeriesESResult
        {
            public string sMediaID;
            public string sSerieName;
        }
    }
}
