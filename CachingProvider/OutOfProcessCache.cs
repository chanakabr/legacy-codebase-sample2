using CouchbaseManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Enyim.Caching.Memcached;

namespace CachingProvider
{
    public abstract class OutOfProcessCache : ICachingService
    {       
     
        public abstract bool Add(string sKey, BaseModuleCache oValue, double nMinuteOffset);
     
        public abstract bool Set(string sKey, BaseModuleCache oValue, double nMinuteOffset);
     
        public abstract bool Add(string sKey, BaseModuleCache oValue);
     
        public abstract bool Set(string sKey, BaseModuleCache oValue);
     
        public abstract BaseModuleCache Get(string sKey);
     
        public abstract T Get<T>(string sKey) where T : class;
     
        public abstract BaseModuleCache Remove(string sKey);

        public abstract BaseModuleCache GetWithVersion<T>(string sKey);

        public abstract bool AddWithVersion<T>(string sKey, BaseModuleCache oValue);

        public abstract bool AddWithVersion<T>(string sKey, BaseModuleCache oValue, double nMinuteOffset);

        public abstract bool SetWithVersion<T>(string sKey, BaseModuleCache oValue, double nMinuteOffset);

        public abstract bool SetWithVersion<T>(string sKey, BaseModuleCache oValue);

        public abstract IDictionary<string, object> GetValues(List<string> keys, bool asJson = false);

        public abstract bool SetJson<T>(string sKey, T obj, double dCacheTT);

        public abstract bool GetJsonAsT<T>(string sKey, out T res) where T : class;
    }
}
