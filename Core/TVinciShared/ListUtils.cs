using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace TVinciShared
{
    public static class ListUtils
    {
        /// <summary>
        /// TVinciShared Extension Method for paging
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="original"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="illegalRequest"></param>
        /// <returns></returns>
        public static IEnumerable<T> Page<T>(this IEnumerable<T> original, int pageSize, int pageIndex, out bool illegalRequest)
        {
            IEnumerable<T>  output = null;
            illegalRequest = false;

            if (pageSize < 0 || pageIndex < 0)
            {
                // illegal parameters
                illegalRequest = true;
            }
            else
            {
                if (pageSize == 0 && pageIndex == 0)
                {
                    // return all results
                    output = original;
                }
                else
                {
                    // apply paging on results
                    output = original.Skip(pageSize * pageIndex).Take(pageSize);
                }
            }

            return output;
        }

        public static Dictionary<string, string> ToDictionary(NameValueCollection nameValueCollection)
        {
            Dictionary<string, string> keyValues = new Dictionary<string, string>();
            foreach (var key in nameValueCollection.AllKeys)
            {
                if (!string.IsNullOrEmpty(key) && !keyValues.ContainsKey(key) && !string.IsNullOrEmpty(nameValueCollection[key]))
                {
                    keyValues.Add(key, nameValueCollection[key]);
                }
            }

            return keyValues;
        }

        public static bool TryAdd<T>(this Dictionary<string, T> current, string key, T value)
            where T : class
        {
            if (!value.Equals(default(T)) && !current.ContainsKey(key))
            {
                current.Add(key, value);
                return true;
            }

            return false;
        }

        public static bool TryAddRange<T>(this Dictionary<string, T> current, Dictionary<string, T> other)
            where T : class
        {
            if (other != null)
            {
                foreach (var item in other)
                {
                    if (!current.TryAdd(item.Key, item.Value))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public static void AddRange<T>(this Dictionary<string, T> current, Dictionary<string, T> other)
            where T : class
        {
            if (other != null)
            {
                foreach (var item in other)
                {
                    if (!current.ContainsKey(item.Key))
                    {
                        current.Add(item.Key, item.Value);
                    }
                }
            }
        }

#if NET48
        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key) => dict.TryGetValue(key, out var value) ? value : default(TValue);
#endif

        /// <summary>
        /// Indicates collection emptiness.
        /// </summary>
        /// <param name="collection"><see cref="IEnumerable{T}"/> collection.</param>
        /// <typeparam name="T">Generic type of collection.</typeparam>
        /// <returns>An <see cref="bool"/> which indicates whether collection is empty or not.</returns>
        public static bool IsEmpty<T>(this IEnumerable<T> collection)
        {
            return !collection.Any();
        }

        public static List<T> GetUpdatedValue<T>(this List<T> value, List<T> otherValue, ref bool needToUpdate)
        {
            if (value == null)
            {
                return otherValue;
            }

            if (otherValue != null)
            {
                var valueHasSet = new HashSet<T>(value);

                if (!valueHasSet.SetEquals(otherValue))
                {
                    needToUpdate = true;
                }
            }

            return value;
        }
    }
}
