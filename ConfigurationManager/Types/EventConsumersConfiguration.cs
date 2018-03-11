using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationManager
{
    public class EventConsumersConfiguration : StringConfigurationValue
    {
        private static JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.None
        };

        public EventConsumersConfiguration(string key) : base(key)
        {
        }

        internal override bool Validate()
        {
            bool result = true;

            try
            {
                ConsumerSettings consumerSettings = null;

                if (this.ObjectValue != null)
                {
                    consumerSettings = Newtonsoft.Json.JsonConvert.DeserializeObject<ConsumerSettings>(this.ObjectValue.ToString(), serializerSettings);
                }
            }

            catch (Exception ex)
            {
                LogError(string.Format("Could not parse event consumers configuration. Error = {0}", ex));
                result = false;
            }

            return result;
        }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class ConsumerSettings
    {
        [JsonProperty("Consumers")]
        public List<ConsumerDefinition> Consumers
        {
            get;
            set;
        }

    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class ConsumerDefinition
    {
        [JsonProperty("DllLocation")]
        public string DllLocation
        {
            get;
            set;
        }

        [JsonProperty("Type")]
        public string Type
        {
            get;
            set;
        }
    }
}
