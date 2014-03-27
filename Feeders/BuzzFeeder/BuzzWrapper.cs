using ApiObjects.SearchObjects;
using BuzzFeeder.BuzzCalculator;
using ElasticSearch.Searcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Extensions;
using ApiObjects.Statistics;

namespace BuzzFeeder
{
    public class BuzzWrapper
    {

        protected Dictionary<string, BuzzActivity> m_dActivities;

        //take all info from config file/DB !!!!!!!!!!!!!!!!!!
        private int nSBIMax = 99;
        private int nSBIMin = 50;


        private List<string> m_lAssetTypes;
        private int m_nGroupID;

        public BuzzWrapper(int nGroupID, List<string> lAssetTypes)
        {
            m_dActivities = new Dictionary<string, BuzzActivity>();
            m_lAssetTypes = lAssetTypes;
            m_nGroupID = nGroupID;
        }

        public void AddActivity(eBuzzActivityTypes eActivityType, BuzzActivity activity)
        {
            if (activity != null)
            {
                m_dActivities[eActivityType.ToString()] = activity;
            }
        }

        public void CalculateBuzz()
        {
            #region calculate buzz
            Task[] buzzTasks = new Task[m_dActivities.Count];
            BuzzActivity[] activities = m_dActivities.Values.ToArray();

            int nViewsSum = 0;
            for (int i = 0; i < activities.Length; i++)
            {
                nViewsSum += activities[i].Weight;

                buzzTasks[i] = Task.Factory.StartNew(
                    (index) =>
                    {
                        if (activities[(int)index].BuzzImpl != null)
                            activities[(int)index].BuzzImpl.CalcBuzz();

                    }, i);
            }

            Task.WaitAll(buzzTasks);
            #endregion

            List<string> activeMedias = GetActiveMedias();
            Dictionary<string, double> dSBI = new Dictionary<string, double>();
            double nMinSum = double.MaxValue;
            double nMaxSum = 0;

            #region calculate weighted average score
            foreach (string mediaID in activeMedias)
            {
                double itemSum = 0.0;
                foreach (var activity in activities)
                {
                    var item = activity.BuzzImpl.GetItem(mediaID);

                    if(item != null)
                        itemSum += item.nSampleCount * activity.Weight;
                }

                itemSum /= nViewsSum;

                dSBI[mediaID] = itemSum;

                if (itemSum > nMaxSum)
                    nMaxSum = itemSum;
                if (itemSum < nMinSum)
                    nMinSum = itemSum;
            }
            #endregion

            //calculate normalization factor
            double factor = (nSBIMax - nSBIMin) / (nMaxSum - nMinSum);

            Couchbase.CouchbaseClient cbClient = CouchbaseManager.CouchbaseManager.GetInstance(CouchbaseManager.eCouchbaseBucket.DEFAULT);

            #region Calculate normalized weighted average score and update CB
            BuzzWeightedAverScore score = new BuzzWeightedAverScore() { UpdateDate = DateTime.UtcNow };
            bool bSuccess;
            string keyPrefix = "was_";
            string key;
            foreach (string mediaID in dSBI.Keys)
            {
                score.WeightedAverageScore = dSBI[mediaID];
                score.NormalizedWeightedAverageScore = nSBIMin + ((score.WeightedAverageScore - nMinSum) * factor);
                key = string.Concat(keyPrefix, mediaID);
                bSuccess = cbClient.StoreJson(Enyim.Caching.Memcached.StoreMode.Set, key, score);
            }
            #endregion
        }

        private List<string> GetActiveMedias()
        {
            ElasticSearch.Common.ElasticSearchApi oESApi = new ElasticSearch.Common.ElasticSearchApi();
            FilteredQuery filteredQuery = new FilteredQuery() { PageSize = 100000 };
            filteredQuery.Filter = new QueryFilter();


            FilterCompositeType filter = new FilterCompositeType(ApiObjects.SearchObjects.CutWith.AND);
            ESTerm groupTerm = new ESTerm(true) { Key = ESMediaFields.GROUP_ID, Value = m_nGroupID.ToString() };
            ESTerm isActiveTerm = new ESTerm(true) { Key = ESMediaFields.IS_ACTIVE, Value = "1" };
            ESTerms mediaTypeTerms = new ESTerms(true) { Key = ESMediaFields.MEDIA_TYPE_ID };
            mediaTypeTerms.Value.AddRange(m_lAssetTypes);

            filter.AddChild(isActiveTerm);
            filter.AddChild(mediaTypeTerms);

            filteredQuery.Filter.FilterSettings = filter;

            string json = filteredQuery.ToString();

            string retval = oESApi.Search(m_nGroupID.ToString(), ESMediaFields.MEDIA, ref json);

            ElasticSearch.Common.SearchResults.ESSearchResult results = new ElasticSearch.Common.SearchResults.ESSearchResult(retval);

            return results.GetHitIds();

        }
    }



    public enum eBuzzActivityTypes { VIEWS, LIKES, COMMENTS, FOLLOWS};

    public class BuzzActivity
    {
        public int Weight;
        public BaseBuzzImpl BuzzImpl;
        public BuzzActivity()
        {
        }
    }
}
