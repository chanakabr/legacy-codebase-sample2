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

        // fix GEN-840
        private static readonly ConsumerDefinition defaultWindowsConsumerSettings = new ConsumerDefinition() { DllLocation = "bin\\WebAPI.dll", Type = "WebAPI.RestNotificationEventConsumer" };
        private static readonly ConsumerDefinition defaultLinuxConsumerSettings = new ConsumerDefinition() { DllLocation = "WebAPI.dll", Type = "WebAPI.RestNotificationEventConsumer" };

        // fix GEN-840
        public BaseValue<List<ConsumerDefinition>> ConsumerSettings = new BaseValue<List<ConsumerDefinition>>(TcmObjectKeys.ConsumerSettings, 
                                                                        new List<ConsumerDefinition>() { defaultWindowsConsumerSettings, defaultLinuxConsumerSettings });


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

