using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;

namespace CachingProvider
{
    /*
     * 1. This class is deprecated. Use SingleInMemoryCache instead.
     * 2. Reason for deprecation: It used multiple MemoryCache objects. This is not recommended by Microsoft.
     * 3. MemoryCache object is a heavy object. 
     */ 
   /* public class InMemoryCache : ICachingService
    {

        private InMemoryCache(string sCacheName)
        {
            throw new NotImplementedException("Deprecated. Use SingleInMemoryCache instead.");
        }

        private void LogError(string methodName, string key, object value, Exception ex, double? minOffset = null)
        {
            throw new NotImplementedException("Deprecated. Use SingleInMemoryCache instead.");
        }

        public bool Add(string sKey, object oValue, double nMinuteOffset)
        {
            throw new NotImplementedException("Deprecated. Use SingleInMemoryCache instead.");
        }

        public bool Add(string sKey, object oValue)
        {
            throw new NotImplementedException("Deprecated. Use SingleInMemoryCache instead.");
        }

        public object Remove(string sKey)
        {
            throw new NotImplementedException("Deprecated. Use SingleInMemoryCache instead.");
        }

        public bool Set(string sKey, object oValue, double nMinuteOffset)
        {
            throw new NotImplementedException("Deprecated. Use SingleInMemoryCache instead.");
        }

        public bool Set(string sKey, object oValue)
        {
            throw new NotImplementedException("Deprecated. Use SingleInMemoryCache instead.");
        }

        public object Get(string sKey)
        {
            throw new NotImplementedException("Deprecated. Use SingleInMemoryCache instead.");
        }

        public T Get<T>(string sKey) where T : class
        {
            throw new NotImplementedException("Deprecated. Use SingleInMemoryCache instead.");
        }

        public static InMemoryCache GetInstance(string sCacheName)
        {
            throw new NotImplementedException("Deprecated. Use SingleInMemoryCache instead.");
        }
    }
    */
}
