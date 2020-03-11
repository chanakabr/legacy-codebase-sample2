using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Caching;
using System.Web;

namespace TVPPro.SiteManager.BaseClass
{
    /// <summary>
    /// Use this base class for inheritance in every implamation of url shorten service.
    /// </summary>
    public abstract class BaseUrlShortenHelper<T> 
        where T:new()
    {
        protected static Cache _cache = HttpContext.Current.Cache;
        protected static object _locker = new object();
        private static T _instance; 

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_locker)
                    {
                        if (_instance == null)
                        {
                            _instance = new T();
                        }
                    }

                }
                return _instance;
            }
        }
        /// <summary>
        /// Get a short url to a link ( cached ) 
        /// </summary>
        /// <param name="longUrl">url to be shorten</param>
        /// <returns></returns>
        public abstract string ShortenUrl(string longUrl);
    }
}
