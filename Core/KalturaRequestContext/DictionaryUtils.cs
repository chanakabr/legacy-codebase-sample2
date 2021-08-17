#if NETFRAMEWORK
using System.Collections;
#endif

namespace KalturaRequestContext
{
    internal static class DictionaryUtils
    {
#if NETFRAMEWORK
        // This is a shim method for .net452 to allow HttpContext.Current.Items.ContainsKey
        // This is because Items collection is Idictionary in net452 but in netCore its Idictionary<object,object>
        public static bool ContainsKey(this IDictionary dict, string key)
        {
            return dict.Contains(key);
        }
#endif
    }
}