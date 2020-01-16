using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using Newtonsoft.Json.Linq;
using System;

namespace ConfigurationManager
{
    public class OTTUserControllerConfiguration : BaseConfig<OTTUserControllerConfiguration>
    {
        public BaseValue<string> UserIdEncryptionKey = new BaseValue<string>("user_id_encryption_key", "L3CDpYFfCrGnx5ACoO/Az3oIIt/XeC2dhSmFcB6ckxA=");
        public BaseValue<string> UserIdEncryptionIV = new BaseValue<string>("user_id_encryption_iv", "Yn5/n0s8B0yLRvGuhSLRrA==");

        public override string TcmKey => TcmObjectKeys.OTTUserControllerConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };


    }
}