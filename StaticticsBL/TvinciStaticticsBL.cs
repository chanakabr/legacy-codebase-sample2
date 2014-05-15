using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects.Statistics;
using DalCB;

namespace StatisticsBL
{
    public class TvinciStaticticsBL: BaseStaticticsBL
    {
        protected StatisicsDal_CouchBase m_oMediaCouchbase;

        public TvinciStaticticsBL(int nGroupID)
        {
            this.m_nGroupID = nGroupID;
            m_oMediaCouchbase = new DalCB.StatisicsDal_CouchBase(m_nGroupID);
        }

        public override BuzzWeightedAverScore GetBuzzAverScore(string sKey)
        {
            string sFullKey = string.Concat("was_", sKey);
            BuzzWeightedAverScore oRes = m_oMediaCouchbase.GetBuzzAverScore(sFullKey);
            oRes = (oRes != null) ? oRes : null;
            return oRes;
        }

        public override Dictionary<string, BuzzWeightedAverScore> GetBuzzAverScore(List<int> lKey)
        {
            List<string> lFullKey = new List<string>(); //string.Concat("was_", sKey);
            foreach (int key in lKey)
            {
                lFullKey.Add(string.Concat("was_", key.ToString()));
            }
            Dictionary<string , BuzzWeightedAverScore> otempRes = m_oMediaCouchbase.GetBuzzAverScore(lFullKey);
            Dictionary<string, BuzzWeightedAverScore> oRes = null;
            foreach (KeyValuePair<string , BuzzWeightedAverScore> BM in otempRes)
            {
                if (oRes == null)
                {
                    oRes = new Dictionary<string, BuzzWeightedAverScore>();
                }
                string key = BM.Key.Replace("was_","");
                oRes.Add(key, BM.Value);
            }


            oRes = (oRes != null) ? oRes : null;
            return oRes;
        }
    }
}
