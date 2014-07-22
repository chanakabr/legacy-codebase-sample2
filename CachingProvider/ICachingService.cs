using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CachingProvider
{
    public interface ICachingService
    {
        bool Add(string sKey, object oValue, double nMinuteOffset);
        bool Add(string sKey, object oValue);
        bool Set(string sKey, object oValue, double nMinuteOffset);
        bool Set(string sKey, object oValue);
        object Get(string sKey);
        object Remove(string sKey);
        T Get<T>(string sKey) where T : class;
    }
}
