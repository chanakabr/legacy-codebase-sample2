using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using TVinciShared;

namespace Tvinci.Data.DataLoader
{
    public class RequestCache : ILoaderCache
    {
        private RequestCache()
        {

        }

        static RequestCache cache = new RequestCache();

        public static RequestCache Current
        {
            get
            {
                return cache;
            }
        }        
        #region ILoaderCache Members

        public bool TryGetData<TData>(string uniqueKey, out TData data)
        {            
            object value  = HttpContext.Current.Items[getKey(uniqueKey)];
            if (value is TData)
            {
                data = (TData)value;
                return true;
            }
            else
            {
                data = default(TData);
                return false;
            }

        }

        public string getKey(string token)
        {
            return string.Format("RequestCache.{0}", token); 
        }

        public void AddData(string uniqueKey, object data, string[] categories, int cacheDuration)
        {
            string key = getKey(uniqueKey);

            HttpContext.Current.Items[key] = data;

            //if (!HttpContext.Current.Items.Contains(key))
            //{
            //    HttpContext.Current.Items[key] = data;
            //}
            //else
            //{
            //    throw new Exception(string.Format("Item already exists with key '{0}'",key));
            //}                        
        }

        #endregion



    }


    public class SessionCache : ILoaderCache
    {
        static SessionCache cache = new SessionCache();

        public static SessionCache Current
        {
            get
            {
                return cache;
            }
        }

        private SessionCache()
        {

        }
        #region ILoaderCache Members

        public bool TryGetData<TData>(string uniqueKey, out TData data)
        {
            object value = HttpContext.Current.Session.Get(string.Format("SessionCache.{0}", uniqueKey));

            if (value is TData)
            {
                data = (TData)value;
                return true;
            }
            else
            {
                data = default(TData);
                return false;
            }

        }

        public void AddData(string uniqueKey, object data, string[] categories, int cacheDuration)
        {
            HttpContext.Current.Session.Set(string.Format("SessionCache.{0}", uniqueKey), data);
        }

        #endregion
    }
}
