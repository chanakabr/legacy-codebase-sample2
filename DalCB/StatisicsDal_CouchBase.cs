using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects.Statistics;
using Couchbase;
using Couchbase.Extensions;
using CouchbaseManager;
using Logger;

namespace DalCB
{
    public class StatisicsDal_CouchBase
    {        
        private static readonly string CB_STATISTICS_DESGIN = Utils.GetValFromConfig("cb_statistics_design");
        
        CouchbaseClient m_oClient;
        
        private int m_nGroupID;

        public StatisicsDal_CouchBase(int nGroupID)
        {
            m_nGroupID = nGroupID;
            m_oClient = CouchbaseManager.CouchbaseManager.GetInstance(eCouchbaseBucket.STATISTICS);
        }

        public BuzzWeightedAverScore GetBuzzAverScore(string sKey)
        {
            BuzzWeightedAverScore oRes = null;
            try
            {
                oRes = m_oClient.GetJson<BuzzWeightedAverScore>(sKey);
            }
            catch (Exception ex)
            {
                Logger.BaseLog log = new Logger.BaseLog(eLogType.CodeLog, DateTime.UtcNow, true);
                log.Message = string.Format("GetBuzzAverScore: ex={0} in {1}", ex.Message, ex.StackTrace);
                log.Error(log.Message, false);
            }

            return oRes;
        }
    }
}
