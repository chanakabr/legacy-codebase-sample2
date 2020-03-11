using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Web;
using System.Threading;
using System.Configuration;
using System.Web.Caching;
using KLogMonitor;
using System.Reflection;

namespace Tvinci.Data.DataLoader
{
    internal class CacheItemWrapper
    {
        public string[] Categories { get; private set; }
        public DateTime CreationTime { get; private set; }
        public object Value { get; private set; }
        public CacheItemWrapper(object value, string[] categories)
        {
            Value = value;
            Categories = categories;
            CreationTime = DateTime.Now;
        }
    }

    public class CategoryContext
    {
        private ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

        public string Name { get; private set; }
        private DateTime m_expirationTime;
        public int DurationInHours { get; private set; }
        private DateTime m_validStartDate = DateTime.Now;

        public DateTime GetValidStartDate()
        {
            return m_validStartDate;
        }

        public void InitializeStartDate()
        {
            calculateUpdateTime();
        }

        public DateTime GetExpirationTime()
        {
            if (locker.TryEnterUpgradeableReadLock(4000))
            {
                try
                {
                    if (m_expirationTime < DateTime.Now)
                    {
                        calculateUpdateTime();
                    }

                    return m_expirationTime;
                }
                finally
                {
                    locker.ExitUpgradeableReadLock();
                }
            }

            return m_expirationTime;
        }

        private void calculateUpdateTime()
        {
            if (locker.TryEnterWriteLock(4000))
            {
                try
                {
                    m_validStartDate = DateTime.Now;

                    if (m_expirationTime < DateTime.Now)
                    {
                        if (m_expirationTime < DateTime.Now)
                        {
                            m_expirationTime = DateTime.Now;
                            int gap = DateTime.Now.Hour % DurationInHours;
                            m_expirationTime = m_expirationTime.AddHours(DurationInHours - gap);
                        }
                    }
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }
        }

        public CategoryContext(string name, int durationInHours)
        {
            Name = name;
            DurationInHours = durationInHours;

            calculateUpdateTime();
        }
    }

    public class CategoriesHandler
    {
        public static readonly CategoriesHandler Instance = new CategoriesHandler();

        private bool m_initialized = false;
        private object m_locker = new object();

        CategoriesHandler()
        {

        }

        public void Initialize(Dictionary<string, CategoryContext> categories)
        {
            lock (m_locker)
            {
                if (m_initialized)
                {
                    throw new Exception("This instance was already initialized.");
                }

                m_initialized = true;
                m_categories = categories;
            }
        }

        private Dictionary<string, CategoryContext> m_categories = null;

        public DateTime GetValidStartDate(string[] categories)
        {
            if (!m_initialized)
            {
                throw new Exception("This instance must be initialized during application start in global.asax");
            }

            DateTime validStart = DateTime.Now.AddHours(-1);// default

            if (categories != null && categories.Length > 1)
            {
                CategoryContext context;

                if (categories.Length == 1)
                {
                    if (m_categories.TryGetValue(categories[0], out context))
                    {
                        DateTime validDate = context.GetValidStartDate();

                        if (validStart < validDate)
                        {
                            validStart = validDate;
                        }
                    }
                }
                else
                {
                    foreach (string category in categories)
                    {
                        if (m_categories.TryGetValue(category, out context))
                        {
                            DateTime validDate = context.GetValidStartDate();

                            if (validStart < validDate)
                            {
                                validStart = validDate;
                            }
                        }
                    }
                }
            }

            return validStart;


        }

        public DateTime GetExpiration(string[] categories)
        {
            if (!m_initialized)
            {
                throw new Exception("This instance must be initialized during application start in global.asax");
            }

            DateTime expiration = DateTime.Now.AddHours(4); // default

            if (categories != null && categories.Length > 1)
            {
                CategoryContext context;

                if (categories.Length == 1)
                {
                    if (m_categories.TryGetValue(categories[0], out context))
                    {
                        DateTime categoryExpiration = context.GetExpirationTime();

                        if (expiration > categoryExpiration)
                        {
                            expiration = categoryExpiration;
                        }
                    }
                }
                else
                {
                    foreach (string category in categories)
                    {
                        if (m_categories.TryGetValue(category, out context))
                        {
                            DateTime categoryExpiration = context.GetExpirationTime();

                            if (expiration > categoryExpiration)
                            {
                                expiration = categoryExpiration;
                            }
                        }
                    }
                }
            }

            return expiration;
        }
    }

    public sealed class LoaderCacheLite : ILoaderCache
    {
        static LoaderCacheLite instance = new LoaderCacheLite();

        public static LoaderCacheLite Current
        {
            get
            {
                return instance;
            }
        }

        public int DefaultCacheDuration { get; private set; }
        public bool ShouldUseCache { get; private set; }

        private LoaderCacheLite()
        {
            int duration;
            if (int.TryParse(ConfigurationManager.AppSettings["Tvinci.DataLoader.CacheLite.DurationInMinutes"], out duration))
            {
                DefaultCacheDuration = duration;
            }
            else
            {
                DefaultCacheDuration = 60;
            }

            bool useCache;
            if (bool.TryParse(ConfigurationManager.AppSettings["Tvinci.DataLoader.CacheLite.ShouldUseCache"], out useCache))
            {
                ShouldUseCache = useCache;
            }
            else
            {
                ShouldUseCache = false;
            }


            // no implementation needed
        }

        #region ILoaderCache Members

        public bool TryGetData<TData>(string uniqueKey, out TData data)
        {
            if (!ShouldUseCache)
            {
                data = default(TData);
                return false;
            }

            object result = HttpContext.Current.Cache.Get(uniqueKey);
            if (result is TData)
            {
                data = (TData)result;
                //logger.DebugFormat("Item with key '{0}' found", uniqueKey);
                return true;
            }
            else
            {
                data = default(TData);
                //logger.WarnFormat("Item with key '{0}' not found", uniqueKey);
                return false;
            }
        }

        public void AddData(string uniqueKey, object data, string[] categories, int cacheDuration)
        {
            if (ShouldUseCache)
            {
                DateTime expirationTime = DateTime.Now.AddMinutes((cacheDuration <= 0 || cacheDuration > 60) ? DefaultCacheDuration : cacheDuration);

                HttpContext.Current.Cache.Insert(uniqueKey, data, null, expirationTime, System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Default, null);
                logger.DebugFormat("Item with key '{0}' was added until '{1}'", uniqueKey, expirationTime.ToLongTimeString());
            }
        }

        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private void cacheRemove(string key, object value, CacheItemRemovedReason reason)
        {
            //logger.WarnFormat("Item with key '{0}' was removed from cache. reason = '{1};", key, reason);
        }
        #endregion
    }



















    //public sealed class LoaderCache : ILoaderCache
    //{       
    //    internal LoaderCache()
    //    {
    //        // no implementation needed
    //    }

    //    #region ILoaderCache Members

    //    public bool TryGetData<TData>(string uniqueKey, out TData data)
    //    {            
    //        data = default(TData);
    //        return false;

    //        CacheItemWrapper obj = HttpContext.Current.Cache.Get(uniqueKey) as CacheItemWrapper;

    //        if (obj != null && obj.CreationTime > CategoriesHandler.Instance.GetValidStartDate(obj.Categories))
    //        {
    //            if (obj is TData)
    //            {
    //                data = (TData)obj.Value;
    //                return true;
    //            }                
    //        }

    //        return false;
    //    }

    //    public void AddData(string uniqueKey, object data, string[] categories)
    //    {
    //        return;
    //        HttpContext.Current.Cache.Insert(uniqueKey, new CacheItemWrapper(data, categories),null, CategoriesHandler.Instance.GetExpiration(categories),System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Default, null);            
    //    }       
    //    #endregion
    //}

}
