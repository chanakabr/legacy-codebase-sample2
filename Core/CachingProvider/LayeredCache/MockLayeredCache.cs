using System;
using System.Collections.Generic;

namespace CachingProvider.LayeredCache
{
    /// <summary>
    /// This is a utility class for injecting a layered cache implementation
    /// that will always call the factory method.
    /// Use strictly for tests.
    /// </summary>
    public class MockLayeredCache : ILayeredCache
    {
        public bool Get<T1>(string key, ref T1 genericParameter, Func<Dictionary<string, object>, Tuple<T1, bool>> fillObjectMethod, Dictionary<string, object> funcParameters, int groupId, string layeredCacheConfigName, List<string> inValidationKeys = null, bool shouldUseAutoNameTypeHandling = false)
        {
            var res = fillObjectMethod(funcParameters);
            genericParameter = res.Item1;
            return res.Item2;
        }

        public bool SetInvalidationKey(string key, DateTime? updatedAt = null)
        {
            throw new NotImplementedException("MockLayeredCache dose not implement this method");
        }

        public long GetInvalidationKeyValue(int groupId, string layeredCacheConfigName, string invalidationKey)
        {
            throw new NotImplementedException("MockLayeredCache dose not implement this method");
        }

        public bool GetValues<T>(Dictionary<string, string> keyToOriginalValueMap, ref Dictionary<string, T> results, Func<Dictionary<string, object>, Tuple<Dictionary<string, T>, bool>> fillObjectsMethod,
            Dictionary<string, object> funcParameters, int groupId, string layeredCacheConfigName, Dictionary<string, List<string>> inValidationKeysMap = null,
            bool shouldUseAutoNameTypeHandling = false)
        {
            throw new NotImplementedException();
        }
    }
}