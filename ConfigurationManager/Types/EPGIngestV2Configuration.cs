using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace ConfigurationManager
{
    public class EPGIngestV2Configuration : ConfigurationValue
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public NumericConfigurationValue LockNumOfRetries;
        public NumericConfigurationValue LockRetryIntervalMS;
        public NumericConfigurationValue LockTTLSeconds;

        // Default retry config is to try 6 time per minute for 10 minutes
        public EPGIngestV2Configuration(string key) : base(key)
        {
            ShouldAllowEmpty = true;

            LockNumOfRetries = new NumericConfigurationValue("lock_num_of_retries", this)
            {
                DefaultValue = 600,
                ShouldAllowEmpty = true
            };
            
            LockRetryIntervalMS = new NumericConfigurationValue("lock_retry_interval_ms", this)
            {
                DefaultValue = 10000, // 10 seconds,
                ShouldAllowEmpty = true
            };

            LockTTLSeconds = new NumericConfigurationValue("lock_ttl_seconds", this)
            {
                DefaultValue = 10800, // 3 hours,
                ShouldAllowEmpty = true
            };
        }

        internal override bool Validate()
        {
            bool result = true;
            result &= LockNumOfRetries.ObjectValue == null || LockNumOfRetries.IntValue > 0;
            result &= LockRetryIntervalMS.ObjectValue == null || LockRetryIntervalMS.IntValue > 0;
            result &= LockTTLSeconds.ObjectValue == null || LockTTLSeconds.IntValue > 0;

            if (LockTTLSeconds.ObjectValue != null && LockTTLSeconds.IntValue < 10800)
            {
                log.Warn($"Setting the distributed lock to less than 6 hours is not recommended, this is a failsafe mechanisms to prevent locking indefinitely");
            }

            return result;
        }
    }
}