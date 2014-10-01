using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CachingProvider
{
    public class VersionModuleCache : BaseModuleCache
    {
        public string version { get; set; }

        public VersionModuleCache()
            : base()
        {
            version = string.Empty;
        }

        public VersionModuleCache(object oResult, string sVersion)
            : base(oResult)
        {
            this.version = sVersion;
        }
    }
}
