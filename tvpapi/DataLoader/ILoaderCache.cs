using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tvinci.Data.DataLoader
{    
    public interface ILoaderCache
    {
        bool TryGetData<TData>(string uniqueKey, out TData data);
        //void AddData(string uniqueKey, object data, string[] categories);
        void AddData(string uniqueKey, object data, string[] categories, int cacheDuration);
        //bool TryGetData<TData>(ILoaderAdapter adapter, out TData data);
        //void AddData(ILoaderAdapter adapter, object data);
    }
}
