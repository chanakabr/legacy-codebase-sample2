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
using KLogMonitor;
using System.Reflection;



namespace DalCB
{
    public class EpgDal_Couchbase
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly string sEndMaxValue = @"\uefff";
        private static readonly string CB_EPG_DESGIN = Utils.GetValFromConfig("cb_epg_design");
        private static readonly string EPG_DAL_CB_LOG_FILE = "EpgDAL_CB";

        CouchbaseClient m_oClient;
        private int m_nGroupID;

        public EpgDal_Couchbase(int nGroupID)
        {
            m_nGroupID = nGroupID;
            m_oClient = CouchbaseManager.CouchbaseManager.GetInstance(eCouchbaseBucket.EPG);
        }

        private string GetLogFileName()
        {
            return String.Concat(EPG_DAL_CB_LOG_FILE, "_", m_nGroupID);
        }

        //Given a key, will generate a unique number that can be used as a unique identifier
        public ulong IDGenerator(string sKey)
        {
            return m_oClient.Increment(sKey, 1, 1);
        }

        public bool InsertProgram(string sDocID, object epg, DateTime? dtExpiresAt)
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
                    #region Logging
                    StringBuilder sb = new StringBuilder("Exception at InsertProgram (3 argument overload). ");
                    sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                    sb.Append(String.Concat(" Doc ID: ", sDocID));
                    sb.Append(String.Concat(" EPG: ", epg != null ? epg.ToString() : "null"));
                    sb.Append(String.Concat(" ExpiresAt: ", dtExpiresAt != null ? dtExpiresAt.ToString() : "null"));
                    sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                    sb.Append(String.Concat(" ST: ", ex.StackTrace));
                    log.Error("Exception - " + sb.ToString(), ex);
                    #endregion
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

                    // TODO  : add here the json serialize 
                    bRes = (dtExpiresAt.HasValue) ? m_oClient.CasJson(Enyim.Caching.Memcached.StoreMode.Set, sDocID, epg, cas, dtExpiresAt.Value) :
                                                   m_oClient.CasJson(Enyim.Caching.Memcached.StoreMode.Set, sDocID, epg, cas);
                }
                catch (Exception ex)
                {
                    #region Logging
                    StringBuilder sb = new StringBuilder("Exception at InsertProgram. ");
                    sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                    sb.Append(String.Concat(" Doc ID: ", sDocID));
                    sb.Append(String.Concat(" EPG: ", epg != null ? epg.ToString() : "null"));
                    sb.Append(String.Concat(" ExpiresAt: ", dtExpiresAt != null ? dtExpiresAt.ToString() : "null"));
                    sb.Append(String.Concat(" CAS: ", cas));
                    sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                    sb.Append(String.Concat(" ST: ", ex.StackTrace));
                    log.Error("Exception - " + sb.ToString(), ex);
                    #endregion
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
                    #region Logging
                    StringBuilder sb = new StringBuilder("Exception at UpdateProgram (3 argument overload). ");
                    sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                    sb.Append(String.Concat(" Doc ID: ", sDocID));
                    sb.Append(String.Concat(" EPG: ", epg != null ? epg.ToString() : "null"));
                    sb.Append(String.Concat(" ExpiresAt: ", dtExpiresAt != null ? dtExpiresAt.ToString() : "null"));
                    sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                    sb.Append(String.Concat(" ST: ", ex.StackTrace));
                    log.Error("Exception - " + sb.ToString(), ex);
                    #endregion
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
                    #region Logging
                    StringBuilder sb = new StringBuilder("Exception at UpdateProgram. ");
                    sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                    sb.Append(String.Concat(" Doc ID: ", sDocID));
                    sb.Append(String.Concat(" EPG: ", epg != null ? epg.ToString() : "null"));
                    sb.Append(String.Concat(" ExpiresAt: ", dtExpiresAt != null ? dtExpiresAt.ToString() : "null"));
                    sb.Append(String.Concat(" CAS: ", cas));
                    sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                    sb.Append(String.Concat(" ST: ", ex.StackTrace));
                    log.Error("Exception - " + sb.ToString(), ex);
                    #endregion
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
                log.Error("Exception - " + string.Format("Exception at DeleteProgram. Msg: {0} , Doc ID: {1} , Ex Type: {2} , ST: {3}", ex.Message, sDocID, ex.GetType().Name, ex.StackTrace), ex);
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
                log.Error("Exception 0 " + string.Format("Exception at GetProgram. Msg: {0} , ID: {1} , Ex Type: {2} , ST: {3}", ex.Message, id, ex.GetType().Name, ex.StackTrace), ex);
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
                log.Error("Exception - " + string.Format("Exception at GetProgram (2 argument overload). Msg: {0} , ID: {1} , Ex Type: {2} , ST: {3}", ex.Message, id, ex.GetType().Name, ex.StackTrace), ex);
            }

            return oRes;
        }

        public List<EpgCB> GetProgram(List<string> p_lstIds)
        {
            List<EpgCB> lstResultEpgs = new List<EpgCB>();

            try
            {
                if (p_lstIds != null && p_lstIds.Count > 0)
                {
                    IDictionary<string, object> dicItems = m_oClient.Get(p_lstIds);

                    if (dicItems != null && dicItems.Count > 0)
                    {
                        // Run on original list of Ids, to maintain their order
                        foreach (var sId in p_lstIds)
                        {
                            // Make sure the Id was returned from CB
                            if (dicItems.ContainsKey(sId))
                            {
                                object oValue = dicItems[sId];

                                // If the value that CB returned is valid
                                if (oValue != null && oValue is string)
                                {
                                    string sValue = Convert.ToString(oValue);

                                    if (!string.IsNullOrEmpty(sValue))
                                    {
                                        // Deserialize string from CB to an EpgCB object
                                        EpgCB oTempEpg = JsonConvert.DeserializeObject<EpgCB>(sValue);

                                        // If it was successful, add to list
                                        if (oTempEpg != null)
                                        {
                                            lstResultEpgs.Add(oTempEpg);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder("IDs: ");
                if (p_lstIds != null && p_lstIds.Count > 0)
                {
                    for (int i = 0; i < p_lstIds.Count; i++)
                    {
                        sb.Append(String.Concat(p_lstIds[i], ";"));
                    }
                }
                else
                {
                    sb.Append("list is null or empty.");
                }

                log.Error("Exception - " + string.Format("Exception at GetProgram (list of ids overload). Msg: {0} , IDs: {1} , Ex Type: {2} , ST: {3}", ex.Message, sb.ToString(), ex.GetType().Name, ex.StackTrace), ex);
            }

            return lstResultEpgs;
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
                log.Error("Exception - " + string.Format("Exception at GetGroupPrograms. Ex Msg: {0} , PS: {1} , SI: {2} , Ex Type: {3} , ST: {4}", ex.Message, nPageSize, nStartIndex, ex.GetType().Name, ex.StackTrace), ex);
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
                log.Error("Exception - " + string.Format("Exception at GetGroupProgramsByStartDate. Ex Msg: {0} , PS: {1} , SI: {2} , Ex Type: {3} , SD: {4} , ST: {5}", ex.Message, nPageSize, nStartIndex, ex.GetType().Name, dStartDate.ToString(), ex.StackTrace), ex);
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
                else
                {
                    log.Debug("group_programs view result is null");
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception - " + string.Format("Exception at GetGroupProgramsByStartDate. Ex Msg: {0} , PS: {1} , SI: {2} , Ex Type: {3} , SD: {4} , TD: {5} , ST: {6}", ex.Message, nPageSize, nStartIndex, ex.GetType().Name, dFromDate.ToString(), dToDate.ToString(), ex.StackTrace), ex);
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
                log.Error("Exception - " + string.Format("Exception at GetChannelPrograms. Ex Msg: {0} , PS: {1} , SI: {2} , C ID: {3} , Ex Type: {4} , ST: {5}", ex.Message, nPageSize, nStartIndex, nChannelID, ex.GetType().Name, ex.StackTrace), ex);
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
                log.Error("Exception - " + string.Format("Exception at GetChannelProgramsByStartDate. Msg: {0} , PS: {1} , SI: {2} ,C ID: {3} , FD: {4} , TD: {5} , Desc: {6} , Ex Type: {7} ST: {8}", ex.Message, nPageSize, nStartIndex, nChannelID, fromDate, toDate, bDesc.ToString(), ex.GetType().Name, ex.StackTrace), ex);
            }

            return lRes;
        }


        public List<EpgCB> GetGroupPrograms(int nPageSize, int nStartIndex, int nParentGroupID, List<string> eIds)
        {
            List<EpgCB> lRes = new List<EpgCB>();
            List<object> Keys = new List<object>();
            try
            {
                foreach (string eID in eIds)
                {
                    List<object> obj = new List<object>() { nParentGroupID, eID.ToString() };

                    Keys.Add(obj);
                }

                var res = (nPageSize > 0) ? m_oClient.GetView<EpgCB>(CB_EPG_DESGIN, "programs_by_identifier", true).Keys(Keys).Skip(nStartIndex).Limit(nPageSize) :
                    m_oClient.GetView<EpgCB>(CB_EPG_DESGIN, "programs_by_identifier", true).Keys(Keys);

                if (res != null)
                {
                    lRes = res.ToList();
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception - " + string.Format("Exception at GetGroupPrograms. Ex Msg: {0} , PS: {1} , SI: {2} , Ex Type: {3} , nParentGroupID: {4}, ST: {5}",
                    ex.Message, nPageSize, nStartIndex, ex.GetType().Name, nParentGroupID, ex.StackTrace), ex);
            }

            return lRes;
        }




        //This method uses StoreMode.Set Store ==>  the data, overwrite if already exist
        public bool SetProgram(string sDocID, object epg, DateTime? dtExpiresAt)
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
                    #region Logging
                    StringBuilder sb = new StringBuilder("Exception at InsertProgram (3 argument overload). ");
                    sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                    sb.Append(String.Concat(" Doc ID: ", sDocID));
                    sb.Append(String.Concat(" EPG: ", epg != null ? epg.ToString() : "null"));
                    sb.Append(String.Concat(" ExpiresAt: ", dtExpiresAt != null ? dtExpiresAt.ToString() : "null"));
                    sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                    sb.Append(String.Concat(" ST: ", ex.StackTrace));
                    log.Error("Exception - " + sb.ToString(), ex);
                    #endregion
                }
            }

            return bRes;
        }

        //This method uses StoreMode.Set Store ==>  the data, overwrite if already exist
        public bool SetProgram(string sDocID, object epg, DateTime? dtExpiresAt, ulong cas)
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
                    #region Logging
                    StringBuilder sb = new StringBuilder("Exception at InsertProgram (3 argument overload). ");
                    sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                    sb.Append(String.Concat(" Doc ID: ", sDocID));
                    sb.Append(String.Concat(" EPG: ", epg != null ? epg.ToString() : "null"));
                    sb.Append(String.Concat(" ExpiresAt: ", dtExpiresAt != null ? dtExpiresAt.ToString() : "null"));
                    sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                    sb.Append(String.Concat(" ST: ", ex.StackTrace));
                    log.Error("Exception - " + sb.ToString(), ex);
                    #endregion
                }
            }

            return bRes;
        }
    }

}

