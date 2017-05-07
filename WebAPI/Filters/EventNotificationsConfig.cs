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
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

                foreach (var setting in consumerSettings.Consumers)
                {
                    try
                    {
                        Assembly consumerAssembly = null;

                        // if we have a dll location defined
                        if (!string.IsNullOrEmpty(setting.DllLocation))
                        {
                            // First treat it as a full path
                            if (File.Exists(setting.DllLocation))
                            {
                                consumerAssembly = Assembly.LoadFrom(setting.DllLocation);
                            }
                            else
                            {
                                // Otherwise treat is as a relative path, and combine it with the base directory of the application
                                string combinedPath = string.Format("{0}{1}", baseDirectory, setting.DllLocation);

                                if (File.Exists(combinedPath))
                                {
                                    consumerAssembly = Assembly.LoadFrom(combinedPath);
                                }
                            }
                        }

                        // If we don't have a valid location, use calling assembly
                        if (consumerAssembly == null)
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
