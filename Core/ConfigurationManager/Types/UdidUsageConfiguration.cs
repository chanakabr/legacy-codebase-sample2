using System;
using ConfigurationManager.ConfigurationSettings.ConfigurationBase;


namespace ConfigurationManager.Types
{
    public class UdidUsageConfiguration: BaseConfig<UdidUsageConfiguration>
    {
        public override string TcmKey => TcmObjectKeys.UdidUsageConfiguration;
        public override string[] TcmPath => new string[] { TcmKey };

        public BaseValue<uint> TTL = new BaseValue<uint>("ttl_seconds", 157680000 /*5 years*/);

        public BaseValue<string> BucketName = new BaseValue<string>("bucket_name", "OTT_Apps");
    }
}