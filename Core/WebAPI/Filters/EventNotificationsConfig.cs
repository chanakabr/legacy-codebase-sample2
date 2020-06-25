using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using EventManager;
using KLogMonitor;
using System.IO;
using ConfigurationManager;
using Microsoft.Extensions.Hosting;

namespace WebAPI.Filters
{
    public static class EventNotificationsConfig
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static IHostBuilder ConfigureEventNotificationsConfig(this IHostBuilder builder)
        {
            builder.ConfigureServices((hostContext, services) =>
            {
                ApplicationConfiguration.Init();

                SubscribeConsumers();
            });

            return builder;
        }

        public static void SubscribeConsumers()
        {
            List<ConsumerDefinition> consumerSettings = ApplicationConfiguration.Current.EventConsumersConfiguration.ConsumerSettings.Value;
            HashSet<string> consumedTypes = new HashSet<string>();
            if (consumerSettings != null && consumerSettings.Any())
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

                foreach (var setting in consumerSettings)
                {
                    // fix GEN-840
                    if (!consumedTypes.Contains(setting.Type))
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

                                if (consumerAssembly != null)
                                {
                                    log.DebugFormat("Loading event notification consumer from assembly {0} in location {1} and type {2}",
                                        consumerAssembly.FullName, consumerAssembly.Location, setting.Type);

                                    Type consumerType = consumerAssembly.GetType(setting.Type);

                                    var newConsumer = (BaseEventConsumer)Activator.CreateInstance(consumerType);

                                    EventManager.EventManager.Subscribe(newConsumer);
                                    consumedTypes.Add(setting.Type);
                                }
                            }
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
    }


}
