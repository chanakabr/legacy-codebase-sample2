using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationManager
{
    public class CDVRAdapterConfiguration : ConfigurationValue
    {
        public NumericConfigurationValue RetryInterval;
        public NumericConfigurationValue MaximumRetriesAllowed;

        public CDVRAdapterConfiguration(string key) : base(key)
        {
            RetryInterval = new ConfigurationManager.NumericConfigurationValue("retry_interval", this)
            {
                ShouldAllowEmpty = true,
                DefaultValue = 5,
                OriginalKey = "CDVRAdapterRetryInterval"
            };

            MaximumRetriesAllowed = new NumericConfigurationValue("maximum_retries_allowed", this)
            {
                ShouldAllowEmpty = true,
                DefaultValue = 6,
                OriginalKey = "CDVRAdapterMaximumRetriesAllowed"
            };
        }
    }
}
