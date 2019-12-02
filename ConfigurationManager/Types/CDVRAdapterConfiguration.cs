using System;
using System.Collections.Generic;
using System.Text;
using ConfigurationManager.Types;
using ConfigurationManager.ConfigurationSettings.ConfigurationBase;

namespace ConfigurationManager
{
    public class CDVRAdapterConfiguration : BaseConfig<CDVRAdapterConfiguration>
    {
        public BaseValue<int> RetryInterval = new BaseValue<int>("retry_interval", 5);
        public BaseValue<int> MaximumRetriesAllowed = new BaseValue<int>("maximum_retries_allowed", 6);

        public override string TcmKey => TcmObjectKeys.CDVRAdapterConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };
    }
}
