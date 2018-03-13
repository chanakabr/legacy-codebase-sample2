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
        }
    }
}
