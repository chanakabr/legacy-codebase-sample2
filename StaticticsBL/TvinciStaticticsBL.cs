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
    }
}
