using BuzzFeeder.BuzzCalculator;
using ElasticSearch.Searcher;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Couchbase.Extensions;
using ApiObjects.SearchObjects;

namespace BuzzFeeder.Implementation.Channels
{
    public abstract class BaseChannelBuzzImpl : BaseBuzzImpl
    {
        public BaseChannelBuzzImpl(int nGroupID, DateTime dtPeriod, TimeSpan dtInterval, int Weight, List<string> lActions, List<string> lAssetTypes, List<int> lFormulaWeights)
            : base(nGroupID, dtPeriod, dtInterval, Weight, lActions, lAssetTypes, lFormulaWeights)
        {

        }

        protected override void PreProcess()
        {
            m_dPreviousBuzzCount = GetPreviousSample(m_dCurrentBuzzCount.Keys.ToList());

            #region Get medias that were not watched in current sample, but are part of total count
            List<string> lMediaIDs = GetMediaIdsByMediaType();

            foreach (string mediaID in lMediaIDs)
            {
                if (!m_dCurrentBuzzCount.ContainsKey(mediaID))
                {
                    m_dCurrentBuzzCount[mediaID] = new ItemsStats() { sMediaID = mediaID };
                }
            }
            #endregion

            #region calculate cumulative count by adding previous cumulative to current cumulative

            foreach (string sMediaID in m_dCurrentBuzzCount.Keys)
            {
                if (m_dPreviousBuzzCount.ContainsKey(sMediaID))
                {
                    m_dCurrentBuzzCount[sMediaID].nSampleCumulativeCount += m_dPreviousBuzzCount[sMediaID].nSampleCumulativeCount;
                }
            }

            #endregion

        }

        protected List<string> GetMediaIdsByMediaType()
        {
            FilteredQuery filteredQuery = new FilteredQuery() { PageIndex = 0, PageSize = 100000 };

            FilterCompositeType filter = new FilterCompositeType(ApiObjects.SearchObjects.CutWith.AND);
            ESTerm isActiveTerm = new ESTerm(true) { Key = ESMediaFields.IS_ACTIVE, Value = "1" };

            ESTerms mediaTypeTerms = new ESTerms(true) { Key = ESMediaFields.MEDIA_TYPE_ID };
            mediaTypeTerms.Value.AddRange(m_lAssetTypes);

            ESRange startDateRange = new ESRange(false) { Key = ESMediaFields.START_DATE};
            startDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, m_dtTimePeriod.ToString(ESMediaFields.DATE_FORMAT)));

            ESRange endDateRange = new ESRange(false) { Key = ESMediaFields.START_DATE };
            endDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GT, (m_dtTimePeriod - m_tsInterval).ToString(ESMediaFields.DATE_FORMAT)));

            filter.AddChild(isActiveTerm);
            filter.AddChild(mediaTypeTerms);
            filter.AddChild(startDateRange);
            filter.AddChild(endDateRange);

            filteredQuery.Filter = new QueryFilter() { FilterSettings = filter };

            string sJsonQuery = filteredQuery.ToString();

            string retval = m_oESApi.Search(m_nGroupID.ToString(), "media", ref sJsonQuery);

            ElasticSearch.Common.SearchResults.ESSearchResult results = new ElasticSearch.Common.SearchResults.ESSearchResult(retval);

            return results.GetHitIds();
        }

        protected override void PostProcess()
        {
            Dictionary<string, BuzzCalculator.ItemsStats> dItemsStats = m_oBuzzCalc.m_dItemStats;

            try
            {
                Couchbase.CouchbaseClient client = CouchbaseManager.CouchbaseManager.GetInstance(CouchbaseManager.eCouchbaseBucket.STATISTICS);

                string groupKey = GetGroupKey();
                string key;
                foreach (string itemID in dItemsStats.Keys)
                {
                    key = string.Concat(groupKey, "_", itemID);
                    bool bRes = client.StoreJson(Enyim.Caching.Memcached.StoreMode.Set, key, dItemsStats[itemID]);
                }
                
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Error", string.Format("caught error when storing item stats in couchbase. ex={0};stack={1}", ex.Message, ex.StackTrace), "BuzzFeeder");
            }
        }
    }
}
