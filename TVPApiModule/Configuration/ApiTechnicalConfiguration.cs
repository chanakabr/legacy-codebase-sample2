using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Configuration;
using System.Configuration;
using TVPPro.Configuration.Technical;
using Tvinci.Data.TVMDataLoader.Protocols;
using System.Threading;
using log4net;

namespace TVPApi.Configuration.Technical
{
	public class ApiTechnichalConfiguration : ConfigurationManager<TechnicalData>
	{
		private static ILog logger = log4net.LogManager.GetLogger(typeof(TechnicalConfiguration));

		ReaderWriterLockSlim m_locker = new ReaderWriterLockSlim();

		static object instanceLock = new object();
        

		public ApiTechnichalConfiguration()
		{
			base.DataModified = this.TechDataModified;
			base.SyncFromFile(ConfigurationManager.AppSettings["TVPPro.Configuration.Technical"], true);
            m_syncFile = ConfigurationManager.AppSettings["TVPPro.Configuration.Technical"];
		}

        public ApiTechnichalConfiguration(string syncFile)
        {
            base.DataModified = this.TechDataModified;
            base.SyncFromFile(syncFile, true);
            m_syncFile = syncFile;
        }
     

		public TechnicalData Config
		{
			get
			{
				return Data;
			}
		}

		public string GenerateConnectionString()
		{
			return string.Concat("Driver={SQL Server};Server=", Data.DBConfiguration.IP,
				";Database=", Data.DBConfiguration.DatabaseInstance,
				";Uid=", Data.DBConfiguration.User,
				";Pwd=", Data.DBConfiguration.Pass,
				";");
		}

		private TVMProtocolConfiguration m_TVMConfiguration;
		public TVMProtocolConfiguration TVMConfiguration
		{
			get
			{
				m_locker.EnterReadLock();

				try
				{
					if (m_TVMConfiguration == null)
					{
						return new TVMProtocolConfiguration(false, false, string.Empty, string.Empty);
					}

					return m_TVMConfiguration;
				}
				finally
				{
					m_locker.ExitReadLock();
				}
			}
			set
			{
				m_TVMConfiguration = value;
			}
		}

		//public static void Sync(EventHandler<ItemAddedEventArgs<TechnicalConfiguration>> itemAddedEvent)
		//{
		//    //if (itemAddedEvent != null)
		//    //{
		//    //    m_provider.ItemAddedEvent += itemAddedEvent;
		//    //}

		//    m_provider.SyncFromIndexFile(ConfigurationManager.AppSettings["TVP.Core.Configuration.Technical"], false, false);

		//    //if (itemAddedEvent != null)
		//    //{
		//    //    m_provider.ItemAddedEvent -= itemAddedEvent;
		//    //}
		//}

		private void TechDataModified(TechnicalData data)
		{
			logger.Info("Start handling technical configuration data changed");

			if (data == null)
			{
				logger.Info("Cannot extract data object.");
				return;
			}

			m_locker.EnterWriteLock();
			try
			{
				//CreateFunctionsStrings(data);

				// Create TVMConfiguration class
				m_TVMConfiguration = new TVMProtocolConfiguration(
					data.TVM.Configuration.ForceUpdatedData, data.TVM.Configuration.EnableTimer,
					data.TVM.Configuration.User, data.TVM.Configuration.Password);

				// set translation attributes
				//switch (data.Translation.ActionOnUnknownKey)
				//{
				//    case ActionOnUnknownKey.ShowKey:
				//        TextLocalization.NotExistsAction = eNotExistsAction.ShowKey;
				//        break;
				//    case ActionOnUnknownKey.ShowNothing:
				//        TextLocalization.NotExistsAction = eNotExistsAction.ShowEmptyString;
				//        break;
				//    case ActionOnUnknownKey.ShowHyphen:
				//    default:
				//        TextLocalization.NotExistsAction = eNotExistsAction.ShowHyphen;
				//        break;
				//}

				// Create localized pages hash table
				//m_localizedPages = new Hashtable();
				//for (int i = 0; i < data.Localization.LocalizedPageCollection.Count; i++)
				//{
				//    m_localizedPages.Add(data.Localization.LocalizedPageCollection[i].Value, true);
				//}

				// Create media types dictionaries
				//CreateMediaTypesDictionaries(data);

				// sync dynamic definitions
				//m_dynamicDefinitions.Clear();

				//foreach (Category category in data.DynamicDefinitions.CategoryCollection)
				//{
				//    foreach (CategoryItem item in category.CategoryItemCollection)
				//    {
				//        m_dynamicDefinitions[string.Concat(category.ID, "_", item.ID)] = item.Value;
				//    }
				//}
				logger.Info("Finished handling technical configuration data changed");
			}
			catch (Exception e)
			{
				m_TVMConfiguration = null;
				logger.Error("Error occured while handling technical configuration data changed", e);
			}
			finally
			{
				m_locker.ExitWriteLock();
			}
		}
	}
}
