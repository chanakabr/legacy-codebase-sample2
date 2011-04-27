using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.IO;
using System.Threading;
using TVPApi.Configuration.Technical;
using TVPApi.Configuration.Site;
using TVPApi.Configuration.Media;
using TVPApi.Configuration.PlatformServices;
using log4net;

/// <summary>
/// Summary description for ConfigurationManager
/// </summary>
/// 

namespace TVPApi
{
    public class ConfigManager
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(PageDataHelper));

        private static ConfigManager m_instance = null;

        private Dictionary<string, ConfigType> m_configs = new Dictionary<string,ConfigType>();
        private static ReaderWriterLockSlim m_ConfigManagerLocker = new ReaderWriterLockSlim();

        public struct ConfigType
        {
            public ApiTechnichalConfiguration TechnichalConfiguration;
            public ApiPlatformServicesConfiguration PlatformServicesConfiguration;
            public ApiSiteConfiguration SiteConfiguration;
            public ApiMediaConfiguration MediaConfiguration;
        }

        //Get specific config manager by group ID. If this is a new group ID - create new instances of platform configuration and Technichal c
        //configuration (sync from different files)
        public static ConfigManager GetInstance()
        {
            if (m_instance == null)
            {
                m_instance = new ConfigManager();
            }
            return m_instance;
        }

        //Init technichal and platform config (sync from file)
        public ConfigType GetConfig(int groupID, string platform)
        {
            string sKey = string.Concat(groupID.ToString(), platform);

            ConfigType configType = new ConfigType();
            bool bConfigExist = false;

            if (m_ConfigManagerLocker.TryEnterReadLock(1000))
            {
                try
                {
                    bConfigExist = m_configs.TryGetValue(sKey, out configType);
                }
                catch (Exception ex)
                {
                    logger.Error("GetConfig->", ex);
                }
                finally
                {
                    m_ConfigManagerLocker.ExitReadLock();
                }

            }

            if (!bConfigExist)
            {
                string parentDirectoryStr = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[groupID.ToString()]);
                string mediaConfigFile = string.Concat(parentDirectoryStr, "MediaConfiguration.config");
                string directoryStr = string.Concat(parentDirectoryStr, platform);

                if (!string.IsNullOrEmpty(directoryStr))
                {
                    string[] fileNames = Directory.GetFiles(directoryStr);
                    foreach (string file in fileNames)
                    {
                        if (file.Contains("Technical"))
                        {
                            configType.TechnichalConfiguration = new TVPApi.Configuration.Technical.ApiTechnichalConfiguration(file);
                        }
                        else if (file.Contains("Platform"))
                        {
                            configType.PlatformServicesConfiguration = new TVPApi.Configuration.PlatformServices.ApiPlatformServicesConfiguration(file);
                        }
                        else if (file.Contains("Site"))
                        {
                            configType.SiteConfiguration = new TVPApi.Configuration.Site.ApiSiteConfiguration(file);
                        }
                    }
                }

                configType.MediaConfiguration = new TVPApi.Configuration.Media.ApiMediaConfiguration(mediaConfigFile);


                if (m_ConfigManagerLocker.TryEnterWriteLock(1000))
                {
                    try
                    {
                        if (!m_configs.Keys.Contains(sKey))
                        {
                            m_configs.Add(sKey, configType);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("GetConfig->", ex);
                    }
                    finally
                    {
                        m_ConfigManagerLocker.ExitWriteLock();
                    }
                }
            }

            return configType;

            
        }

    }
}
