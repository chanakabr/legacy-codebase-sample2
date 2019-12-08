using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace ConfigurationManager.Types
{
    public class AdaptersConfiguration : BaseConfig<AdaptersConfiguration>
    {
        private  static readonly AdapterConfiguration defaultAdapterConfig = new AdapterConfiguration();
        
        public override string TcmKey => TcmObjectKeys.AdaptersConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };


        private static readonly Dictionary<string, AdapterConfiguration> defaultConfigurationDictionary
            = new Dictionary<string, AdapterConfiguration>()
            {
                {TcmObjectKeys.DefaultConfigurationKey, defaultAdapterConfig }
            };

        public BaseValue<Dictionary<string, AdapterConfiguration>> ConfigurationDictionary = new BaseValue<Dictionary<string, AdapterConfiguration>>(string.Empty, defaultConfigurationDictionary);



        public override void SetActualValue<TV>(JToken token, BaseValue<TV> defaultData)
        {
            var defaltAdapterConfiguration = defaultData.DefaultValue as Dictionary<string, AdapterConfiguration>;
            Dictionary<string, AdapterConfiguration> actualValue = new Dictionary<string, AdapterConfiguration>();
            if (token == null)
            {
                _Logger.Info($"Empty data in TCM under object:  [{GetType().Name}]  for key [{TcmKey}], setting default value as actual value");
                return;
            }
            AdapterConfiguration defaultConfig = defaltAdapterConfiguration[TcmObjectKeys.DefaultConfigurationKey];
            JObject tokenConfiguration = JObject.Parse(token.ToString());
            var defaultTokenData = tokenConfiguration[TcmObjectKeys.DefaultConfigurationKey];
            UpdateAdapterConfigurationAccordingToTcm(defaultTokenData, defaultConfig);

            actualValue.Add(TcmObjectKeys.DefaultConfigurationKey, defaultConfig);

            foreach (KeyValuePair<string, JToken> pair in tokenConfiguration)
            {
                if (pair.Key == TcmObjectKeys.DefaultConfigurationKey)
                {
                    continue;//already init at the top 
                }

                if (defaltAdapterConfiguration.TryGetValue(pair.Key, out var currentConfig))
                {
                    actualValue.Add(pair.Key, currentConfig);
                }
                else
                {
                    AdapterConfiguration newConfig = AdapterConfiguration.Copy(defaultConfig);
                    actualValue.Add(pair.Key, newConfig);
                }

                UpdateAdapterConfigurationAccordingToTcm(pair.Value, actualValue[pair.Key]);
            }

            SetActualValue(defaultData  as BaseValue<Dictionary<string, AdapterConfiguration>>, actualValue);
        }



        private Dictionary<string, AdapterConfiguration> DeepClone(Dictionary<string, AdapterConfiguration> defaltAdapterConfiguration)
        {
            Dictionary<string, AdapterConfiguration> res = new Dictionary<string, AdapterConfiguration>(defaltAdapterConfiguration.Count,
                                                            defaltAdapterConfiguration.Comparer);
            foreach (KeyValuePair<string, AdapterConfiguration> entry in defaltAdapterConfiguration)
            {
                res.Add(entry.Key, AdapterConfiguration.Copy(entry.Value));
            }
            return res;
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
