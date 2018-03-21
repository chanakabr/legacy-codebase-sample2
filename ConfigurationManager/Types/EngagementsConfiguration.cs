using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationManager
{
    public class EngagementsConfiguration : ConfigurationValue
    {
        public NumericConfigurationValue UserEngagementsTTLDays;
        public NumericConfigurationValue NumberOfBulkMessageEngagements;
        public NumericConfigurationValue NumberOfEngagementThreads;

        public EngagementsConfiguration(string key) : base(key)
        {
            UserEngagementsTTLDays = new NumericConfigurationValue("user_engagements_ttl_days", this)
            {
                DefaultValue = 30
            };

            NumberOfBulkMessageEngagements = new NumericConfigurationValue("num_of_bulk_message_engagements", this)
            {
                DefaultValue = 500
            };
            NumberOfEngagementThreads = new NumericConfigurationValue("num_of_engagement_threads", this)
            {
                DefaultValue = 5
            };
        }
    }
}
