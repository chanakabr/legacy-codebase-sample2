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
            // if this is a sub-configuration value, the second consturctor should be used, sending "this" as a parent
            BulkSize = new ConfigurationManager.NumericConfigurationValue("bulk_size", this) 
            {
                // Since this is a remote tasks configuration value, it should be empty on phoenix, which is the component that is validated
                ShouldAllowEmpty = true,
                // default value can be set, but is not a must.
                DefaultValue = 50,
                // descriptions are important for inetgrators, they help them understand what is the meaning of this key
                Description = "Number of documents to be updated in same ElasticSearch bulk when rebuilding the index. " +
                "This value can be several hundreds, depending on typical document size and machine capabilities"
            };

            NumberOfShards = new ConfigurationManager.NumericConfigurationValue("shards", this)
            {
                ShouldAllowEmpty = true
            };

            NumberOfReplicas = new NumericConfigurationValue("replicas", this)
            {
                ShouldAllowEmpty = true
            };

            ChannelStartDateDays = new ConfigurationManager.NumericConfigurationValue("channel_start_date_days", this)
            {
                ShouldAllowEmpty = true,
                DefaultValue = 30,
                Description = "Original key is Channel_StartDate_Days. Used in EPG Channel updater (when getting programs by channel Ids and dates)"
            };
        }
    }
}
