using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConfigurationManager
{
    public abstract class MemoryCacheConfiguration : BaseConfig<MemoryCacheConfiguration>
    {
        public BaseValue<long> CacheMemoryLimit = new BaseValue<long>("cache_memory_limit_megabytes", 0, false, "amount of memory on the computer, in megabytes, that can be used by the cache.");
        public BaseValue<int> PollingIntervalSeconds  = new BaseValue<int>("polling_interval_seconds", 0, false, 
            "value that indicates the time interval after which the cache implementation compares the current memory load " +
            "against the absolute and percentage-based memory limits that are set for the cache instance.");
        
    }

    public class LayeredCacheInMemoryCacheConfiguration : MemoryCacheConfiguration
    {
        public override string TcmKey => TcmObjectKeys.LayeredCacheInMemoryCacheConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };

        public LayeredCacheInMemoryCacheConfiguration()
        {
            this.CacheMemoryLimit.EnvironmentVariable = "LAYERED_CACHE_IN_MEMORY_LIMIT_MB";
        }
    }

    public class GeneralInMemoryCacheConfiguration : MemoryCacheConfiguration
    {
        public override string TcmKey => TcmObjectKeys.GeneralInMemoryCacheConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };

        public GeneralInMemoryCacheConfiguration()
        {
            this.CacheMemoryLimit.EnvironmentVariable = "GENERAL_IN_MEMORY_CACHE_LIMIT_MB";
        }
    }
}
