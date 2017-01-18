using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Billing
{
    public class BillingCache
    {
        private static bool Add(string key, object obj)
        {
            return TvinciCache.WSCache.Instance.Add(key, obj);
        }
        private static bool Add(string key, object obj, double nMinuteOffset)
        {
            return TvinciCache.WSCache.Instance.Add(key, obj, nMinuteOffset);
        }

        private static T Get<T>(string key)
        {
            return TvinciCache.WSCache.Instance.Get<T>(key);
        }

        internal static bool AddItem(string key, object obj)
        {
            return (!string.IsNullOrEmpty(key)) && Add(key, obj);
        }

        internal static bool AddItem(string key, object obj, double nMinuteOffset)
        {
            return (!string.IsNullOrEmpty(key)) && Add(key, obj, nMinuteOffset);
        }

        /// <summary>
        /// Gets an item from the cache of type T. Returns whether the key was found in the cache or not
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="p_sKey"></param>
        /// <param name="p_oValue"></param>
        /// <returns>True if the key was found, false otherwise</returns>
        internal static bool GetItem<T>(string p_sKey, out T p_oValue)
        {
            return TvinciCache.WSCache.Instance.TryGet<T>(p_sKey, out p_oValue);
        }
    }
}
