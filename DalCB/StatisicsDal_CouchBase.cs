using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects.Statistics;
using CouchbaseManager;
using Logger;
using Newtonsoft.Json;

namespace DalCB
{
    public class StatisicsDal_CouchBase
    {        
        private static readonly string CB_STATISTICS_DESGIN = Utils.GetValFromConfig("cb_statistics_design");

        CouchbaseManager.CouchbaseManager cbManager;

        private int m_nGroupID;

        public StatisicsDal_CouchBase(int nGroupID)
        {
            m_nGroupID = nGroupID;
            cbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.STATISTICS);
        }

        public Dictionary<string, BuzzWeightedAverScore> GetBuzzAverScore(List<string> lKey)
        {
            Dictionary<string, BuzzWeightedAverScore> oRes = new Dictionary<string, BuzzWeightedAverScore>();
            try
            {
                if (lKey != null && lKey.Count > 0)
                {
                    IDictionary<string, object> dItems = cbManager.GetValues<object>(lKey, true);

                    if (dItems != null && dItems.Count > 0)
                    {
                        BuzzWeightedAverScore tempBM;
                        foreach (KeyValuePair<string, object> item in dItems)
                        {
                            if (item.Value != null && !string.IsNullOrEmpty(Convert.ToString(item.Value)))
                            {
                                tempBM = JsonConvert.DeserializeObject<BuzzWeightedAverScore>(item.Value.ToString());
                                if (tempBM != null)
                                {
                                    oRes.Add(item.Key, tempBM);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.BaseLog log = new Logger.BaseLog(eLogType.CodeLog, DateTime.UtcNow, true);
                log.Message = string.Format("GetBuzzAverScore: ex={0} in {1}", ex.Message, ex.StackTrace);
                log.Error(log.Message, false);
            }

            return oRes;
        }

        public BuzzWeightedAverScore GetBuzzAverScore(string sKey)
        {
            BuzzWeightedAverScore oRes = null;
            try
            {
                oRes = cbManager.GetJsonAsT<BuzzWeightedAverScore>(sKey);
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
