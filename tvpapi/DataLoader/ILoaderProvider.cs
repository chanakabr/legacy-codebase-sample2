using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Tvinci.Data.DataLoader
{
    public interface ILoaderProvider
    {
        object GetDataFromSource(ILoaderAdapter adapter);        
        //object GetDataFromSource(ILoaderAdapter adapter, eCacheMode cacheMode);        
    }
}
