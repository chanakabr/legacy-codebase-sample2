using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ElasticSearch;
using ElasticSearch.Searcher;
using ApiObjects.SearchObjects;
using BuzzFeeder.BuzzCalculator;
using Couchbase.Extensions;

namespace BuzzFeeder
{
    public abstract class BaseBuzzImpl
    {
        
        #region members
        protected int m_nGroupID;
        protected DateTime m_dtTimePeriod;
        protected TimeSpan m_tsInterval;
        protected List<string> m_lAssetTypes;
        protected List<string> m_lActions;
        public int Weight { get; protected set; }
        protected Dictionary<string, ItemsStats> m_dCurrentBuzzCount;
        protected Dictionary<string, ItemsStats> m_dPreviousBuzzCount;
        protected ElasticSearch.Common.ElasticSearchApi m_oESApi;
        protected BuzzCalculator.BuzzCalculator m_oBuzzCalc;
        #endregion

        public BaseBuzzImpl(int nGroupID, DateTime dtPeriod, TimeSpan tsInterval, int nWeight, List<string> lActions, List<string> lAssetTypes, List<int> lFormulaWeights)
        {
            m_nGroupID = nGroupID;
            m_dtTimePeriod = dtPeriod;
            m_tsInterval = tsInterval;
            m_lAssetTypes = lAssetTypes;
            m_lActions = lActions;
            Weight = nWeight;
            m_dCurrentBuzzCount = new Dictionary<string, ItemsStats>();
            m_dPreviousBuzzCount = new Dictionary<string, ItemsStats>();
            m_oBuzzCalc = new BuzzCalculator.BuzzCalculator(lFormulaWeights);
            m_oESApi = new ElasticSearch.Common.ElasticSearchApi();
        }



        public virtual void CalcBuzz()
        {
            m_dCurrentBuzzCount = GetCurrentSample();
            
            PreProcess();
            
            m_oBuzzCalc.m_dCurBuzzCount = m_dCurrentBuzzCount;
            m_oBuzzCalc.m_dPrevBuzzCount = m_dPreviousBuzzCount;
            m_oBuzzCalc.CalcBuzz();

            PostProcess();
        }

        protected abstract void PreProcess();
        protected abstract void PostProcess();

        //returns group key that is to be used to retrieve/store group media info from couchbase
        protected abstract string GetGroupKey();

        public ItemsStats GetItem(string id)
        {
            ItemsStats res = null;

            try
            {
                Couchbase.CouchbaseClient client = CouchbaseManager.CouchbaseManager.GetInstance(CouchbaseManager.eCouchbaseBucket.STATISTICS);

                string key = string.Concat(GetGroupKey(), "_", id);

                string json = client.Get<string>(key);
                if (!string.IsNullOrEmpty(json))
                {
                    res = Newtonsoft.Json.JsonConvert.DeserializeObject<ItemsStats>(json);
                }                
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Error", string.Format("caught error when trying to get group items from cb. ex={0};stack={1}", ex.Message, ex.StackTrace), "BuzzFeeder");
            }

            return res;
        }
        public List<ItemsStats> GetItems(List<string> ids)
        {
            List<ItemsStats> res = new List<ItemsStats>();

            try
            {
                Couchbase.CouchbaseClient client = CouchbaseManager.CouchbaseManager.GetInstance(CouchbaseManager.eCouchbaseBucket.STATISTICS);
                string json = client.Get<string>(GetGroupKey());

                var retval = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, ItemsStats>>(json);

                if (retval != null && retval.Count > 0)
                {
                    ItemsStats tempItem;
                    foreach (string id in ids)
                    {
                        if (retval.TryGetValue(id, out tempItem))
                        {
                            res.Add(tempItem);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Error", string.Format("caught error when trying to get group items from cb. ex={0};stack={1}", ex.Message, ex.StackTrace), "BuzzFeeder");
            }

            return res;
        }

        protected virtual Dictionary<string, ItemsStats> GetCurrentSample()
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
            assetTypes.Value.AddRange(m_lAssetTypes);

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

        protected virtual Dictionary<string, ItemsStats> GetPreviousSample(List<string> lMediaIDs)
        {
            Dictionary<string, ItemsStats> dRes = new Dictionary<string, ItemsStats>();

            try
            {
                Couchbase.CouchbaseClient client = CouchbaseManager.CouchbaseManager.GetInstance(CouchbaseManager.eCouchbaseBucket.STATISTICS);
                string groupKey = GetGroupKey();
                List<string> lKeys = new List<string>();

                foreach (var mediaID in lMediaIDs)
                {
                    lKeys.Add(string.Concat(groupKey, "_", mediaID));
                }

                var retval = client.Get(lKeys);

                lKeys = null;

                if (retval != null)
                {
                    ItemsStats tempItem;
                    foreach (string key in retval.Keys)
                    {
                        try
                        {
                            tempItem = Newtonsoft.Json.JsonConvert.DeserializeObject<ItemsStats>(retval[key] as string);
                            if (tempItem != null)
                            {
                                dRes[tempItem.sMediaID] = tempItem;
                            }
                        }
                        catch { }
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Error", string.Format("caught error when trying to get group previous sample from cb. ex={0};stack={1}", ex.Message, ex.StackTrace), "BuzzFeeder");
            }

            return dRes;
        }

        protected ESTermsFacet.ESTermsFacetItem CreateGroupPeriodicalGrowthFacet(string sFacetName, DateTime startTime, DateTime endTime) //calculate total views in sample group. that is all media hits in past 5 minutes
        {
            ESTermsFacet.ESTermsFacetItem facet = new ESTermsFacet.ESTermsFacetItem() { FacetName = sFacetName, Field = "group_id", Size = 100000 };

            #region define filters
            BaseFilterCompositeType facetFilter = new FilterCompositeType(CutWith.AND);

            //facets caluclate on data based on these times
            ESRange dateRange = new ESRange(false);
            dateRange.Key = "action_date";
            dateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GT, startTime.ToString("yyyyMMddHHmmss")));
            dateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, endTime.ToString("yyyyMMddHHmmss")));

            facetFilter.AddChild(dateRange);
            #endregion

            facet.FacetFilter = facetFilter;
            return facet;
        }

        protected ESTermsFacet.ESTermsFacetItem CreateItemPeriodicalGrowthFacet(string sFacetName, DateTime startTime, DateTime endTime)
        {

            ESTermsFacet.ESTermsFacetItem facet = new ESTermsFacet.ESTermsFacetItem() { FacetName = sFacetName, Field = "media_id", Size = 100000 };

            #region define filters
            BaseFilterCompositeType facetFilter = new FilterCompositeType(CutWith.AND);
            //facets caluclate on data based on these times
            ESRange dateRange = new ESRange(false);
            dateRange.Key = "action_date";
            dateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GT, startTime.ToString("yyyyMMddHHmmss")));
            dateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, endTime.ToString("yyyyMMddHHmmss")));

            facetFilter.AddChild(dateRange);
            #endregion

            facet.FacetFilter = facetFilter;

            return facet;
        }
    }
}