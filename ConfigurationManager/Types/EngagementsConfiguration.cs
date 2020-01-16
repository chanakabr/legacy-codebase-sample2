using System;
using System;
using System.Collections.Generic;
using System.Text;
using ConfigurationManager.Types;
using ConfigurationManager.ConfigurationSettings.ConfigurationBase;

namespace ConfigurationManager
{
    public class EngagementsConfiguration : BaseConfig<EngagementsConfiguration>
    {
        public BaseValue<int> UserEngagementsTTLDays = new BaseValue<int>("user_engagements_ttl_days", 30);
        public BaseValue<int> NumberOfBulkMessageEngagements = new BaseValue<int>("num_of_bulk_message_engagements", 500);
        public BaseValue<int> NumberOfEngagementThreads = new BaseValue<int>("num_of_engagement_threads", 5);

        public override string TcmKey => TcmObjectKeys.EngagementsConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };
    }
}
