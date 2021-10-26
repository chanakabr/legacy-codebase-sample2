using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace ConfigurationManager
{
    public class EPGIngestV2Configuration : BaseConfig<EPGIngestV2Configuration>
    {
        public BaseValue<int> LockNumOfRetries = new BaseValue<int>("lock_num_of_retries", 600);
        public BaseValue<int> LockRetryIntervalMS = new BaseValue<int>("lock_retry_interval_ms", 10000);
        public BaseValue<int> LockTTLSeconds = new BaseValue<int>("lock_ttl_seconds", 10800);

        public override string TcmKey => TcmObjectKeys.EPGIngestV2Configuration;

        public override string[] TcmPath => new string[] { TcmKey };


        public override bool Validate()
        {
            var isValid = LockNumOfRetries.ActualValue > 0;
            isValid &= LockRetryIntervalMS.ActualValue > 0;
            isValid &= LockTTLSeconds.ActualValue > 0;

            // TODO: Check with lior if its okay to add klogger and print additional logs to this chekc .. 
            //if (LockTTLSeconds.ActualValue < 10800)
            //{
                //log.Warn($"Setting the distributed lock to less than 6 hours is not recommended, this is a failsafe mechanisms to prevent locking indefinetlly");
            //}

            return isValid;
        }
    }
}