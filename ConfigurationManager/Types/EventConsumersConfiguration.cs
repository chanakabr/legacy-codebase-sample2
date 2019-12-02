using System;
using System.Collections.Generic;
using System.Linq;
using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConfigurationManager
{
    public class EventConsumersConfiguration : BaseConfig<EventConsumersConfiguration>
    {
        public override string TcmKey => TcmObjectKeys.EventConsumersConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };

        public ConsumerSettings ConsumerSettings = new ConsumerSettings();


        public void SetValues(JToken token, ConsumerSettings defaultSettings)
        {

            try
            {

                var res = JsonConvert.DeserializeObject<ConsumerSettings>(token.ToString());
                if(res != null)
                {
                    ConsumerSettings = res;
                }

            }
            catch (Exception ex)
            {
                _Logger.Error(string.Format("Could not parse event consumers configuration. Error = {0}", ex));
            }
        }
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

