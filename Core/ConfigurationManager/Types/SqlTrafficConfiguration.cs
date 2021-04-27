using System;
using System.Collections.Generic;
using System.Text;
using ConfigurationManager.Types;
using ConfigurationManager.ConfigurationSettings.ConfigurationBase;

namespace ConfigurationManager
{
    public class SqlTrafficConfiguration : BaseConfig<SqlTrafficConfiguration>
    {
        public BaseValue<bool> ShouldUseTrafficHandler = new BaseValue<bool>("should_use_traffic_handler", false)
        {
            EnvironmentVariable = "SHOULD_USE_SQL_TRAFFIC_HANDLER"
        };

        public BaseValue<int> MaxInvalidationKeyStalenessInSeconds = new BaseValue<int>("max_invalidation_key_staleness_in_seconds", 3)
        {
            EnvironmentVariable = "SQL_TRAFFIC_HANDLER_MAX_INVALIDATION_KEY_STALENESS_SECONDS"
        };

        public override string TcmKey => TcmObjectKeys.SqlTrafficConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };
    }
}
