using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Configuration;
using System.Configuration;
using TVPPro.Configuration.Technical;
using Tvinci.Configuration.ConfigSvc;
using Tvinci.Data.TVMDataLoader.Protocols;
using System.Threading;
using KLogMonitor;
using System.Reflection;


namespace TVPApi.Configuration.Technical
{
    public class ApiTechnichalConfiguration : ConfigurationManager<TechnicalData>
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
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


        public ApiTechnichalConfiguration(int nGroupID, string sPlatform, string sEnvironment)
            : base(eSource.Service)
        {
            SyncFromService(nGroupID, sPlatform, sEnvironment, eConfigType.Technical, CreateTechnicalConfig);
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
            //logger.Info("Start handling technical configuration data changed");

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
                //logger.Info("Finished handling technical configuration data changed");
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

        private TechnicalData CreateTechnicalConfig(IEnumerable<ConfigKeyVal> source)
        {
            TechnicalData retVal = new TechnicalData();

            retVal.DBConfiguration.User = DbConfigManager.GetValFromConfig(source, "DBConfiguration_User");
            retVal.DBConfiguration.Pass = DbConfigManager.GetValFromConfig(source, "DBConfiguration_Pass");
            retVal.DBConfiguration.IP = DbConfigManager.GetValFromConfig(source, "DBConfiguration_IP");
            retVal.DBConfiguration.DatabaseInstance = DbConfigManager.GetValFromConfig(source, "DBConfiguration_DatabaseInstance");
            retVal.TVM.Servers.MainServer.URL = DbConfigManager.GetValFromConfig(source, "TVM_Servers_MainServer_URL");
            retVal.TVM.Servers.MainServer.TVMReadURL = DbConfigManager.GetValFromConfig(source, "TVM_Servers_MainServer_TVMReadURL");
            retVal.TVM.Servers.MainServer.TVMWriteURL = DbConfigManager.GetValFromConfig(source, "TVM_Servers_MainServer_TVMWriteURL");

            retVal.TVM.Configuration.ForceUpdatedData = DbConfigManager.GetBoolFromConfig(source, "TVM_Configuration_ForceUpdateData");

            retVal.TVM.Configuration.EnableTimer = DbConfigManager.GetBoolFromConfig(source, "TVM_Configuration_EnableTimer");
            retVal.TVM.Configuration.User = DbConfigManager.GetValFromConfig(source, "TVM_Configuration_User");
            retVal.TVM.Configuration.Password = DbConfigManager.GetValFromConfig(source, "TVM_Configuration_Password");
            retVal.TVM.Configuration.EmbedUser = DbConfigManager.GetValFromConfig(source, "TVM_Configuration_EmbedUser");
            retVal.TVM.Configuration.EmberPassword = DbConfigManager.GetValFromConfig(source, "TVM_Configuration_EmberPassword");
            retVal.TVM.CachingServer.AllowedIPs = DbConfigManager.GetValFromConfig(source, "TVM_CachingServer_AllowedIPs");
            retVal.TVM.FlashVars.FileFormat = DbConfigManager.GetValFromConfig(source, "TVM_FlashVars_FileFormat");
            retVal.TVM.FlashVars.SubFileFormat = DbConfigManager.GetValFromConfig(source, "TVM_FlashVars_SubFileFormat");
            retVal.TVM.TVMRssURL = DbConfigManager.GetValFromConfig(source, "TVM_TVMRssURL");
            retVal.Translation.UseTranslatedMediaType = DbConfigManager.GetBoolFromConfig(source, "Translation_UseTranslatedMediaType");
            retVal.Translation.Culture = DbConfigManager.GetValFromConfig(source, "Translations_Culture");
            DbConfigManager.GetMultipleValsFromConfig(source, "Translations_CharacterReplace")
                .Select(
                    cr =>
                    retVal.Translation.CharacterReplace.Add(new Character()
                    {
                        OldChar = cr.Split(';')[0],
                        NewChar = cr.Split(';')[1]
                    })
                        );

            retVal.Player.MainFileFormat = DbConfigManager.GetValFromConfig(source, "Player_MainFileFormat");
            retVal.Player.TrailerFileFormat = DbConfigManager.GetValFromConfig(source, "Player_TrailerFileFormat");
            retVal.Player.TrickPlayFileFormat = DbConfigManager.GetValFromConfig(source, "Player_TrickPlayFileFormat");

            return retVal;
        }
    }
}
