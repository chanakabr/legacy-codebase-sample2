
  using Couchbase;
    using Couchbase.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using CouchbaseManager;
    using System.Configuration;
    using ApiObjects;
    using Newtonsoft.Json;
using Logger;



namespace DalCB
{
    public class EpgDal_Couchbase
    {
        private static readonly string sEndMaxValue = @"\uefff";
        private static readonly string CB_EPG_DESGIN = Utils.GetValFromConfig("cb_epg_design");

        CouchbaseClient m_oClient;
        private int m_nGroupID;

        public EpgDal_Couchbase(int nGroupID)
        {
            m_nGroupID = nGroupID;
            m_oClient = CouchbaseManager.CouchbaseManager.GetInstance(eCouchbaseBucket.EPG);
        }

        //Given a key, will generatre a unique number that can be used as a unique identifier
        public ulong IDGenerator(string sKey)
        {
            return m_oClient.Increment(sKey, 1, 1);
        }

        //This method uses StoreMode.Add, hence can only be used for new documents.
        public bool InsertProgram(string sDocID, object epg, DateTime? dtExpiresAt)
        {
            bool bRes = false;

            if (epg != null)
            {
                try
                {
                    bRes = (dtExpiresAt.HasValue) ? m_oClient.StoreJson(Enyim.Caching.Memcached.StoreMode.Add, sDocID, epg, dtExpiresAt.Value) :
                                                   m_oClient.StoreJson(Enyim.Caching.Memcached.StoreMode.Add, sDocID, epg);
                }
                catch (Exception ex)
                {
                    Logger.BaseLog log = new Logger.BaseLog(eLogType.CodeLog, DateTime.UtcNow, true);
                    log.Message = string.Format("InsertProgram: ex={0} in {1}", ex.Message, ex.StackTrace);
                    log.Error(log.Message, false);
                }
            }

            return bRes;
        }


        public bool InsertProgram(string sDocID, object epg, DateTime? dtExpiresAt, ulong cas)
        {
            bool bRes = false;

            if (epg != null)
            {
                try
                {
                    
                    bRes = (dtExpiresAt.HasValue) ? m_oClient.CasJson(Enyim.Caching.Memcached.StoreMode.Add, sDocID, epg, cas, dtExpiresAt.Value) :
                                                   m_oClient.CasJson(Enyim.Caching.Memcached.StoreMode.Add, sDocID, epg, cas);
                }
                catch (Exception ex)
                {
                    Logger.BaseLog log = new Logger.BaseLog(eLogType.CodeLog, DateTime.UtcNow, true);
                    log.Message = string.Format("InsertProgram with cas: ex={0} in {1}", ex.Message, ex.StackTrace);
                    log.Error(log.Message, false);
                }
            }

            return bRes;
        }

        //This method uses StoreMode.Set, hence can Inserts&Updates a records
        public bool UpdateProgram(string sDocID, object epg, DateTime? dtExpiresAt)
        {
            bool bRes = false;

            if (epg != null)
            {
                try
                {
                    bRes = (dtExpiresAt.HasValue) ? m_oClient.StoreJson(Enyim.Caching.Memcached.StoreMode.Set, sDocID, epg, dtExpiresAt.Value) :
                                                    m_oClient.StoreJson(Enyim.Caching.Memcached.StoreMode.Set, sDocID, epg);
                }
                catch (Exception ex)
                {
                    Logger.BaseLog log = new Logger.BaseLog(eLogType.CodeLog, DateTime.UtcNow, true);
                    log.Message = string.Format("UpdateProgram: ex={0} in {1}", ex.Message, ex.StackTrace);
                    log.Error(log.Message, false);
                }
            }

            return bRes;
        }

        public bool UpdateProgram(string sDocID, object epg, DateTime? dtExpiresAt, ulong cas)
        {
            bool bRes = false;

            if (epg != null)
            {
                try
                {
                    bRes = (dtExpiresAt.HasValue) ? m_oClient.CasJson(Enyim.Caching.Memcached.StoreMode.Set, sDocID, epg, cas, dtExpiresAt.Value) :
                                                    m_oClient.CasJson(Enyim.Caching.Memcached.StoreMode.Set, sDocID, epg, cas);
                }
                catch (Exception ex)
                {
                    Logger.BaseLog log = new Logger.BaseLog(eLogType.CodeLog, DateTime.UtcNow, true);
                    log.Message = string.Format("UpdateProgram with cas: ex={0} in {1}", ex.Message, ex.StackTrace);
                    log.Error(log.Message, false);
                }
            }

            return bRes;
        }

        public bool DeleteProgram(string sDocID)
        {
            bool bRes = false;
            try
            {
                bRes = m_oClient.Remove(sDocID);
            }
            catch (Exception ex)
            {
                Logger.BaseLog log = new Logger.BaseLog(eLogType.CodeLog, DateTime.UtcNow, true);
                log.Message = string.Format("DeleteProgram: ex={0} in {1}", ex.Message, ex.StackTrace);
                log.Error(log.Message, false);
            }

            return bRes;
        }

        public EpgCB GetProgram(string id)
        {
            EpgCB oRes = null;
            try
            {
                oRes = m_oClient.GetJson<EpgCB>(id);
            }
            catch (Exception ex)
            {
                Logger.BaseLog log = new Logger.BaseLog(eLogType.CodeLog, DateTime.UtcNow, true);
                log.Message = string.Format("GetProgram: ex={0} in {1}", ex.Message, ex.StackTrace);
                log.Error(log.Message, false);
            }

            return oRes;
        }

        public EpgCB GetProgram(string id, out ulong cas)
        {
            EpgCB oRes = null;
            cas = 0;
            try
            {
                var casObj = m_oClient.GetWithCas<string>(id);
                oRes = JsonConvert.DeserializeObject<EpgCB>(casObj.Result);
                cas = casObj.Cas;
            }
            catch (Exception ex)
            {
                Logger.BaseLog log = new Logger.BaseLog(eLogType.CodeLog, DateTime.UtcNow, true);
                log.Message = string.Format("GetProgram: ex={0} in {1}", ex.Message, ex.StackTrace);
                log.Error(log.Message, false);
            }

            return oRes;
        }

        public List<EpgCB> GetProgram(List<string> lIds)
        {
            List<EpgCB> oRes = new List<EpgCB>();
            try
            {
                if (lIds != null && lIds.Count > 0)
                {
                    IDictionary<string, object> dItems = m_oClient.Get(lIds);
                    if (dItems != null && dItems.Count > 0)
                    {
                        EpgCB tempEpg;
                        foreach (KeyValuePair<string, object> item in dItems)
                        {
                            if (item.Value != null && !string.IsNullOrEmpty(item.Value as string))
                            {
                                tempEpg = JsonConvert.DeserializeObject<EpgCB>(item.Value.ToString());
                                if (tempEpg != null)
                                {
                                    oRes.Add(tempEpg);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.BaseLog log = new Logger.BaseLog(eLogType.CodeLog, DateTime.UtcNow, true);
                log.Message = string.Format("GetProgram: ex={0} in {1}", ex.Message, ex.StackTrace);
                log.Error(log.Message, false);
            }

            return oRes;
        }

        //returns all programs with group id from view (does not take start_date into consideration)
        public List<EpgCB> GetGroupPrograms(int nPageSize, int nStartIndex)
        {
            List<EpgCB> lRes = new List<EpgCB>();
            List<object> startKey = new List<object>() { m_nGroupID };
            List<object> endKey = new List<object>() { m_nGroupID, sEndMaxValue };

            try
            {
                var res = (nPageSize > 0) ? m_oClient.GetView<EpgCB>(CB_EPG_DESGIN, "group_programs", true).StartKey(startKey).EndKey(endKey).Skip(nStartIndex).Limit(nPageSize) :
                                                          m_oClient.GetView<EpgCB>(CB_EPG_DESGIN, "group_programs", true).StartKey(startKey).EndKey(endKey);
                if (res != null)
                {
                    lRes = res.ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.BaseLog log = new Logger.BaseLog(eLogType.CodeLog, DateTime.UtcNow, true);
                log.Message = string.Format("GetGroupPrograms: ex={0} in {1}", ex.Message, ex.StackTrace);
                log.Error(log.Message, false);
            }

            return lRes;
        }

        //(dStartDate <= start_date < sEndMaxValue) 
        //returns all programs with group id, that have a start date that's greater than or equal to dStartDate
        public List<EpgCB> GetGroupProgramsByStartDate(int nPageSize, int nStartIndex, DateTime dStartDate)
        {
            List<EpgCB> lRes = new List<EpgCB>();
            List<object> startKey = new List<object>() { m_nGroupID, dStartDate.ToString("yyyyMMddHHmmss") };
            List<object> endKey = new List<object>() { m_nGroupID, sEndMaxValue };

            try
            {
                var res = (nPageSize > 0) ? m_oClient.GetView<EpgCB>(CB_EPG_DESGIN, "group_programs", true).StartKey(startKey).EndKey(endKey).Skip(nStartIndex).Limit(nPageSize) :
                    m_oClient.GetView<EpgCB>(CB_EPG_DESGIN, "group_programs", true).StartKey(startKey).EndKey(endKey);

                if (res != null)
                {
                    lRes = res.ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.BaseLog log = new Logger.BaseLog(eLogType.CodeLog, DateTime.UtcNow, true);
                log.Message = string.Format("GetGroupProgramsByStartDate: ex={0} in {1}", ex.Message, ex.StackTrace);
                log.Error(log.Message, false);
            }

            return lRes;
        }

        //(dFromDate <= start_date < dToDate)
        //returns all programs with group id, that have a start date that's greater than or equal to dFromDate and smaller than dToDate
        public List<EpgCB> GetGroupProgramsByStartDate(int nPageSize, int nStartIndex, DateTime dFromDate, DateTime dToDate)
        {
            List<EpgCB> lRes = new List<EpgCB>();
            List<object> startKey = new List<object>() { m_nGroupID, dFromDate.ToString("yyyyMMddHHmmss") };
            List<object> endKey = new List<object>() { m_nGroupID, dToDate.ToString("yyyyMMddHHmmss") };

            try
            {
                var res = (nPageSize > 0) ? m_oClient.GetView<EpgCB>(CB_EPG_DESGIN, "group_programs", true).StartKey(startKey).EndKey(endKey).Skip(nStartIndex).Limit(nPageSize) :
                    m_oClient.GetView<EpgCB>(CB_EPG_DESGIN, "group_programs", true).StartKey(startKey).EndKey(endKey);

                if (res != null)
                {
                    lRes = res.ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.BaseLog log = new Logger.BaseLog(eLogType.CodeLog, DateTime.UtcNow, true);
                log.Message = string.Format("GetGroupProgramsByStartDate: ex={0} in {1}", ex.Message, ex.StackTrace);
                log.Error(log.Message, false);
            }

            return lRes;
        }

        //returns all channel programs from view. (does not take start/end date into consideration) 
        public List<EpgCB> GetChannelPrograms(int nPageSize, int nStartIndex, int nChannelID)
        {
            List<EpgCB> lRes = new List<EpgCB>();
            List<object> startKey = new List<object>() { m_nGroupID, nChannelID };
            List<object> endKey = new List<object>() { m_nGroupID, nChannelID, sEndMaxValue };
            try
            {
                var res = (nPageSize > 0) ? m_oClient.GetView<EpgCB>(CB_EPG_DESGIN, "channel_programs", true).Key(startKey).EndKey(endKey).Skip(nStartIndex).Limit(nPageSize) :
                                                            m_oClient.GetView<EpgCB>(CB_EPG_DESGIN, "channel_programs", true).Key(startKey).EndKey(endKey);

                if (res != null)
                {
                    lRes = res.ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.BaseLog log = new Logger.BaseLog(eLogType.CodeLog, DateTime.UtcNow, true);
                log.Message = string.Format("GetChannelPrograms: ex={0} in {1}", ex.Message, ex.StackTrace);
                log.Error(log.Message, false);
            }
            return lRes;
        }

        //(fromDate <= start_date < toDate) 
        //returns all channel programs from view that have a startdate that's greater than or equal to fromDate and less than toDate
        public List<EpgCB> GetChannelProgramsByStartDate(int nPageSize, int nStartIndex, int nChannelID, DateTime fromDate, DateTime toDate, bool bDesc)
        {
            List<EpgCB> lRes = new List<EpgCB>();
            List<object> startKey = new List<object>() { m_nGroupID, nChannelID, fromDate.ToString("yyyyMMddHHmmss") };
            List<object> endKey = new List<object>() { m_nGroupID, nChannelID, toDate.ToString("yyyyMMddHHmmss") };
            try
            {
                if (!bDesc)
                {
                    var res = (nPageSize > 0) ? m_oClient.GetView<EpgCB>(CB_EPG_DESGIN, "channel_programs", true).Key(startKey).EndKey(endKey).Skip(nStartIndex).Limit(nPageSize) :
                                                                m_oClient.GetView<EpgCB>(CB_EPG_DESGIN, "channel_programs", true).Key(startKey).EndKey(endKey);

                    if (res != null)
                    {
                        lRes = res.ToList();
                    }
                }
                else
                {//when Sorting the results in Descending order, the startKey and EndKey are switched
                    var res = (nPageSize > 0) ? m_oClient.GetView<EpgCB>(CB_EPG_DESGIN, "channel_programs", true).Key(endKey).EndKey(startKey).Skip(nStartIndex).Descending(bDesc).Limit(nPageSize) :
                                                                  m_oClient.GetView<EpgCB>(CB_EPG_DESGIN, "channel_programs", true).Key(endKey).EndKey(startKey).Descending(bDesc);

                    if (res != null)
                    {
                        lRes = res.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Exception", string.Format("Exception at GetChannelProgramsByStartDate. Msg: {0} , ST: {1}", ex.Message, ex.StackTrace), "EpgDal_Couchbase");
            }

            return lRes;
        }



    }
}

