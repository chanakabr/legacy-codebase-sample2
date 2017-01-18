using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.ConditionalAccess
{
    public class ConditionalAccessCache
    {
        private static bool Add(string key, object obj)
        {
            return TvinciCache.WSCache.Instance.Add(key, obj);
        }

        private static T Get<T>(string key)
        {
            return TvinciCache.WSCache.Instance.Get<T>(key);
        }

        internal static bool AddItem(string key, object obj)
        {
            return (!string.IsNullOrEmpty(key)) && Add(key, obj);
        }

        internal static bool GetItem<T>(string key, out T oValue)
        {
            return TvinciCache.WSCache.Instance.TryGet<T>(key, out oValue);
        }
    }
}
