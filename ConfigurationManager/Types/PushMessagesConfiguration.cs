using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationManager
{
    public class PushMessagesConfiguration : ConfigurationValue
    {

        public NumericConfigurationValue NumberOfMessagesPerSecond;
        public NumericConfigurationValue TTLSeconds;

        public PushMessagesConfiguration(string key) : base(key)
        {
            NumberOfMessagesPerSecond = new NumericConfigurationValue("num_of_messages_per_second", this)
            {
                DefaultValue = 1
            };
            TTLSeconds = new NumericConfigurationValue("ttl_seconds", this)
            {
                DefaultValue = 30
            };
        }
    }
}
