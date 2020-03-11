using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Configuration.ProvideConfiguration;
using System.IO;
using KLogMonitor;
using System.Reflection;

namespace Tvinci.Configuration
{
    public class ItemAddedEventArgs<TItem> : EventArgs
    {
        public string Identifier { get; set; }
        public TItem Item { get; set; }
    }

    public class InstanceProvider<TItem>
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        Dictionary<string, TItem> m_items = new Dictionary<string, TItem>(new Tvinci.Helpers.CompareCaseInSensitive());

        public EventHandler<ItemAddedEventArgs<TItem>> ItemAddedEvent { get; set; }

        public TItem this[string identifier]
        {
            get
            {
                return m_items[identifier];
            }
        }

        public void RemoveItem(string identifier)
        {
            if (m_items.ContainsKey(identifier))
            {
                m_items.Remove(identifier);
            }
        }

        public void AddItem(string identifier, TItem item)
        {
            if (m_items.ContainsKey(identifier))
            {
                string message = string.Format("item with identifier '{0}' already exists. operation aborted", identifier);
                logger.ErrorFormat("Error occured while adding new Item to provider. {0}", message);
                throw new Exception(message);
            }

            m_items.Add(identifier, item);

            if (ItemAddedEvent != null)
            {
                ItemAddedEvent(this, new ItemAddedEventArgs<TItem> { Identifier = identifier, Item = item });
            }
        }
    }


    public class ConfigurationProvider<TConfiguration> : InstanceProvider<TConfiguration> where TConfiguration : class, ISupportProvider
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        bool m_isSynced = false;

        public void SyncFromIndexFile(string configurationPath, bool isIndexEncrypted, bool isItemsEncrypted)
        {
            if (m_isSynced)
            {
                // Programmer notice - if you want to add this feature remeber to treat multiple-thread environment
                throw new Exception("Currently not supported multiple synchronization");
            }

            m_isSynced = true;

            logger.InfoFormat("Start syncing provider from index file '{0}'", configurationPath);

            ProvideConfiguration.ProviderConfiguration configuration = ConfigurationHelper.ExtractFromFile<ProvideConfiguration.ProviderConfiguration>(configurationPath, isIndexEncrypted);

            if (configuration != null)
            {
                logger.InfoFormat("Found {0} configuration items", configuration.Count);

                foreach (Item item in configuration)
                {
                    try
                    {
                        TConfiguration instance = Activator.CreateInstance(typeof(TConfiguration), true) as TConfiguration;

                        if (instance != null)
                        {
                            logger.DebugFormat("Syncing configuration of item '{0}' from path '{1}", item.ID, item.VirtualPath);
                            instance.SyncFromConfigurationFile(item.VirtualPath);

                            base.AddItem(item.ID, instance);
                            logger.DebugFormat("Item with identifier '{0}' added to provider", item.ID);
                        }
                        else
                        {
                            string message = string.Format("Failed to create instance of type '{0}'. Make sure the class implement interface '{1}'. operation aborted", typeof(TConfiguration), typeof(ISupportProvider));
                            logger.ErrorFormat("Error occured while syncing from index file '{0}'. {1}", configurationPath, message);
                            throw new Exception(message);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(string.Format("Error occured while syncing from index file '{0}'", configurationPath), ex);
                        throw;
                    }
                }
            }
            else
            {
                string message = string.Format("Failed to extract index configuration from file. operation aborted", configurationPath);
                logger.ErrorFormat("Error occured while syncing from index file '{0}'. {1}", configurationPath, message);
                throw new Exception(message);
            }
        }
    }
}