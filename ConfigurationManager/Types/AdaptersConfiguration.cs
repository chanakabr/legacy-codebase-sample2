using ConfigurationManager.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace ConfigurationManager.Types
{
    public class AdaptersConfiguration : ConfigurationValue
    {
        public Dictionary<string, AdapterConfiguration> ConfigurationDictionary =
            new Dictionary<string, AdapterConfiguration>();
        private readonly JObject _Json;
        private const string DefaultConfigurationKey = "default";
        public AdaptersConfiguration(string key) : base(key)
        {
            string objectValue = Convert.ToString(ObjectValue);
            if (!string.IsNullOrEmpty(objectValue))
            {
                _Json = JObject.Parse(objectValue);
                ConfigurationDictionary = JsonConvert.DeserializeObject<Dictionary<string, AdapterConfiguration>>(_Json.ToString());
            }

            if (ConfigurationDictionary.TryGetValue(DefaultConfigurationKey, out var tcmConfiguration))
            {
                tcmConfiguration.CloseTimeout = tcmConfiguration.CloseTimeout ?? AdapterConfiguration.DefaultConfig.CloseTimeout;
                tcmConfiguration.MaxReceivedMessageSize = tcmConfiguration.MaxReceivedMessageSize ?? AdapterConfiguration.DefaultConfig.MaxReceivedMessageSize;
                tcmConfiguration.OpenTimeout = tcmConfiguration.OpenTimeout ?? AdapterConfiguration.DefaultConfig.OpenTimeout;
                tcmConfiguration.ReceiveTimeout = tcmConfiguration.ReceiveTimeout ?? AdapterConfiguration.DefaultConfig.ReceiveTimeout;
                tcmConfiguration.SendTimeout = tcmConfiguration.SendTimeout ?? AdapterConfiguration.DefaultConfig.SendTimeout;
            }
            else
            {
                ConfigurationDictionary[DefaultConfigurationKey] = AdapterConfiguration.DefaultConfig;
            }

            ShouldAllowEmpty = true;
        }

        internal override bool Validate()
        {
            try
            {
                base.Validate();
                var configuration = JsonConvert.DeserializeObject<Dictionary<string, AdapterConfiguration>>(_Json.ToString());
                var res = configuration["default"]; //verify default configuration exists
            }
            catch (Exception ex)
            {
                LogError($"failed to deserialized adapter configuration: {ex.Message}", ConfigurationValidationErrorLevel.Failure);

                return false;
            }
            return true;
        }
    }
}
