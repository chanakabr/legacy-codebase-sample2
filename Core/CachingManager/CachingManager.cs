using System;
using System.Collections.Generic;
using System.Linq;
using KLogMonitor;
using System.Reflection;
using System.Runtime.Caching;

namespace CachingManager
{
    public class CachingData
    {
        private static readonly KLogger _Log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public object _Val;
        public DateTime _StartDate;
        public DateTime _LastUsedDate;
        public int _UseCounter;
        public int _MediaID;
        public int _CacheSec;
        public bool _IsRenewRequired;
        public CacheItemPriority _Priority;
        public CachingData(object oVal, int nMediaID, bool bToRenew, int nCacheSecs, CacheItemPriority priority)
        {
            _Val = oVal;
            _MediaID = nMediaID;
            _IsRenewRequired = bToRenew;
            _StartDate = DateTime.UtcNow;
            _LastUsedDate = DateTime.UtcNow;
            _UseCounter = 1;
            _CacheSec = nCacheSecs;
            _Priority = priority;
        }

        public void GetValues(ref int nMediaID, ref bool bRenew, ref DateTime dStart, ref DateTime dLastUsed, ref int nCounter, ref int nCacheSecs, ref CacheItemPriority priority)
        {
            nMediaID = _MediaID;
            bRenew = _IsRenewRequired;
            dStart = _StartDate;
            dLastUsed = _LastUsedDate;
            nCounter = _UseCounter;
            nCacheSecs = _CacheSec;
            priority = _Priority;
        }

        public void Hit()
        {
            _UseCounter++;
            _LastUsedDate = DateTime.UtcNow;
        }
    }
    public class CachingManager
    {
        private static readonly KLogger _Log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        //static System.Collections.Hashtable cacheKeyList = new System.Collections.Hashtable();

        public static bool Exist(string sName)
        {
            if (MemoryCache.Default[sName] != null)
                return true;
            return false;
        }

        public static object GetCachedData(string sName)
        {
            if (MemoryCache.Default[sName] != null)
            {
                ((CachingData)(MemoryCache.Default[sName])).Hit();
                return ((CachingData)(MemoryCache.Default[sName]))._Val;
            }
            return "";
        }

        public static object GetCachedDataNull(string name)
        {
            if (MemoryCache.Default[name] != null)
            {
                ((CachingData)(MemoryCache.Default[name])).Hit();
                return ((CachingData)(MemoryCache.Default[name]))._Val;
            }
            return null;
        }

        /*GetCacheDataObject : this function return the CachingData object for a specific key*/
        public static CachingData GetCacheDataObject(string sName)
        {
            if (MemoryCache.Default[sName] != null)
            {
                ((CachingData)(MemoryCache.Default[sName])).Hit();
                return ((CachingData)(MemoryCache.Default[sName]));
            }
            return null;
        }

        /*GetCacheObject : this function return object for a specific key*/
        public static object GetCacheObject(string sName)
        {
            if (MemoryCache.Default[sName] != null)
            {
                return ((object)(MemoryCache.Default[sName]));
            }
            return null;
        }

        public static void RenewCachedData(string sName, CachingData sValue, int nHours, CacheItemPriority oPriority, int nMediaID, bool bToRenew)
        {
            var bExist = Exist(sName);
            if (bExist) { MemoryCache.Default.Remove(sName); }

            sValue._UseCounter = 1;
            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTime.UtcNow.AddSeconds(nHours),
                Priority = oPriority,
                RemovedCallback = CachingManager.CachedRemoved,
            };

            MemoryCache.Default.Add(sName, sValue, policy);
        }

        public static void SetCachedData(string sName, object sValue, int nSeconds, CacheItemPriority oPriority, int nMediaID, bool bToRenew)
        {
            var bExist = Exist(sName);
            if (bExist) { MemoryCache.Default.Remove(sName); }

            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTime.UtcNow.AddSeconds(nSeconds),
                Priority = oPriority,
                RemovedCallback = CachingManager.CachedRemoved,
            };

            var theDate = new CachingData(sValue, nMediaID, bToRenew, nSeconds, oPriority);
            MemoryCache.Default.Add(sName, theDate, policy);
        }

        public static void RemoveFromCache(string sKey)
        {
            var removeList = new System.Collections.ArrayList();

            foreach (var entry in MemoryCache.Default)
            {
                if (entry.Key.StartsWith(sKey) || sKey == "")
                    removeList.Add(entry.Key);
            }
            foreach (string key in removeList)
            {
                try
                {
                    MemoryCache.Default.Remove(key);
                }
                catch
                { }
            }
        }

        public static void CachedRemoved(CacheEntryRemovedArguments args)
        {
            var key = args.CacheItem.Key;
            var value = args.CacheItem.Value;
            var reason = args.RemovedReason;

            //cacheKeyList.Remove(key); 
            if (reason == CacheEntryRemovedReason.Expired || reason == CacheEntryRemovedReason.Evicted)
            {
                if (value.GetType().ToString() == typeof(CachingData).ToString())
                {
                    try
                    {
                        var nMediaID = 0;
                        var bRenew = false;
                        var dStart = DateTime.UtcNow.AddHours(-48);
                        var dLastUsed = DateTime.UtcNow.AddHours(-48);
                        var nCounter = 0;
                        var nCacheSec = 0;
                        var priority = CacheItemPriority.Default;
                        ((CachingData)(value)).GetValues(ref nMediaID, ref bRenew, ref dStart, ref dLastUsed, ref nCounter, ref nCacheSec, ref priority);
                        if (bRenew == true || nCacheSec <= 10800)
                        {
                            var dNow = DateTime.UtcNow;
                            var nSecs = (int)((dNow - dStart).TotalSeconds);
                            if (nCounter > 5 || (nCacheSec < 1800 && nCounter > 1))
                            {
                                if (nSecs < 86400)
                                {
                                    var nLastUsedSec = (int)((dNow - dLastUsed).TotalSeconds);
                                    if (nCacheSec > nLastUsedSec * 2 || nCacheSec < 300)
                                        RenewCachedData(key, (CachingData)value, nCacheSec, priority, nMediaID, true);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _Log.Error("Exception - " + ex.Message + " | " + ex.StackTrace, ex);
                    }
                }
            }
        }

        public static List<string> GetCachedKeys()
        {
            var keys = new List<string>();
            try
            {
                keys = MemoryCache.Default.Select(entry => entry.Key.ToString()).ToList();
            }
            catch (Exception ex)
            {
                _Log.Error("GetCachedKeys failed", ex);
            }
            return keys;
        }

    }
}
