using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Enyim.Caching.Memcached;

namespace CachingProvider
{
    public interface ICachingService
    {
        bool Add(string sKey, BaseModuleCache oValue, double nMinuteOffset);
        bool Add(string sKey, BaseModuleCache oValue);
        bool Set(string sKey, BaseModuleCache oValue, double nMinuteOffset);
        bool Set(string sKey, BaseModuleCache oValue);
        BaseModuleCache Get(string sKey);
        BaseModuleCache Remove(string sKey);

        BaseModuleCache GetWithVersion<T>(string sKey);
        bool AddWithVersion<T>(string sKey, BaseModuleCache oValue);
        bool AddWithVersion<T>(string sKey, BaseModuleCache oValue, double nMinuteOffset);
        bool SetWithVersion<T>(string sKey, BaseModuleCache oValue, double nMinuteOffset);      
        
        T Get<T>(string sKey) where T : class;

        IDictionary<string, object> GetValues(List<string> keys, bool asJson = false);


        bool SetJson<T>(string sKey, T obj, double dCacheTT);


        bool GetJsonAsT<T>(string sKey, out T res) where T : class;
    }
}
