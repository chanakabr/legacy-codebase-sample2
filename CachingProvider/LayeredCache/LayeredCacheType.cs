using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CachingProvider.LayeredCache
{
    public enum LayeredCacheType
    {
        None = 0,
        InMemoryCache = 1,
        CbCache = 2,
        CbMemCache = 3
    }  
}
