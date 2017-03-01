using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using EventManager;
using KLogMonitor;
using System.IO;

namespace WebAPI.Filters
{
    public class EventNotificationsConfig
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static void SubscribeConsumers()
        {
            ConsumerSettings consumerSettings = null;

            try
            {
                object consumerSettingsJson = TCMClient.Settings.Instance.GetValue<object>("ConsumerSettings");
                consumerSettings = Newtonsoft.Json.JsonConvert.DeserializeObject<ConsumerSettings>(consumerSettingsJson.ToString());
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed reading TCM value of event consumer settings: {0}", ex);
            }

            if (consumerSettings != null && consumerSettings.Consumers != null)
            {
                foreach (var setting in consumerSettings.Consumers)
                {
                    try
                    {
                        Assembly consumerAssembly = null;

                        if (!string.IsNullOrEmpty(setting.DllLocation) && File.Exists(setting.DllLocation))
                        {
                            consumerAssembly = Assembly.LoadFrom(setting.DllLocation);
                        }
                        else
                        {
                            consumerAssembly = Assembly.GetCallingAssembly();
                        }

                        Type consumerType = consumerAssembly.GetType(setting.Type);

                        var newConsumer = (BaseEventConsumer)Activator.CreateInstance(consumerType);

                        EventManager.EventManager.Subscribe(newConsumer);
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Failed loading specific consumer from assembly. location = {0}, type = {1}, ex = {2}",
                            setting.DllLocation, setting.Type, ex);
                    }
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
}
