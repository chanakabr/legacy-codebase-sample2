using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using KLogMonitor;
using System.Reflection;

namespace CachingManager
{
    public class CachingData
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public object m_oVal;
        public DateTime m_dStart;
        public DateTime m_dLastUsed;
        public Int32 m_nUseCounter;
        public Int32 m_nMediaID;
        public Int32 m_nCacheSecs;
        public bool m_bToRenew;
        public System.Web.Caching.CacheItemPriority m_Priority;
        public CachingData(object oVal, Int32 nMediaID, bool bToRenew, Int32 nCacheSecs, System.Web.Caching.CacheItemPriority priority)
        {
            m_oVal = oVal;
            m_nMediaID = nMediaID;
            m_bToRenew = bToRenew;
            m_dStart = DateTime.UtcNow;
            m_dLastUsed = DateTime.UtcNow;
            m_nUseCounter = 1;
            m_nCacheSecs = nCacheSecs;
            m_Priority = priority;
        }

        public void GetValues(ref Int32 nMediaID, ref bool bRenew, ref DateTime dStart, ref DateTime dLastUsed, ref Int32 nCounter, ref Int32 nCacheSecs, ref System.Web.Caching.CacheItemPriority priority)
        {
            nMediaID = m_nMediaID;
            bRenew = m_bToRenew;
            dStart = m_dStart;
            dLastUsed = m_dLastUsed;
            nCounter = m_nUseCounter;
            nCacheSecs = m_nCacheSecs;
            priority = m_Priority;
        }

        public void Hit()
        {
            m_nUseCounter++;
            m_dLastUsed = DateTime.UtcNow;
        }
    }
    public class CachingManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        //static System.Collections.Hashtable cacheKeyList = new System.Collections.Hashtable();

        static public bool Exist(string sName)
        {
            if (System.Web.HttpRuntime.Cache[sName] != null)
                return true;
            return false;
        }

        static public object GetCachedData(string sName)
        {
            if (System.Web.HttpRuntime.Cache[sName] != null)
            {
                ((CachingData)(System.Web.HttpRuntime.Cache[sName])).Hit();
                return ((CachingData)(System.Web.HttpRuntime.Cache[sName])).m_oVal;
            }
            return "";
        }

        /*GetCacheDataObject : this function return the CachingData object for a specific key*/
        static public CachingData GetCacheDataObject(string sName)
        {
            if (System.Web.HttpRuntime.Cache[sName] != null)
            {
                ((CachingData)(System.Web.HttpRuntime.Cache[sName])).Hit();
                return ((CachingData)(System.Web.HttpRuntime.Cache[sName]));
            }
            return null;
        }

        /*GetCacheObject : this function return object for a specific key*/
        static public object GetCacheObject(string sName)
        {
            if (System.Web.HttpRuntime.Cache[sName] != null)
            {
                return ((object)(System.Web.HttpRuntime.Cache[sName]));
            }
            return null;
        }

        static public void RenewCachedData(string sName, CachingData sValue, Int32 nHours, System.Web.Caching.CacheItemPriority oPriority, Int32 nMediaID, bool bToRenew)
        {
            bool bExist = Exist(sName);
            if (bExist)
                System.Web.HttpRuntime.Cache.Remove(sName);
            sValue.m_nUseCounter = 1;
            System.Web.Caching.CacheItemRemovedCallback onRemove = null;
            onRemove = new System.Web.Caching.CacheItemRemovedCallback(CachingManager.CachedRemoved);
            System.Web.HttpRuntime.Cache.Add(sName, sValue, null, DateTime.Now.AddSeconds(nHours), System.Web.Caching.Cache.NoSlidingExpiration, oPriority, onRemove);
            //cacheKeyList[sName] = DateTime.Now; 
        }


        static public void SetCachedData(string sName, object sValue, Int32 nSeconds, System.Web.Caching.CacheItemPriority oPriority, Int32 nMediaID, bool bToRenew)
        {
            bool bExist = Exist(sName);
            if (bExist)
                System.Web.HttpRuntime.Cache.Remove(sName);
            System.Web.Caching.CacheItemRemovedCallback onRemove = null;
            onRemove = new System.Web.Caching.CacheItemRemovedCallback(CachingManager.CachedRemoved);
            CachingData theDate = new CachingData(sValue, nMediaID, bToRenew, nSeconds, oPriority);
            System.Web.HttpRuntime.Cache.Add(sName, theDate, null, DateTime.Now.AddSeconds(nSeconds), System.Web.Caching.Cache.NoSlidingExpiration, oPriority, onRemove);
            //cacheKeyList[sName] = DateTime.Now; 
        }

        static public Int32 GetMaxCachedSec()
        {
            if (GetTcmConfigValue("CACHE_MAX_SEC") != string.Empty)
            {
                return int.Parse(GetTcmConfigValue("CACHE_MAX_SEC"));
            }
            else
                return 86400;
        }

        static public void RemoveFromCache(string sKey)
        {
            System.Collections.ArrayList removeList = new System.Collections.ArrayList();

            foreach (System.Collections.DictionaryEntry entry in System.Web.HttpRuntime.Cache)
            {
                if (entry.Key.ToString().StartsWith(sKey) || sKey == "")
                    removeList.Add(entry.Key.ToString());
            }
            foreach (string key in removeList)
            {
                try
                {
                    System.Web.HttpRuntime.Cache.Remove(key);
                }
                catch
                { }
            }
        }

        static public void CachedRemoved(string key, Object value, System.Web.Caching.CacheItemRemovedReason reason)
        {
            //cacheKeyList.Remove(key); 
            if (reason == System.Web.Caching.CacheItemRemovedReason.Expired || reason == System.Web.Caching.CacheItemRemovedReason.Underused)
            {
                if (value.GetType().ToString() == typeof(CachingData).ToString())
                {
                    try
                    {
                        Int32 nMediaID = 0;
                        bool bRenew = false;
                        DateTime dStart = DateTime.Now.AddHours(-48);
                        DateTime dLastUsed = DateTime.Now.AddHours(-48);
                        Int32 nCounter = 0;
                        Int32 nCacheSec = 0;
                        System.Web.Caching.CacheItemPriority priority = System.Web.Caching.CacheItemPriority.Default;
                        ((CachingData)(value)).GetValues(ref nMediaID, ref bRenew, ref dStart, ref dLastUsed, ref nCounter, ref nCacheSec, ref priority);
                        if (bRenew == true || nCacheSec <= 10800)
                        {
                            DateTime dNow = DateTime.Now;
                            Int32 nSecs = (Int32)((dNow - dStart).TotalSeconds);
                            if (nCounter > 5 || (nCacheSec < 1800 && nCounter > 1))
                            {
                                if (nSecs < 86400)
                                {
                                    Int32 nLastUsedSec = (Int32)((dNow - dLastUsed).TotalSeconds);
                                    if (nCacheSec > nLastUsedSec * 2 || nCacheSec < 300)
                                        RenewCachedData(key, (CachingData)value, nCacheSec, priority, nMediaID, true);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("Exception - " + ex.Message + " | " + ex.StackTrace, ex);
                    }
                }
            }
        }


        static public string GetTcmConfigValue(string sKey)
        {
            string result = string.Empty;
            try
            {
                result = TCMClient.Settings.Instance.GetValue<string>(sKey);
            }
            catch (Exception ex)
            {
                result = string.Empty;
                log.Error("CachingManager - Key=" + sKey + "," + ex.Message, ex);
            }
            return result;
        }

        static public List<string> GetCachedKeys()
        {
            List<string> keys = new List<string>();
            try
            {
                foreach (System.Collections.DictionaryEntry entry in System.Web.HttpRuntime.Cache)
                {
                    keys.Add(entry.Key.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error("GetCachedKeys failed", ex);
            }
            return keys;
        }

    }
}
