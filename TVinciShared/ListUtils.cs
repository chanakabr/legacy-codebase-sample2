using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace TVinciShared
{
    public class ListUtils
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
        public static IEnumerable<T> Page<T>(IEnumerable<T> original, int pageSize, int pageIndex, out bool illegalRequest)
        {
            IEnumerable<T> output = null;

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
    }
}
