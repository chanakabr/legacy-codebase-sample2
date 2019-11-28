using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace ConfigurationManager.Types
{
    public class AdaptersConfiguration : BaseConfig<AdaptersConfiguration>
    {
        private  static readonly AdapterConfiguration defaultAdapterConfig = new AdapterConfiguration();
        

        public override string TcmKey => TcmObjectKeys.AdaptersConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };


        public Dictionary<string, AdapterConfiguration> ConfigurationDictionary
            = new Dictionary<string, AdapterConfiguration>()
            {
                {TcmObjectKeys.DefaultAdapterConfigurationKey, defaultAdapterConfig }
            };

        

        public void SetValues(JToken token, Dictionary<string, AdapterConfiguration> defaultData)
        {
            AdapterConfiguration defaultConfig = defaultData[TcmObjectKeys.DefaultAdapterConfigurationKey];
            JObject tokenConfiguration = JObject.Parse(token.ToString());
            foreach (KeyValuePair<string, JToken> pair in tokenConfiguration)
            {
                if (ConfigurationDictionary.TryGetValue(pair.Key,  out var currentConfig))
                {
                    UpdateAdapterConfigurationAccordingToTcm(pair.Value, currentConfig);
                }
                else
                {
                    AdapterConfiguration newConfig = defaultConfig.DeepCopy();
                    UpdateAdapterConfigurationAccordingToTcm(pair.Value, newConfig);
                    ConfigurationDictionary.Add(pair.Key, newConfig);
                }
            }
        }


        private void UpdateAdapterConfigurationAccordingToTcm(JToken token, AdapterConfiguration config)
        {
            base.SetActualValue(token, config.CloseTimeout);
            base.SetActualValue(token, config.MaxReceivedMessageSize);
            base.SetActualValue(token, config.OpenTimeout);
            base.SetActualValue(token, config.ReceiveTimeout);
            base.SetActualValue(token, config.SendTimeout);
        }
        
    }
}
