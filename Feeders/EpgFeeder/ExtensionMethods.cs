using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpgFeeder
{
    public static class ExtensionMethods
    {
        public static void AddRange<T, S>(this Dictionary<DateTime, List<int>> source, Dictionary<DateTime, List<int>> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("Collection is null");
            }

            foreach (var item in collection)
            {
                if (!source.ContainsKey(item.Key))
                {
                    source.Add(item.Key, item.Value);
                }
                else
                {
                    source[item.Key].AddRange(item.Value);// handle duplicate key issue here
                }
            }
        }
    }
}
