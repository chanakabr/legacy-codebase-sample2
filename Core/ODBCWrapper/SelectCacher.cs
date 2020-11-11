using System;
using System.Runtime.Caching;
using ConfigurationManager;

namespace ODBCWrapper
{
    class SelectCachWraper
    {
        public string m_sQueryStr;
        public System.Data.DataTable m_dDataTable;
        public DateTime m_dUpdateDate;

        public SelectCachWraper()
        {
            m_sQueryStr = "";
            m_dDataTable = null;
            m_dUpdateDate = DateTime.UtcNow;
        }
    }

    public class SelectCacher
    {
        static protected string m_sLocker = "";
        public static Int32 GetCachedSec()
        {
            var cacheSec = ApplicationConfiguration.Current.DatabaseConfiguration.ODBCCacheSeconds.Value;
            if (cacheSec >= 0)
            {
                return cacheSec;
            }
            // TODO: Find way to store ODBC_CACH_SEC value in a different location or access the current context in a multitarget environment.
            //if (HttpContext.Current != null)
            //{
            //    if (HttpContext.Current.Session["ODBC_CACH_SEC"] != null)
            //        return int.Parse(HttpContext.Current.Session["ODBC_CACH_SEC"].ToString());
            //    else
            //        return 60;
            //}
            else
                return 60;
        }
        protected SelectCacher()
        {
        }



        public static System.Data.DataTable GetCachedDataTable(string sCachStr, Int32 nCachSec)
        {
            try
            {
                if (nCachSec <= 0)
                    return null;

                var cacheValue = CachingManager.CachingManager.GetCachedDataNull(sCachStr);
                if (cacheValue != null && cacheValue is System.Data.DataTable)
                    return ((System.Data.DataTable)cacheValue).Copy();
                else
                    return null;
            }
            catch
            {
                return null;
            }
        }

        public static void SetCachedDataTable(string sCachStr, System.Data.DataTable dDataTable, int cacheSec = 0)
        {
            try
            {
                if (cacheSec == 0)
                {
                    cacheSec = GetCachedSec();
                }

                if (cacheSec == 0)
                {
                    return;
                }

                CachingManager.CachingManager.SetCachedData(sCachStr, dDataTable.Copy(), cacheSec, CacheItemPriority.Default, 0, true);
            }
            catch
            {
            }
        }
    }
}
