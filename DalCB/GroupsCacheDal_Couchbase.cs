using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Couchbase;
using CouchbaseManager;
using Couchbase.Extensions;
using Logger;

namespace DalCB
{
    public class GroupsCacheDal_Couchbase
    {
        private static readonly string sEndMaxValue = @"\uefff";
        //private static readonly string CB_EPG_DESGIN = GetValFromConfig("cb_epg_design");

        CouchbaseClient m_oClient;
        private int m_nGroupID;

        public GroupsCacheDal_Couchbase(int nGroupID)
        {
            m_nGroupID = nGroupID;
            m_oClient = CouchbaseManager.CouchbaseManager.GetInstance(eCouchbaseBucket.CACHE);
        }

        //This method uses StoreMode.Add, hence can only be used for new documents.
        public bool InsertGroup(string sDocID, object group, DateTime? dtExpiresAt)
        {
            bool bRes = false;

            if (group != null)
            {
                try
                {
                    bRes = (dtExpiresAt.HasValue) ? m_oClient.StoreJson(Enyim.Caching.Memcached.StoreMode.Add, sDocID, group, dtExpiresAt.Value) :
                                                   m_oClient.StoreJson(Enyim.Caching.Memcached.StoreMode.Add, sDocID, group);
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
        public bool InsertGroup(string sDocID, object group, DateTime? dtExpiresAt, ulong cas)
        {
            bool bRes = false;

            if (group != null)
            {
                try
                {

                    bRes = (dtExpiresAt.HasValue) ? m_oClient.CasJson(Enyim.Caching.Memcached.StoreMode.Add, sDocID, group, cas, dtExpiresAt.Value) :
                                                   m_oClient.CasJson(Enyim.Caching.Memcached.StoreMode.Add, sDocID, group, cas);
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

        public object GetGroup(string sDocID)
        {
            object oRes = null;
            try
            {
                oRes = m_oClient.Get(sDocID);
            }
            catch (Exception ex)
            {
                Logger.BaseLog log = new Logger.BaseLog(eLogType.CodeLog, DateTime.UtcNow, true);
                log.Message = string.Format("GetGroup: ex={0} in {1}", ex.Message, ex.StackTrace);
                log.Error(log.Message, false);
            }

            return oRes;
        }

        public bool DeleteGroup(string sDocID)
        {
            bool bRes = false;
            try
            {
                bRes = m_oClient.Remove(sDocID);
            }
            catch (Exception ex)
            {
                Logger.BaseLog log = new Logger.BaseLog(eLogType.CodeLog, DateTime.UtcNow, true);
                log.Message = string.Format("DeleteGroup: ex={0} in {1}", ex.Message, ex.StackTrace);
                log.Error(log.Message, false);
            }

            return bRes;
        }


        //This method uses StoreMode.Set, hence can Inserts&Updates a records
        public bool UpdateGroup(string sDocID, object group, DateTime? dtExpiresAt)
        {
            bool bRes = false;

            if (group != null)
            {
                try
                {
                    bRes = (dtExpiresAt.HasValue) ? m_oClient.StoreJson(Enyim.Caching.Memcached.StoreMode.Set, sDocID, group, dtExpiresAt.Value) :
                                                    m_oClient.StoreJson(Enyim.Caching.Memcached.StoreMode.Set, sDocID, group);
                }
                catch (Exception ex)
                {
                    Logger.BaseLog log = new Logger.BaseLog(eLogType.CodeLog, DateTime.UtcNow, true);
                    log.Message = string.Format("UpdateGroup: ex={0} in {1}", ex.Message, ex.StackTrace);
                    log.Error(log.Message, false);
                }
            }

            return bRes;
        }

        public bool UpdateGroup(string sDocID, object group, DateTime? dtExpiresAt, ulong cas)
        {
            bool bRes = false;

            if (group != null)
            {
                try
                {
                    bRes = (dtExpiresAt.HasValue) ? m_oClient.CasJson(Enyim.Caching.Memcached.StoreMode.Set, sDocID, group, cas, dtExpiresAt.Value) :
                                                    m_oClient.CasJson(Enyim.Caching.Memcached.StoreMode.Set, sDocID, group, cas);
                }
                catch (Exception ex)
                {
                    Logger.BaseLog log = new Logger.BaseLog(eLogType.CodeLog, DateTime.UtcNow, true);
                    log.Message = string.Format("UpdateGroup with cas: ex={0} in {1}", ex.Message, ex.StackTrace);
                    log.Error(log.Message, false);
                }
            }

            return bRes;
        }

        public static string GetValFromConfig(string sKey)
        {
            return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);

        }

    }
}
