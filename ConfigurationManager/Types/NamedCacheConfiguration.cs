using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationManager
{
    public class NamedCacheConfiguration : BaseCacheConfiguration
    {
        public StringConfigurationValue Name;

        public NamedCacheConfiguration(string key) : base(key)
        {
            Name = new StringConfigurationValue("name", this)
            {
                DefaultValue = "Cache"
            };
        }
    }
}
