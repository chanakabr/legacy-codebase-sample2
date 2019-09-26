using ConfigurationManager.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace ConfigurationManager.Types
{
    public class AdaptersConfiguration : ConfigurationValue
    {
        public Dictionary<string, AdapterConfiguration> configurationDictionary;
        JObject json;
        public AdaptersConfiguration(string key) : base(key)
        {
            string objectValue = Convert.ToString(ObjectValue);
            if (!string.IsNullOrEmpty(objectValue))
            {
                json = JObject.Parse(objectValue);
                configurationDictionary = JsonConvert.DeserializeObject<Dictionary<string, AdapterConfiguration>>(json.ToString());
            }

        }

        internal override bool Validate()
        {
            try
            {
                base.Validate();
                var configuration =  JsonConvert.DeserializeObject<Dictionary<string, AdapterConfiguration>>(json.ToString());
                var res = configuration["default"]; //verify defult configuration exists
            }
            catch (Exception ex)
            {
                LogError($"failed to deserilized adapter configuration: {ex.Message}", ConfigurationValidationErrorLevel.Failure);

                return false;
            }
            return true;
        }
    }
}