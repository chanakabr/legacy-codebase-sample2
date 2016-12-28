using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.SessionState;
using System.Configuration;

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
            m_dUpdateDate = DateTime.Now;
        }
    }

    public class SelectCacher
    {
        static protected string m_sLocker = "";
        static public Int32 GetCachedSec()
        {
            if (Utils.GetTcmConfigValue("ODBC_CACH_SEC") != string.Empty)
            {
                return int.Parse(Utils.GetTcmConfigValue("ODBC_CACH_SEC"));
            }
            if (HttpContext.Current != null)
            {
                if (HttpContext.Current.Session["ODBC_CACH_SEC"] != null)
                    return int.Parse(HttpContext.Current.Session["ODBC_CACH_SEC"].ToString());
                else
                    return 60;
            }
            else
                return 60;
        }
        protected SelectCacher()
        {
        }

        static public System.Data.DataTable GetCachedDataTable(string sCachStr)
        {
            if (Utils.GetTcmConfigValue("ODBC_CACH_SEC") != string.Empty)
            {
                return GetCachedDataTable(sCachStr, int.Parse(Utils.GetTcmConfigValue("ODBC_CACH_SEC")));
            }
            if (HttpContext.Current != null)
            {
                if (HttpContext.Current.Session["ODBC_CACH_SEC"] != null)
                    return GetCachedDataTable(sCachStr, int.Parse(HttpContext.Current.Session["ODBC_CACH_SEC"].ToString()));
                else
                    return null;
            }
            else
                return null;
        }

        /// 
        /// Remove all the Cache Items from the Current Cache ...
        /// 

        static public void ClearCache()
        {

            System.Collections.IDictionaryEnumerator CacheEnum = null;
            CacheEnum = HttpRuntime.Cache.GetEnumerator();
            while (CacheEnum.MoveNext())
            {
                string key = CacheEnum.Key.ToString();
                HttpRuntime.Cache.Remove(key);
            }
        }

        static public System.Data.DataTable GetCachedDataTable(string sCachStr, Int32 nCachSec)
        {
            try
            {
                if (nCachSec <= 0)
                    return null;
                if (HttpRuntime.Cache[sCachStr] != null)
                    return ((System.Data.DataTable)(CachingManager.CachingManager.GetCachedData(sCachStr))).Copy();
                //return ((System.Data.DataTable)(HttpRuntime.Cache[sCachStr])).Copy();
                else
                    return null;
            }
            catch
            {
                return null;
            }
        }

        static public void SetCachedDataTable(string sCachStr, System.Data.DataTable dDataTable, int cacheSec = 0)
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

                CachingManager.CachingManager.SetCachedData(sCachStr, dDataTable.Copy(), cacheSec, System.Web.Caching.CacheItemPriority.Default, 0, true);
                //HttpRuntime.Cache.Add(sCachStr, dDataTable.Copy(), null, DateTime.Now.AddHours(nCacheSec), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Default, null);
            }
            catch
            {
                ClearCache();
            }
        }
    }
}
