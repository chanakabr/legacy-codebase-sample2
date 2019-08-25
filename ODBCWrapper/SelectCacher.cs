using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Text;
using System.Web;
//using System.Web.SessionState;

namespace TVPApi.ODBCWrapper
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
            if (System.Configuration.ConfigurationManager.AppSettings["ODBC_CACH_SEC"] != null &&
                System.Configuration.ConfigurationManager.AppSettings["ODBC_CACH_SEC"].ToString() != "")
            {
                return int.Parse(System.Configuration.ConfigurationManager.AppSettings["ODBC_CACH_SEC"].ToString());
            }
            //if (HttpContext.Current != null)
            //{
            //    if (HttpContext.Current.Session.try
            //        ["ODBC_CACH_SEC"] != null)
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

        static public System.Data.DataTable GetCachedDataTable(string sCachStr)
        {
            if (System.Configuration.ConfigurationManager.AppSettings["ODBC_CACH_SEC"] != null &&
                System.Configuration.ConfigurationManager.AppSettings["ODBC_CACH_SEC"].ToString() != "")
            {
                return GetCachedDataTable(sCachStr, int.Parse(System.Configuration.ConfigurationManager.AppSettings["ODBC_CACH_SEC"].ToString()));
            }
            //if (HttpContext.Current != null)
            //{
            //    if (HttpContext.Current.Session["ODBC_CACH_SEC"] != null)
            //        return GetCachedDataTable(sCachStr, int.Parse(HttpContext.Current.Session["ODBC_CACH_SEC"].ToString()));
            //    else
            //        return null;
            //}
            else
                return null;
        }

        /// 
  /// Remove all the Cache Items from the Current Cache ...
  /// 

        static public void ClearCache()
        {
            System.Collections.IDictionaryEnumerator CacheEnum = null;
            foreach (var entry in MemoryCache.Default)
            {
                string key = entry.Key.ToString();
                MemoryCache.Default.Remove(key);
            }
            //System.Collections.IDictionaryEnumerator CacheEnum = HttpRuntime.Cache.GetEnumerator();
            //while (CacheEnum.MoveNext())
            //{
            //    string key = CacheEnum.Key.ToString();
            //    HttpRuntime.Cache.Remove(key); 
            //}
        }



        static public System.Data.DataTable GetCachedDataTable(string sCachStr, Int32 nCachSec)
        {
            try
            {
                if (nCachSec <= 0)
                    return null;
                if (MemoryCache.Default[sCachStr] != null)
                    return ((System.Data.DataTable)(CachingManager.CachingManager.GetCachedData(sCachStr))).Copy();
                //if (HttpRuntime.Cache[sCachStr] != null)
                //{
                //}
                //{
                //    if (((SelectCachWraper)(HttpRuntime.Cache[sCachStr])).m_dUpdateDate.AddSeconds(nCachSec) > DateTime.Now)
                //        return ((SelectCachWraper)(HttpRuntime.Cache[sCachStr])).m_dDataTable.Copy();
                //    else
                //    {
                //        //lock (m_sLocker)
                //        //{
                //            HttpRuntime.Cache.Remove(sCachStr);
                //        //}
                //    }
                //    return null;
                //}
                //else
                return null;
            }
            catch
            {
                ClearCache();
                return null;
            }
        }

        static public void SetCachedDataTable(string sCachStr, System.Data.DataTable dDataTable)
        {            
            //lock (m_sLocker)
            //{
                try
                {
                    SelectCachWraper d = new SelectCachWraper();
                    d.m_dDataTable = dDataTable.Copy();
                    d.m_dUpdateDate = DateTime.Now;
                    d.m_sQueryStr = sCachStr;

                    CachingManager.CachingManager.SetCachedData(sCachStr, dDataTable.Copy(), GetCachedSec(), CacheItemPriority.Default, 0, true);
                }
                catch
                {
                    ClearCache();
                }
            //}
        }
    }
}
