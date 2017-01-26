using CouchbaseManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public abstract List<string> GetKeys();

        public abstract bool Get<T>(string key, ref T result);

        public abstract bool GetWithVersion<T>(string key, out ulong version, ref T result);

        public abstract bool RemoveKey(string key);

        public abstract bool Add<T>(string key, T value, uint expirationInSeconds);

        public abstract bool SetWithVersion<T>(string key, T value, ulong version, uint expirationInSeconds);
    }
}
