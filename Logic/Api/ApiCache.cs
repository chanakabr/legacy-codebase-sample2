using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects;

namespace Core.Api
{
    public class ApiCache
    {
        private static bool Add(string key, object obj)
        {
            return TvinciCache.WSCache.Instance.Add(key, obj);
        }

        private static T Get<T>(string key)
        {
            return TvinciCache.WSCache.Instance.Get<T>(key);
        }

        private static bool TryGet<T>(string key, out T value)
        {
            return TvinciCache.WSCache.Instance.TryGet<T>(key, out value);
        }

        internal static bool GetList<T1>(string key, out List<T1> temp)
        {
            try
            {
                temp = Get<List<T1>>(key);
                if (temp != null)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                temp = null;
                return false;
            }
        }

        internal static bool AddList<T>(string key, List<T> ret)
        {  
            return ret != null && Add(key, ret);
        }

        internal static IDictionary<string, object> GetValues(List<string> lKeys)
        {
            IDictionary<string, object> dict = null;
            try
            {
                dict =  TvinciCache.WSCache.Instance.GetValues(lKeys);

                if (dict == null)
                {
                    dict = new Dictionary<string, object>();
                }
                // add all keys that are not exsits in cach with null value                 
                foreach (string key in lKeys)
                {
                    if (!dict.ContainsKey(key))
                    {
                        dict.Add(key, null);
                    }                    
                }
                
                return dict;
            }
            catch 
            {
                return null;
            }
        }

        internal static bool AddItem(string key, object obj)
        {
            return (!string.IsNullOrEmpty(key)) && Add(key, obj);
        }

        /// <summary>
        /// Gets an item from the cache of type T. Returns whether the key was found in the cache or not
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="key"></param>
        /// <param name="oValue"></param>
        /// <returns>True if the key was found, false otherwise</returns>
        internal static bool GetItem<T>(string key, out T oValue)
        {
            return TryGet<T>(key, out oValue);
        }


    }
}
