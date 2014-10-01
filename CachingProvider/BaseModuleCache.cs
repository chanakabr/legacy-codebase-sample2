using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CachingProvider
{
    public class BaseModuleCache
    {
        public object result {get;set;}
        public BaseModuleCache()
        {
        }

        public BaseModuleCache(object oResult)
        {
            this.result = oResult;
        }      
    }
}
