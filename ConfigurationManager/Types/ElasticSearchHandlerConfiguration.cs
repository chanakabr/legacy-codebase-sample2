using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationManager
{
    public class ElasticSearchHandlerConfiguration : ConfigurationValue
    {
        public NumericConfigurationValue BulkSize;
        public NumericConfigurationValue NumberOfShards;
        public NumericConfigurationValue NumberOfReplicas;
        public NumericConfigurationValue ChannelStartDateDays;

        public ElasticSearchHandlerConfiguration(string key) : base(key)
        {
            BulkSize = new ConfigurationManager.NumericConfigurationValue("bulk_size")
            {
                ShouldAllowEmpty = true,
                DefaultValue = 50
            };

            NumberOfShards = new ConfigurationManager.NumericConfigurationValue("shards")
            {
                ShouldAllowEmpty = true
            };

            NumberOfReplicas = new NumericConfigurationValue("replicas")
            {
                ShouldAllowEmpty = true
            };

            ChannelStartDateDays = new ConfigurationManager.NumericConfigurationValue("channel_start_date_days")
            {
                ShouldAllowEmpty = true,
                DefaultValue = 30,
                Description = "Original key is Channel_StartDate_Days. Used in EPG Channel updater (when getting programs by channel Ids and dates)"
            };
        }
    }
}
